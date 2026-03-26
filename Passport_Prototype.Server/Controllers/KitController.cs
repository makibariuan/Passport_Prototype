using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using OnlineRegistration.Server.DTOs;
using OnlineRegistration.Server.Models;
using OnlineRegistration.Server.Services;
using OnlineRegistration.Server.Services.Interfaces;
using SeniorCitizen.Server.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static SeniorCitizen.Server.DTOs.DFADto;
using static System.Net.Mime.MediaTypeNames;

namespace OnlineRegistration.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    [Authorize]
    public class KitController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmployeesDbContext _db;
        private readonly AfisQueueService _afisQueue;
        private readonly IPgpService _pgpService;
        public KitController(AppDbContext context, EmployeesDbContext db, AfisQueueService afisQueue, IPgpService pgpService)
        {
            _context = context;
            _db = db;
            _afisQueue = afisQueue;
            _pgpService = pgpService;
        }


        // =========================================================================
        //                 CITIZEN/EMPLOYEE KIT ENDPOINTS
        // =========================================================================



        //        STATUS
        //1 capture
        //2 uploaded
        //3 afis hit
        //4 validated
        //5 generate file
        //6 id produced
        //7 active

        /// <summary>
        /// Retrieves applicant identity and initializes or updates a Biometric Enrollment record.
        /// </summary>
        [HttpPost("applicant")]
        [ProducesResponseType(typeof(ApplicantDetailDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ApplicantDetailDto>> GetApplicantDetails(
            [FromBody] SearchApplicantDetailDto dto)
        {
            // 1. Setup Identity
            var currentKitUser = User.FindFirst("username")?.Value ?? "Unknown";
            var currentUsername = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            if (string.IsNullOrWhiteSpace(dto.ApplicationCode))
            {
                return BadRequest(new { responseCode = -1, message = "ApplicationCode is required." });
            }

            // --- STEP 1: Unified Search in EnrollmentRegistryID ---
            // No more checking Citizens or Employees tables separately!
            var applicant = await _db.EnrollmentRegistries
                .FirstOrDefaultAsync(p => p.ApplicationCode == dto.ApplicationCode);

            if (applicant == null)
            {
                LogApplicantSearch("APPLICANT_SEARCH_FAILURE_NOT_FOUND", dto.ApplicationCode, "N/A", dto.KitName, currentUsername);
                await _context.SaveChangesAsync();
                return NotFound(new { responseCode = -1, message = "Application Code not found." });
            }

            // --- STEP 2: Handle Biometric Locking Logic ---
            // Check BiometricStatus instead of the old BDE table status
            // BiometricStatus 2 = Uploaded/Locked
            if (applicant.BiometricStatus >= 2)
            {
                LogApplicantSearch("BIOMETRIC_ASSIGNMENT_FAILURE_LOCKED", dto.ApplicationCode, currentKitUser, dto.KitName, currentUsername);
                await _context.SaveChangesAsync();
                return BadRequest(new { responseCode = -1, message = "Biometrics for this applicant are already locked." });
            }

            // --- STEP 3: Assignment & Progress Tracking ---
            // If BiometricStatus is 0 (Not Started), move it to 1 (In-Progress)
            string logDescription = (applicant.BiometricStatus == 0)
                ? "BIOMETRIC_ASSIGNMENT_INITIAL"
                : "BIOMETRIC_ASSIGNMENT_UPDATE";

            applicant.BiometricStatus = 1;      // Technical: Capture In-Progress
            applicant.KitUser = currentKitUser;
            applicant.KitName = dto.KitName;
            applicant.DateCapture = DateTime.Now; // Mark the start of capture

            // 4. Audit and Save
            LogApplicantSearch(logDescription, dto.ApplicationCode, currentKitUser, dto.KitName, currentUsername);
            await _db.SaveChangesAsync();

            return Ok(new ApplicantDetailDto
            {
                PersonId = applicant.Id, // Returning the Unified Table Primary Key
                ApplicationCode = applicant.ApplicationCode,
                FirstName = applicant.FirstName,
                LastName = applicant.LastName,
                GovIDImage = applicant.GovIDImage,
            });
        }
        void LogApplicantSearch(string description, string appCode, string kitUser, string kitName, string accessedByUsername = "Unknown")
        {
            var logEntry = new ApplicationLog
            {
                LogId = Guid.NewGuid().ToString("N"),
                UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (int?)null,
                LogDescription = description,
                LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                LogTime = DateTime.Now.ToString("HH:mm:ss"),
                Notes = $"App Code: {appCode}. Assigned Kit: {kitName}, Kit User: {kitUser}. Performed by: {accessedByUsername}"
            };
            _context.ApplicationLogs.Add(logEntry);
        }

        /// <summary>
        /// Uploads a PGP-encrypted file containing full biometric data.
        /// The ApplicationCode is extracted from the decrypted JSON payload.
        /// </summary>
        [HttpPut("applicant/biometrics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UploadUnifiedBiometrics(IFormFile file)
        {
            // 1. Basic File Validation
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No encrypted biometric file provided." });

            // 2. Identity and Authorization
            var currentUsername = User.FindFirst("username")?.Value;
            var currentKitUser = User.FindFirstValue(ClaimTypes.Name) ?? currentUsername ?? "Unknown";

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
                return Unauthorized(new { message = "User identity not found." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (user == null || user.UserRole != 3)
            {
                // Note: We can't log the specific AppCode yet because it's still encrypted
                LogBiometricUpload("BIOMETRIC_UPLOAD_UNAUTHORIZED", "PENDING_DECRYPTION", "N/A", "N/A", currentUsername, "Invalid Role");
                await _context.SaveChangesAsync();
                return BadRequest(new { message = "Access Denied: Kit Users only." });
            }

            try
            {
                // 3. Read the encrypted file content
                string encryptedPgp;
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    encryptedPgp = await reader.ReadToEndAsync();
                }

                // 4. Decrypt via PGP Service
                string decryptedJson = await _pgpService.DecryptAsync(encryptedPgp);

                // 5. Deserialize the inner JSON into the DTO
                var dto = JsonSerializer.Deserialize<UnifiedBiometricsDto>(decryptedJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dto == null || string.IsNullOrEmpty(dto.ApplicationCode))
                    return BadRequest(new { message = "Decryption failed or ApplicationCode is missing in the payload." });

                // 6. Fetch the Unified Record from Database using the code from the JSON
                var registryEntry = await _db.EnrollmentRegistries
                    .FirstOrDefaultAsync(p => p.ApplicationCode == dto.ApplicationCode);

                if (registryEntry == null)
                    return NotFound(new { message = $"Application record [{dto.ApplicationCode}] not found in registry." });

                // Lock Check: Don't allow upload if BiometricStatus is already 2 (Uploaded) or higher
                if (registryEntry.BiometricStatus >= 2)
                    return BadRequest(new { message = "Biometrics for this applicant are already locked/uploaded." });

                // 7. Prepare Timestamps (Philippine Time)
                var phTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
                DateTime dateUpload = TimeZoneInfo.ConvertTime(DateTime.UtcNow, phTimeZone);
                DateTime dateCapture = dto.DateCapture ?? DateTime.Now;

                // 8. Update the Record
                registryEntry.DateCapture = dateCapture;
                registryEntry.DateUpload = dateUpload;
                registryEntry.KitUser = dto.KitUser ?? currentKitUser;
                registryEntry.KitName = dto.KitName ?? "Mobile Kit";
                registryEntry.BiometricStatus = 2; // Technical: Uploaded

                // Base Biometrics
                if (!string.IsNullOrEmpty(dto.Photo)) registryEntry.Photo = dto.Photo;
                if (!string.IsNullOrEmpty(dto.Signature)) registryEntry.Signature = dto.Signature;
                if (!string.IsNullOrEmpty(dto.EyeLeft)) registryEntry.EyeLeft = dto.EyeLeft;
                if (!string.IsNullOrEmpty(dto.EyeRight)) registryEntry.EyeRight = dto.EyeRight;

                // Map RAW Fingerprints
                registryEntry.LeftThumb = dto.LeftThumb ?? registryEntry.LeftThumb;
                registryEntry.LeftIndex = dto.LeftIndex ?? registryEntry.LeftIndex;
                registryEntry.LeftMiddle = dto.LeftMiddle ?? registryEntry.LeftMiddle;
                registryEntry.LeftRing = dto.LeftRing ?? registryEntry.LeftRing;
                registryEntry.LeftSmall = dto.LeftSmall ?? registryEntry.LeftSmall;
                registryEntry.RightThumb = dto.RightThumb ?? registryEntry.RightThumb;
                registryEntry.RightIndex = dto.RightIndex ?? registryEntry.RightIndex;
                registryEntry.RightMiddle = dto.RightMiddle ?? registryEntry.RightMiddle;
                registryEntry.RightRing = dto.RightRing ?? registryEntry.RightRing;
                registryEntry.RightSmall = dto.RightSmall ?? registryEntry.RightSmall;

                // Map ISO Templates
                registryEntry.ISO_L_Thumb = dto.IsoLeftThumb;
                registryEntry.ISO_L_Index = dto.IsoLeftIndex;
                registryEntry.ISO_L_Middle = dto.IsoLeftMiddle;
                registryEntry.ISO_L_Ring = dto.IsoLeftRing;
                registryEntry.ISO_L_Small = dto.IsoLeftSmall;
                registryEntry.ISO_R_Thumb = dto.IsoRightThumb;
                registryEntry.ISO_R_Index = dto.IsoRightIndex;
                registryEntry.ISO_R_Middle = dto.IsoRightMiddle;
                registryEntry.ISO_R_Ring = dto.IsoRightRing;
                registryEntry.ISO_R_Small = dto.IsoRightSmall;

                // Map AFIS Templates
                registryEntry.AFIS_L_Thumb = dto.AfisLeftThumb;
                registryEntry.AFIS_L_Index = dto.AfisLeftIndex;
                registryEntry.AFIS_L_Middle = dto.AfisLeftMiddle;
                registryEntry.AFIS_L_Ring = dto.AfisLeftRing;
                registryEntry.AFIS_L_Small = dto.AfisLeftSmall;
                registryEntry.AFIS_R_Thumb = dto.AfisRightThumb;
                registryEntry.AFIS_R_Index = dto.AfisRightIndex;
                registryEntry.AFIS_R_Middle = dto.AfisRightMiddle;
                registryEntry.AFIS_R_Ring = dto.AfisRightRing;
                registryEntry.AFIS_R_Small = dto.AfisRightSmall;

                // 9. Audit Logging and Save
                LogBiometricUpload("KIT_USER_BIOMETRIC_BATCH_FILE_SUCCESS", dto.ApplicationCode, registryEntry.KitUser, registryEntry.KitName, currentUsername);

                await _db.SaveChangesAsync();
                return Ok(new { responseCode = 0, message = $"Biometrics for {dto.ApplicationCode} decrypted and stored successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing biometric file.", details = ex.Message });
            }
        }

        void LogBiometricUpload(
           string description,
           string appCode,
           string kitUser,
           string kitName,
           string accessedByUsername = "Unknown",
           string additionalNote = null)
        {
            // Extract UserID robustly from claims
            int? userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? (int?)id : null;

            // Construct the Notes field
            string notes = $"App Code: {appCode}. Kit: {kitName}, Kit User: {kitUser}. Performed by: {accessedByUsername}";

            if (!string.IsNullOrEmpty(additionalNote))
            {
                notes += $". {additionalNote}";
            }

            var logEntry = new ApplicationLog
            {
                LogId = Guid.NewGuid().ToString("N"),
                UserId = userId,
                LogDescription = description,
                LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                LogTime = DateTime.Now.ToString("HH:mm:ss"),
                Notes = notes
            };

            // Use the common context for logging
            _context.ApplicationLogs.Add(logEntry);
        }

        [HttpPost("download-encrypted")]
        public async Task<IActionResult> DownloadEncryptedPayload([FromBody] System.Text.Json.JsonElement payload)
        {
            try
            {
                // 1. Convert the JSON body to a raw string
                // This preserves the exact structure of your biometrics JSON
                string jsonString = payload.GetRawText();

                // 2. Encrypt it using your PgpService
                // This will use Packet 18 (MDC), which your Decrypt method likes
                string encryptedPgp = await _pgpService.EncryptAsync(jsonString);

                // 3. Prepare for download
                byte[] fileBytes = Encoding.UTF8.GetBytes(encryptedPgp);

                // Use the applicationCode from your JSON as the filename if possible
                string fileName = "biometrics_payload.asc";
                if (payload.TryGetProperty("applicationCode", out var code))
                {
                    fileName = $"{code.GetString()}_biometrics.asc";
                }

                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Logs a biometric bypass event (e.g., missing fingers/medical issues) and updates status.
        /// </summary>
        [HttpPut("set-bypass-logs/{id}")]
        public async Task<IActionResult> SetBiometricBypassStatusAndLog(int id, [FromBody] BypassLogDto dto)
        {
            var currentUsername = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";
            var currentKitUser = User.FindFirst("username")?.Value ?? "N/A";
            var stepNameToCheck = dto.StepName ?? "DEFAULT_STEP_NAME";

            // 1. Validations
            if (!DateTime.TryParse(dto.DateBypassed, out DateTime parsedDate))
            {
                LogBypassAttempt("BYPASS_FAILURE_INVALID_DATE", id, stepNameToCheck, dto.KitUser, dto.KitName, currentKitUser);
                await _context.SaveChangesAsync();
                return BadRequest(new { resposeCode = -1, message = "Invalid date format." });
            }

            if (string.IsNullOrWhiteSpace(dto.ReasonCode))
            {
                LogBypassAttempt("BYPASS_FAILURE_INVALID_REASON_CODE", id, stepNameToCheck, dto.KitUser, dto.KitName, currentKitUser);
                await _context.SaveChangesAsync();
                return BadRequest(new { resposeCode = -1, message = "ReasonCode is required." });
            }

            // 2. Data Retrieval - Unified to use _db (or _context if EnrollmentRegistries is there)
            // CRITICAL: Ensure registryEntry is loaded from the SAME context that starts the transaction.
            var registryEntry = await _db.EnrollmentRegistries.FindAsync(id);
            if (registryEntry == null) return NotFound(new { message = "Registry Record not found" });

            // 3. Begin Transaction on _db since it owns the main record
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // 4. Check for existing log
                // Note: If BypassLogs is in _context, you must move it to _db or vice versa for this transaction.
                var bypassLog = await _context.BypassLogs
                    .FirstOrDefaultAsync(p => p.BDEID == id && p.StepName == stepNameToCheck);

                if (bypassLog != null)
                {
                    LogBypassAttempt("BYPASS_ALREADY_EXISTS", id, stepNameToCheck, currentKitUser, dto.KitName, currentUsername);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new { resposeCode = 1, message = "Bypass already logged." });
                }

                // 5. Create New Bypass Log
                var newLog = new BypassLog
                {
                    BDEID = id,
                    StepName = stepNameToCheck,
                    ReasonCode = dto.ReasonCode,
                    ReasonDetails = dto.ReasonDetails,
                    Image = dto.Image,
                    DateBypassed = parsedDate,
                    KitName = dto.KitName,
                    KitUser = currentKitUser
                };
                _context.BypassLogs.Add(newLog);

                // 6. Update Unified Registry Status
                registryEntry.BiometricBypass = true;
                if (registryEntry.BiometricStatus < 1) registryEntry.BiometricStatus = 1;

                // 7. Success Logging
                LogBypassAttempt("BYPASS_SUCCESS", id, stepNameToCheck, currentKitUser, dto.KitName, currentUsername);

                // 8. Save both contexts (If they share the same database, this is safer)
                await _context.SaveChangesAsync();
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { resposeCode = 0, message = "Bypass logged successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error saving bypass", details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        void LogBypassAttempt(string description, int bdeId, string stepname, string kitUser, string kitName, string accessedByUsername = "Unknown")
        {
            var logEntry = new ApplicationLog
            {
                LogId = Guid.NewGuid().ToString("N"),
                UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var logUserId) ? logUserId : (int?)null,
                LogDescription = description,
                LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                LogTime = DateTime.Now.ToString("HH:mm:ss"),
                Notes = $"BDE ID: {bdeId}. Bypass set with Step Name: {stepname}. Kit: {kitName}, Kit User: {kitUser}. Performed by: {accessedByUsername}"
            };
            _context.ApplicationLogs.Add(logEntry);
        }
      

        /// <summary>
        /// Finalizes the capture process, locks the record, and enqueues fingerprints for AFIS processing.
        /// </summary>
        [HttpPut("applicant/biometrics-lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> FinalizeUnifiedCapture([FromBody] BiometricDataEnrollmentDto dto)
        {
            var currentUsername = User.FindFirst("username")?.Value ?? "N/A";

            if (string.IsNullOrEmpty(dto.ApplicationCode))
                return BadRequest(new { responseCode = -1, message = "ApplicationCode is required." });

            // 1. Fetch Unified Record
            var registryEntry = await _db.EnrollmentRegistries
                .FirstOrDefaultAsync(p => p.ApplicationCode == dto.ApplicationCode);

            if (registryEntry == null)
            {
                LogBiometricLock("BIOMETRIC_LOCK_FAILURE_NOT_FOUND", dto.ApplicationCode, dto.KitUser ?? "N/A", dto.KitName ?? "N/A", currentUsername);
                await _db.SaveChangesAsync();
                return NotFound(new { message = "Application record not found in registry." });
            }

            // 2. Setup Timestamps (Philippine Time)
            var phTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
            DateTime captureTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, phTimeZone);

            // 3. AFIS Queueing Logic 
            // We check the standard ISO/RAW columns in the registry record
            var fingerprints = new Dictionary<int, string?>
    {
        { 1, registryEntry.LeftThumb },  { 2, registryEntry.LeftIndex }, { 3, registryEntry.LeftMiddle },
        { 4, registryEntry.LeftRing },   { 5, registryEntry.LeftSmall },
        { 6, registryEntry.RightThumb }, { 7, registryEntry.RightIndex }, { 8, registryEntry.RightMiddle },
        { 9, registryEntry.RightRing },  { 10, registryEntry.RightSmall }
    };

            int fingerprintsQueuedCount = 0;

            // Use registryEntry.Id as the identifier for AFIS jobs
            foreach (var fp in fingerprints.Where(f => !string.IsNullOrEmpty(f.Value)))
            {
                _afisQueue.Enqueue(new AfisJob
                {
                    PersonId = registryEntry.Id,
                    FingerPosition = fp.Key,
                    Base64Image = fp.Value
                });
                fingerprintsQueuedCount++;
            }

            // 4. Determine Status based on your Unified Status Map
            // Status 2 = Captured/Waiting for AFIS
            // Status 4 = Validated (Auto-cleared if no fingerprints exist)
            int targetStatus = (fingerprintsQueuedCount > 0) ? 2 : 4;

            // 5. Update the Record
            registryEntry.DateCapture = captureTime;
            registryEntry.BiometricStatus = targetStatus; // Technical Status
            registryEntry.KitUser = dto.KitUser ?? currentUsername;
            registryEntry.KitName = dto.KitName ?? "Mobile Kit";

            // 6. Save with Transaction safety
            try
            {
                LogBiometricLock(targetStatus == 2 ? "KIT_LOCK_SUCCESS_AFIS" : "KIT_LOCK_SUCCESS_CLEARED",
                    dto.ApplicationCode, dto.KitUser, dto.KitName, currentUsername);

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    responseCode = 0,
                    status = targetStatus,
                    message = fingerprintsQueuedCount > 0 ? "Enqueued for AFIS." : "Bypassed AFIS (No fingerprints)."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Database Sync Error", details = ex.Message });
            }
        }

        void LogBiometricLock(string description, string appCode, string kitUser, string kitName, string accessedByUsername = "Unknown")
        {
            var logEntry = new ApplicationLog
            {
                LogId = Guid.NewGuid().ToString("N"),
                UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (int?)null,
                LogDescription = description,
                LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                LogTime = DateTime.Now.ToString("HH:mm:ss"),
                Notes = $"App Code: {appCode}. Locking Kit: {kitName}, Kit User: {kitUser}. Performed by: {accessedByUsername}"
            };
            _context.ApplicationLogs.Add(logEntry);
        }


        /// <summary>
        /// Processes a PGP-encrypted JSON dump from a kit. 
        /// Backs up the encrypted file to disk and updates the database with extracted ICAO/AFIS templates.
        /// </summary>
        [HttpPost("upload-json-dump")]
        public async Task<IActionResult> UploadAndProcessBiometricDump(IFormFile file, [FromForm] string applicationCode)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                // 1. Read the raw JSON
                string rawJson;
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    rawJson = await reader.ReadToEndAsync();
                }

                // 2. Encrypt the raw content for secure backup
                string encryptedPgp = await _pgpService.EncryptAsync(rawJson);

                // 3. SAVE TO LOCAL FOLDER (Secure Archive)
                string backupFolder = Path.Combine(AppContext.BaseDirectory, "BiometricBackups");
                if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);

                // Unique filename: APPCODE_TIMESTAMP.pgp
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{applicationCode}_{timestamp}.pgp";
                string fullPath = Path.Combine(backupFolder, fileName);

                await System.IO.File.WriteAllTextAsync(fullPath, encryptedPgp);

                // 4. DECRYPT to process for the Database update
                string decryptedJson = await _pgpService.DecryptAsync(encryptedPgp);
                var templates = JsonSerializer.Deserialize<BiometricTemplateDataDto>(decryptedJson);

                // 5. Database Logic - Pointing to the Unified Table
                var registryEntry = await _db.EnrollmentRegistries
                    .FirstOrDefaultAsync(e => e.ApplicationCode == applicationCode);

                if (registryEntry == null) return NotFound("Application code not found in registry.");

                // 6. Mapping ISO Templates (Renamed from ICAO)
                registryEntry.ISO_L_Thumb = templates.ISO_LeftThumb;
                registryEntry.ISO_L_Index = templates.ISO_LeftIndex;
                registryEntry.ISO_L_Middle = templates.ISO_LeftMiddle;
                registryEntry.ISO_L_Ring = templates.ISO_LeftRing;
                registryEntry.ISO_L_Small = templates.ISO_LeftSmall;
                registryEntry.ISO_R_Thumb = templates.ISO_RightThumb;
                registryEntry.ISO_R_Index = templates.ISO_RightIndex;
                registryEntry.ISO_R_Middle = templates.ISO_RightMiddle;
                registryEntry.ISO_R_Ring = templates.ISO_RightRing;
                registryEntry.ISO_R_Small = templates.ISO_RightSmall;

                // 7. Mapping AFIS Templates
                registryEntry.AFIS_L_Thumb = templates.AFIS_LeftThumb;
                registryEntry.AFIS_L_Index = templates.AFIS_LeftIndex;
                registryEntry.AFIS_L_Middle = templates.AFIS_LeftMiddle;
                registryEntry.AFIS_L_Ring = templates.AFIS_LeftRing;
                registryEntry.AFIS_L_Small = templates.AFIS_LeftSmall;
                registryEntry.AFIS_R_Thumb = templates.AFIS_RightThumb;
                registryEntry.AFIS_R_Index = templates.AFIS_RightIndex;
                registryEntry.AFIS_R_Middle = templates.AFIS_RightMiddle;
                registryEntry.AFIS_R_Ring = templates.AFIS_RightRing;
                registryEntry.AFIS_R_Small = templates.AFIS_RightSmall;

                // 8. Update Status
                // BiometricStatus 2 = Uploaded/Encrypted
                registryEntry.BiometricStatus = 2;
                registryEntry.DateUpload = DateTime.Now;

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "File processed, encrypted for backup, and database updated.",
                    appCode = applicationCode,
                    backupFile = fileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal processing error: {ex.Message}");
            }
        }


        /// <summary>
        /// Returns the raw PGP Public Key for the Mobile Kit to use for encryption.
        /// </summary>
        [HttpGet("public-key")]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPublicKey()
        {
            try
            {
                // 1. Locate the public key file in your project directory
                // Ensure your Keys folder is in the root and 'public.asc' is set to "Copy to Output Directory"
                string publicKeyPath = Path.Combine(AppContext.BaseDirectory, "Keys", "public.asc");

                if (!System.IO.File.Exists(publicKeyPath))
                {
                    return NotFound("Public key file not found on server.");
                }

                // 2. Read the raw text from the file
                string publicKeyText = await System.IO.File.ReadAllTextAsync(publicKeyPath);

                // 3. Return as raw text (Content-Type: text/plain)
                return Content(publicKeyText, "text/plain");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error retrieving public key: {ex.Message}");
            }
        }
        

        /// <summary>
        /// Packages the captured photo and signature into a ZIP archive for manual download.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("download-picture-and-signature/{applicationCode}")]
        public async Task<IActionResult> DownloadPictureAndSignature(string applicationCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationCode))
                {
                    return BadRequest("Application Code is required.");
                }

                // 1. Fetch the data from the database
                var bde = await _context.BiometricEnrollments
                    .Where(e => e.ApplicationCode == applicationCode)
                    .Select(e => new { e.Photo, e.Signature })
                    .FirstOrDefaultAsync();

                if (bde == null || (string.IsNullOrEmpty(bde.Photo) && string.IsNullOrEmpty(bde.Signature)))
                {
                    return NotFound(new { message = "No photo or signature found for this application." });
                }

                // Create the memory stream
                var memoryStream = new MemoryStream();

                // 2. Create the ZIP file
                // Note: 'leaveOpen: true' allows us to use the memoryStream after the 'using' block
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    // --- Process Photo ---
                    if (!string.IsNullOrEmpty(bde.Photo))
                    {
                        byte[] photoBytes = Convert.FromBase64String(CleanBase64(bde.Photo));
                        var photoEntry = archive.CreateEntry($"{applicationCode}_photo.jpg");
                        using (var entryStream = photoEntry.Open())
                        {
                            await entryStream.WriteAsync(photoBytes, 0, photoBytes.Length);
                        }
                    }

                    // --- Process Signature ---
                    if (!string.IsNullOrEmpty(bde.Signature))
                    {
                        byte[] sigBytes = Convert.FromBase64String(CleanBase64(bde.Signature));
                        var sigEntry = archive.CreateEntry($"{applicationCode}_signature.png");
                        using (var entryStream = sigEntry.Open())
                        {
                            await entryStream.WriteAsync(sigBytes, 0, sigBytes.Length);
                        }
                    }
                } // <--- Archive is disposed here, finalizing the ZIP footer

                // 3. Reset stream position for the File result
                memoryStream.Position = 0;

                string zipName = $"{applicationCode}_Assets_{DateTime.Now:yyyyMMdd}.zip";

                // Return FileStreamResult which handles disposal of memoryStream automatically
                return File(memoryStream, "application/zip", zipName);
            }
            catch (Exception ex)
            {
                // Log the error here (e.g., _logger.LogError(ex, "Download failed"))
                return StatusCode(500, new { error = $"Download failed: {ex.Message}" });
            }
        }

        // Helper method used by the controller
        private string CleanBase64(string base64String)
        {
            if (string.IsNullOrWhiteSpace(base64String)) return string.Empty;

            // 1. Remove Data URL prefix (e.g., "data:image/jpeg;base64,")
            string cleaned = base64String.Contains(",") ? base64String.Split(',')[1] : base64String;

            // 2. Remove whitespace, newlines, tabs, and carriage returns
            // This is the most common cause of 500 errors during conversion
            return cleaned.Trim()
                          .Replace("\n", "")
                          .Replace("\r", "")
                          .Replace("\t", "")
                          .Replace(" ", "");
        }
    }
}
