using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data; // Adjust to your actual namespace
using OnlineRegistration.Server.Models;
using OnlineRegistration.Server.Services.Interfaces;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Security.Claims;
using ZXing.Common;
using ZXing;
using static OnlineRegistration.Server.Controllers.EmployeeController;
using Microsoft.Data.SqlClient;
using System.Data;

namespace OnlineRegistration.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CitizenController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmployeesDbContext _db;
        private readonly IEmailQueue _emailQueue;
        private readonly IConfiguration _config;

        public CitizenController(AppDbContext context, IEmailQueue emailQueue, IConfiguration config, EmployeesDbContext db)
        {
            _context = context; //database
            _emailQueue = emailQueue;
            _config = config;
            _db = db;
        }

        /// <summary>
        /// Submits an appointment request and automatically approves it via Stored Procedure.
        /// </summary>
        [HttpPost("my-schedule/request-auto")]
        
        public async Task<IActionResult> RequestAndAutoApprove([FromBody] MyScheduleRequest request)
        {
            // 1. Identify User
            var userIdClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            try
            {
                // 2. Prepare Code and Parameters
                string newAppCode = GenerateApplicationCode();

                // CHANGE: Direction must be InputOutput to send the new code AND receive the final one
                var appCodeParam = new SqlParameter("@ApplicationCode", SqlDbType.NVarChar, 50)
                {
                    Value = (object)newAppCode ?? DBNull.Value,
                    Direction = ParameterDirection.InputOutput
                };

                var firstNameParam = new SqlParameter("@FirstName", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                var emailParam = new SqlParameter("@UserEmail", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };

                // 3. Execute DB Logic
                // NOTE: Ensure the SP name matches "sp_RequestAndAutoApprove"
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[sp_RequestAndAutoApprove] @PersonID, @PreferredDate, @ApplicationCode OUTPUT, @FirstName OUTPUT, @UserEmail OUTPUT",
                    new SqlParameter("@PersonID", userId),
                    new SqlParameter("@PreferredDate", request.PreferredDate),
                    appCodeParam, // Use the parameter object here
                    firstNameParam,
                    emailParam);

                // 4. Extract Final Values
                string appCode = appCodeParam.Value.ToString();
                string email = emailParam.Value.ToString();
                string firstName = firstNameParam.Value.ToString();

                // 5. Side Effects: Barcode and Email
                GenerateBarcodeFile(appCode);

                var publicUrl = $"{_config["BarcodeSettings:PublicBaseUrl"]}/{appCode}.png";
                _emailQueue.QueueEmail(new EmailMessage
                {
                    To = email,
                    Subject = "Your Appointment is Confirmed",
                    Body = $@"<html><body>
                        <h2>Appointment Confirmed!</h2>
                        <p>Hello {firstName}, your biometric capture is scheduled for:</p>
                        <p><strong>{request.PreferredDate:MMMM dd, yyyy}</strong></p>
                        <img src='{publicUrl}' width='300' height='100' />
                        <p>Application Code: {appCode}</p>
                      </body></html>"
                });

                return Ok(new
                {
                    message = "Appointment scheduled and confirmed successfully.",
                    applicationCode = appCode,
                    date = request.PreferredDate
                });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        private void GenerateBarcodeFile(string appCode)
        {
            var barcodeWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions { Width = 300, Height = 100, Margin = 10 }
            };

            string barcodeFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "barcodes");
            if (!Directory.Exists(barcodeFolder)) Directory.CreateDirectory(barcodeFolder);

            var pixelData = barcodeWriter.Write(appCode);
            using var image = new Image<Rgba32>(pixelData.Width, pixelData.Height);
            // ... (Pixel loop logic omitted for brevity, same as original)
            image.Save(Path.Combine(barcodeFolder, $"{appCode}.png"), new PngEncoder());
        }

        /// <summary>
        /// Retrieves the current schedule status for the logged-in citizen.
        /// </summary>
        [HttpGet("my-schedule/date")]
        public async Task<IActionResult> GetScheduleDate()
        {
            var userIdClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            try
            {
                // Direct LINQ query is the "Best Practice" here.
                // It bypasses the Stored Procedure's requirement for matching ALL columns.
                var schedule = await _db.EnrollmentRegistries
                    .AsNoTracking()
                    .Where(a => a.PersonID == userId)
                    .Select(a => new
                    {
                        a.DateSchedule,
                        a.Status,
                        a.ApplicationCode
                    })
                    .FirstOrDefaultAsync();

                if (schedule == null)
                {
                    return NotFound(new { message = "Citizen application record not found." });
                }

                return Ok(schedule);
            }
            catch (Exception ex)
            {
                // This will show you exactly what's wrong in your Debug console
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");

                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving your schedule.",
                    error = ex.Message
                });
            }
        }

        private string GenerateApplicationCode()
        {
            // Example: MKT-20251011-ABC123
            return $"MKT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }
    }
}