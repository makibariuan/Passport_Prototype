using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using OnlineRegistration.Server.DTOs;
using OnlineRegistration.Server.Models;
using OnlineRegistration.Server.Services.Interfaces;
using SeniorCitizen.Server.DTOs;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
namespace OnlineRegistration.Server.Controllers
{

    /// <summary>
    /// AUTHORIZATION POLICY REFERENCE
    /// -----------------------------
    /// This table documents how the numeric UserRole claim values map to defined authorization policies.
    /// </summary>
    /// 
    /*
    | RoleID (Claim Value) | RoleDesc     |                         Policies Granted Access To (Direct/Inherited)                        |
    |:---------------------|:-------------|:---------------------------------------------------------------------------------------------|
    | 1                    | Super Admin  | RequireSuperAdmin, RequireManagementAccess, RequireAdministrativeView, RequireEmployeeAccess |
    | 2                    | System User  | RequireManagementAccess, RequireAdministrativeView, RequireEmployeeAccess                    |
    | 3                    | Kit Operator | (None defined)                                                                               |
    | 4                    | Employee     | RequireEmployeeAccess (If Role 4 is the Employee role)                                       |
    | 5                    | Citizen      | RequireCitizenAccess (If Role 5 is the Citizen role)                                         |
    | 6                    | HR           | RequireAdministrativeView                                                                    |
    */

    // Note: Management/Admin roles (1 & 2) typically inherit access to lower-level policies like EmployeeAccess.
    // Please ensure the code reflects the intended access for Roles 4 and 5 based on your last configuration.

    [ApiController]
    [Route("api/[controller]")]

    //[Authorize(Policy = "RequireManagementAccess")] // ROLES: 1 = SuperAdmin, 2 = SystemUser
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPgpService _pgpService;

        public AdminController(AppDbContext context, IPgpService pgpService)
        {
            _context = context;
            _pgpService = pgpService;
        }

        // --- Helper method for generating secure temporary passwords ---
        private (string plainPassword, string hashedPassword) GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            return (password, hash);
        }

        //        USER TYPE                 USER ROLE
        //        1 - System                1 - Super Admin
        //        2 - Kit                   2 - System User
        //                                  3 - Kit User


        //---------------------------------------------------
        //--------User Security & PGP (Create User)----------
        //---------------------------------------------------

        /// <summary>
        /// Creates a new System or Kit user and handles credential encryption.
        /// </summary>
        /// <remarks>
        /// **Security Logic:**
        /// - **Kit Users (Type 2):** Credentials are automatically encrypted into a PGP block.
        /// - **System Users (Type 1):** Returns a plain-text temporary password for initial setup.
        /// </remarks>
        /// <response code="200">User created. Check 'pgpData' for Kit users or 'initialPassword' for System users.</response>
        /// <response code="400">Username conflict or invalid role.</response>
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateSystemUser([FromBody] UserCreateDto dto)
        {
            // C# Logic: Security & Credentials
            string plainPassword;
            string hashedPassword;

            // 1. Check if the DTO has a password. If not, THEN generate a random one.
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                plainPassword = dto.Password;
                hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            }
            else
            {
                (plainPassword, hashedPassword) = GenerateTemporaryPassword();
            }

            var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(adminIdClaim, out int adminId)) adminId = 0;

            // PGP Encryption logic removed from here

            try
            {
                var newIdParam = new Microsoft.Data.SqlClient.SqlParameter
                {
                    ParameterName = "@NewId",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output
                };

                // Executing stored procedure - passing null for @KitEncryptedPgpData
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                  EXEC [dbo].[sp_CreateSystemUser] 
                      @UserType = {dto.UserType}, 
                      @UserRole = {dto.UserRole}, 
                      @Username = {dto.Username}, 
                      @FirstName = {dto.FirstName}, 
                      @LastName = {dto.LastName}, 
                      @PasswordHash = {hashedPassword}, 
                      @KitEncryptedPgpData = {null}, 
                      @AccessedByUserId = {adminId}, 
                      @NewId = {newIdParam} OUTPUT");

                return Ok(new
                {
                    message = "System User created successfully.",
                    userId = (int)newIdParam.Value,
                    username = dto.Username,
                    initialPassword = plainPassword // Always returning plainPassword for the admin to see/provide to user
                });
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //---------------------------------------------------
        //-------System Statistics (Counters)----------------
        //---------------------------------------------------

        /// <summary>
        /// Retrieves a paginated list of system or kit users.
        /// </summary>
        [HttpGet("user-list/{userType}")]
        public async Task<IActionResult> GetSystemUsers(string userType, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // 1. Validation Logic
            userType = userType.ToLowerInvariant();
            int targetUserType = userType switch
            {
                "system" => 1,
                "kit" => 2,
                _ => 0
            };

            var adminUsername = User.FindFirstValue("username") ?? "Unknown";

            if (targetUserType == 0)
            {
                LogUserListAccess("INVALID_LIST_ACCESS_ATTEMPT", page, pageSize, adminUsername);
                await _context.SaveChangesAsync();
                return BadRequest(new { message = "Invalid UserType. Must be 'system' or 'kit'." });
            }

            // 2. Data Fetching (Stored Procedure)
            var users = await _context.UserReadDtos
                .FromSqlInterpolated($@"EXEC [dbo].[sp_GetSystemUsers] 
                                @UserType = {targetUserType}, 
                                @PageNumber = {page}, 
                                @PageSize = {pageSize}")
                .ToListAsync();

            // 3. Log the Success
            string logType = (targetUserType == 1) ? "LIST_OF_SYSTEM_USERS" : "LIST_OF_KIT_USERS";
            LogUserListAccess(logType, page, pageSize, adminUsername);

            // One single SaveChanges to push the log to the DB
            await _context.SaveChangesAsync();

            return Ok(users);
        }
        void LogUserListAccess(string userType, int page, int pageSize, string accessedByUsername)
        {
            var logEntry = new ApplicationLog
            {
                LogId = Guid.NewGuid().ToString("N"),
                // Assume you can get the current User's ID from claims/context
                // For simplicity, we'll use a placeholder or 0 if not available
                UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (int?)null,
                LogDescription = "USER_LIST_ACCESS",
                LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                LogTime = DateTime.Now.ToString("HH:mm:ss"),
                Notes = $"User {accessedByUsername} accessed '{userType}' list. Page: {page}, Size: {pageSize}"
            };
            _context.ApplicationLogs.Add(logEntry);
        }

        /// <summary>
        /// Retrieves a paginated and searchable list of applicants from the Biometric Data Enrollment Database.
        /// </summary>
        [HttpGet("applicant-list")]
        public async Task<ActionResult<IEnumerable<ApplicantReadDto>>> GetBdeList(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                // 1. Get Admin ID from JWT Claims
                var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // If no ID is found in token (unauthorized), you might want to return 401
                if (string.IsNullOrEmpty(adminIdClaim)) return Unauthorized("Admin session expired.");

                int adminId = int.Parse(adminIdClaim);

                // Basic Pagination Guard
                page = page < 1 ? 1 : page;
                pageSize = pageSize < 1 ? 50 : pageSize;

                // 2. Execute Updated SP
                // Using FromSqlInterpolated helps prevent SQL injection
                var results = await _context.ApplicantReadDtos
                    .FromSqlInterpolated($@"EXEC [dbo].[sp_GetApplicantList] 
                                    @Search = {search}, 
                                    @PageNumber = {page}, 
                                    @PageSize = {pageSize},
                                    @AccessedByUserId = {adminId}")
                    .ToListAsync();

                // 3. Optional: Add a custom Header for total count if your interns want to build pagination UI
                return Ok(results);
            }
            catch (Exception ex)
            {
                // Log the actual exception for debugging
                return StatusCode(500, new { message = "Failed to retrieve applicant list.", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets a summary count of all users and active sessions.
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(StatisticCountDto), 200)]
        public async Task<ActionResult<StatisticCountDto>> GetCounts()
        {
            try
            {
                // Execute the stored procedure and retrieve the first (and only) row
                var stats = await _context.Database
                    .SqlQueryRaw<StatisticCountDto>("EXEC [dbo].[sp_GetUserStatistics]")
                    .ToListAsync();

                var result = stats.FirstOrDefault();

                if (result == null)
                {
                    return Ok(new StatisticCountDto()); // Return zeros if table is empty
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Database Error: {ex.Message}");
            }
        }


        //---------------------------------------------------
        //------User Administration (Password & Assignment)--
        //---------------------------------------------------

        /// <summary>
        /// Forces a password reset for a specific user and generates a temporary credential.
        /// </summary>
        [HttpPost("reset-user-password")]
        public async Task<IActionResult> ResetUserPassword([FromBody] ResetUserPasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Generate new temporary credentials in C#
            var (plainPassword, hashedPassword) = GenerateTemporaryPassword();

            // 2. Extract Admin Info from Claims
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int adminId = int.TryParse(adminIdClaim, out var id) ? id : 0;
            var adminUsername = User.FindFirst("username")?.Value ?? "Unknown";

            try
            {
                // 3. Execute the Stored Procedure
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC [dbo].[sp_ResetUserPassword]
                @Username = {request.Username},
                @NewPasswordHash = {hashedPassword},
                @ResetToken = {plainPassword},
                @AccessedByUserId = {adminId},
                @AccessedByUsername = {adminUsername}");

                return Ok(new
                {
                    message = $"Password for user '{request.Username}' has been successfully reset.",
                    resetToken = plainPassword,
                    userMustChangePassword = true
                });
            }
            catch (SqlException ex)
            {
                // If SQL throws the 50003 error (User not found)
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Resets an application's assignment, moving it back to an unassigned/kit-ready state.
        /// </summary>
        [HttpPut("reset-to-kit")]
        public async Task<IActionResult> ReassignApplication([FromBody] ReassignApplicationDto request)
        {
            // 1. Basic Validation
            if (string.IsNullOrWhiteSpace(request.ApplicationCode))
            {
                return BadRequest("Application Code is required to clear the assignment.");
            }

            // 2. Extract Admin Info
            var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int adminId = int.TryParse(adminIdClaim, out var id) ? id : 0;
            var adminUsername = User.FindFirst("username")?.Value ?? "Unknown";

            try
            {
                // 3. Execute Stored Procedure
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
            EXEC [dbo].[sp_ResetApplicationAssignment]
                @ApplicationCode = {request.ApplicationCode},
                @AccessedByUserId = {adminId},
                @AccessedByUsername = {adminUsername}");

                return Ok(new
                {
                    message = "Application successfully cleared and reset to unassigned status.",
                    applicationCode = request.ApplicationCode,
                });
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                // SQL Throws 50006 if App Code is not found
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Error: {ex.Message}");
            }
        }


        //---------------------------------------------------
        //--------User Administration (Status & Roles)-------
        //---------------------------------------------------

        /// <summary>
        /// Toggles a user between Active and Inactive status.
        /// </summary>
        [HttpPost("toggle-status")]
        public async Task<IActionResult> ToggleUserStatus([FromBody] ToggleUserStatusRequest request)
        {
            // 1. Extract Admin Info
            var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int adminId = int.TryParse(adminIdClaim, out var id) ? id : 0;
            var adminUsername = User.FindFirst("username")?.Value ?? "Unknown";

            try
            {
                // 2. Prepare Output Parameters
                var newStatusParam = new Microsoft.Data.SqlClient.SqlParameter
                {
                    ParameterName = "@NewStatus",
                    SqlDbType = System.Data.SqlDbType.Bit,
                    Direction = System.Data.ParameterDirection.Output
                };
                var usernameParam = new Microsoft.Data.SqlClient.SqlParameter
                {
                    ParameterName = "@TargetUsername",
                    SqlDbType = System.Data.SqlDbType.NVarChar,
                    Size = 100,
                    Direction = System.Data.ParameterDirection.Output
                };

                // 3. Execute SP
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC [dbo].[sp_ToggleUserStatus]
                @TargetUserId = {request.UserId},
                @AccessedByUserId = {adminId},
                @AccessedByUsername = {adminUsername},
                @NewStatus = {newStatusParam} OUTPUT,
                @TargetUsername = {usernameParam} OUTPUT");

                bool isActive = (bool)newStatusParam.Value;
                string action = isActive ? "activated" : "deactivated";

                return Ok(new
                {
                    message = $"User '{usernameParam.Value}' (ID: {request.UserId}) has been successfully {action}.",
                    isActive = isActive
                });
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Changes a user's role (Permissions). Checks for SuperAdmin restrictions in SQL.
        /// </summary>
        [HttpPut("update-user-role")]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleRequestDto request)
        {
            // 1. Extract Admin Claims
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int adminId = int.TryParse(adminIdClaim, out var id) ? id : 0;
            var adminUsername = User.FindFirst("username")?.Value ?? "Unknown";

            // Get the current Admin's Role to pass to the SP for the SuperAdmin check
            int adminRoleId = int.TryParse(User.FindFirstValue("UserRole"), out var rId) ? rId : -1;

            try
            {
                var oldRoleNameParam = new Microsoft.Data.SqlClient.SqlParameter { ParameterName = "@OldRoleName", SqlDbType = System.Data.SqlDbType.NVarChar, Size = 100, Direction = System.Data.ParameterDirection.Output };
                var newRoleNameParam = new Microsoft.Data.SqlClient.SqlParameter { ParameterName = "@NewRoleName", SqlDbType = System.Data.SqlDbType.NVarChar, Size = 100, Direction = System.Data.ParameterDirection.Output };
                var targetUsernameParam = new Microsoft.Data.SqlClient.SqlParameter { ParameterName = "@TargetUsername", SqlDbType = System.Data.SqlDbType.NVarChar, Size = 100, Direction = System.Data.ParameterDirection.Output };

                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC [dbo].[sp_UpdateUserRole]
                @TargetUserId = {request.Id},
                @NewRoleId = {request.NewUserRole},
                @AccessedByUserId = {adminId},
                @AccessedByUsername = {adminUsername},
                @AccessingUserRoleId = {adminRoleId},
                @OldRoleName = {oldRoleNameParam} OUTPUT,
                @NewRoleName = {newRoleNameParam} OUTPUT,
                @TargetUsername = {targetUsernameParam} OUTPUT");

                // Check if the SP returned without updating because roles were identical
                if (oldRoleNameParam.Value != DBNull.Value && newRoleNameParam.Value != DBNull.Value &&
                    oldRoleNameParam.Value.ToString() == newRoleNameParam.Value.ToString())
                {
                    return Ok(new { message = "No change needed. User already has this role." });
                }

                return Ok(new
                {
                    message = $"User '{targetUsernameParam.Value}' role successfully changed to '{newRoleNameParam.Value}'.",
                    userId = request.Id,
                    newRole = newRoleNameParam.Value
                });
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                // Catches THROW 50007, 50008, or 50009
                return BadRequest(new { message = ex.Message });
            }
        }


        //---------------------------------------------------
        //-----------Logistics & Quota Management------------
        //---------------------------------------------------

        /// <summary>
        /// Sets or updates the maximum number of appointments allowed for a specific site and time slot.
        /// </summary>
        /// <param name="request">The site name, date, time (HH:mm), and max slots.</param>
        /// <response code="200">Quota successfully updated.</response>
        /// <response code="400">Invalid time format provided.</response>
        [HttpPut("set-appointment-quota")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetAppointmentQuota([FromBody] SetQuotaRequestDto request)
        {
            // 1. Validate Time Format
            if (!TimeSpan.TryParse(request.ScheduledTime, out TimeSpan parsedTime))
            {
                return BadRequest(new { message = "Invalid time format. Use HH:mm (e.g., 08:00)." });
            }

            // 2. Extract Admin Info
            var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int adminId = int.TryParse(adminIdClaim, out var id) ? id : 0;
            string adminName = User.FindFirst("username")?.Value ?? "Admin";

            try
            {
                // 3. Execute Stored Procedure
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
            EXEC [dbo].[sp_SetAppointmentQuota]
                @SiteName = {request.SiteName},
                @ScheduledDate = {request.ScheduledDate.Date},
                @ScheduledTime = {parsedTime},
                @MaxSlots = {request.MaxSlots},
                @AccessedByUserId = {adminId},
                @AccessedByUsername = {adminName}");

                return Ok(new
                {
                    message = "Quota successfully set.",
                    site = request.SiteName,
                    date = request.ScheduledDate.ToString("yyyy-MM-dd"),
                    time = parsedTime.ToString(@"hh\:mm"),
                    limit = request.MaxSlots
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Database Error", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves the list of active appointment quotas for scheduling sites.
        /// </summary>
        [HttpGet("get-quotas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuotas([FromQuery] string? siteName, [FromQuery] DateTime? date)
        {
            try
            {
                // 1. Extract Admin Info from JWT Claims
                var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int? adminId = int.TryParse(adminIdClaim, out var id) ? id : (int?)null;
                string adminUsername = User.FindFirst("username")?.Value ?? "Unknown Admin";

                // 2. Execute the Stored Procedure
                // We use .Date to ensure we only send the date part (2026-02-12) to SQL
                var results = await _context.AppointmentQuotas
                    .FromSqlInterpolated($@"
                EXEC [dbo].[sp_GetAppointmentQuotas] 
                    @SiteName = {siteName}, 
                    @ScheduledDate = {date?.Date}, 
                    @AccessedByUserId = {adminId},
                    @AccessedByUsername = {adminUsername}")
                    .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving quotas", error = ex.Message });
            }
        }

        //STATUS- lookup table
        //1 capture
        //2 uploaded
        //3 afis hit
        //4 validated
        //5 generate file
        //6 id produced
        //7 active
    }
}
