using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineRegistration.Server.Data;
using OnlineRegistration.Server.DTOs;
using OnlineRegistration.Server.Models;
using OnlineRegistration.Server.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MimeKit;
using MailKit.Net.Smtp;
using System.Text.RegularExpressions;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System.Data;


namespace OnlineRegistration.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmployeesDbContext _db;
        private readonly IPasswordHasher<UsersEmployee> _hasher;
        private readonly IConfiguration _config;
        private readonly IEmailQueue _emailQueue;
        private const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";
        private readonly IPgpService _pgpService;

        public AuthController(AppDbContext context,EmployeesDbContext db, IPasswordHasher<UsersEmployee> hasher, IConfiguration config, IEmailQueue emailQueue, IPgpService pgpService)
        {
            _context = context;
            _db = db;
            _hasher = hasher;
            _config = config;
            _emailQueue = emailQueue;
            _pgpService = pgpService;
        }

        private string GenerateJwtToken(Users user)
        {
            var jwtSettings = _config.GetSection("Jwt");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // 🔑 required
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim("email", user.Email?.ToString() ?? ""),
                new Claim("firstname", user.FirstName),
                new Claim("lastname", user.LastName),
                new Claim("usertype", user.UserType.ToString()),
                new Claim("userrole", user.UserRole.ToString()),
                new Claim("PersonID", user.PersonID.ToString()),
                new Claim("EmployeeCode", user.EmployeeID?.ToString() ?? ""),
        };


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        //        USER TYPE                 USER ROLE
        //        1 - System                1 - Super Admin
        //        2 - Kit                   2 - System User
        //                                  3 - Kit User
        //                                  4 - Employee
        //                                  5 - Citizen
        //                                  6 - HR


        /// <summary>
        /// Registers a new user and queues a verification email.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // 1. Basic Model Validation
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 2. Generate security tokens and hash password
            var emailToken = Guid.NewGuid().ToString("N");
            var passHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            try
            {
                // 3. Prepare the Output Parameter to capture the new User ID from the SP
                var userIdParam = new Microsoft.Data.SqlClient.SqlParameter
                {
                    ParameterName = "@NewUserId",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output
                };

                // 4. Execute the updated Stored Procedure
                // This now inserts into [Users] and [EnrollmentRegistryID] in one transaction
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC [dbo].[sp_RegisterUser]
                @Username = {dto.Username.Trim()},
                @Email = {dto.Email.Trim().ToLower()},
                @PasswordHash = {passHash},
                @FirstName = {dto.FirstName.Trim()},
                @LastName = {dto.LastName.Trim()},
                @MiddleName = {dto.MiddleName?.Trim()},
                @BirthDate = {dto.BirthDate},
                @EmployeeID = {dto.EmployeeID?.Trim()},
                @GovIDType = {dto.GovIDType},
                @GovIDNumber = {dto.GovIDNumber},
                @GovIDImage = {dto.IDImageBase64},
                @GovIDExtension = {dto.IDFileExtension},
                @EmailToken = {emailToken},
                @NewUserId = {userIdParam} OUTPUT");

                // Capture the ID returned by the database
                var newUserId = (int)userIdParam.Value;

                // 5. Post-Registration: Email Queueing
                try
                {
                    var confirmUrl = $"{_config["FrontendUrl"]}/confirm-email?token={emailToken}&email={dto.Email}";
                    _emailQueue.QueueEmail(new EmailMessage
                    {
                        To = dto.Email,
                        Subject = "Action Required: Confirm Your Makatizen Account",
                        Body = $"<h1>Welcome!</h1><p>Please <a href='{confirmUrl}'>click here</a> to verify your email.</p>"
                    });
                }
                catch (Exception emailEx)
                {
                    // Logging the error but not failing the request
                    Console.WriteLine($"Email service error for User {newUserId}: {emailEx.Message}");
                }

                return Ok(new
                {
                    message = "Registration successful!",
                    userId = newUserId,
                    details = "A confirmation email has been sent to your registered address."
                });
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                // Catches THROWs from the SP (e.g., Email exists)
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
            }
        }

        /// <summary>
        /// Validates a user's email address using a unique security token.
        /// </summary>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token, [FromQuery] string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Token and Email are required." });
            }

            try
            {
                // Execute the stored procedure
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC [dbo].[sp_ConfirmUserEmail] 
                @Email = {email}, 
                @Token = {token}");

                return Ok(new { message = "Email confirmed successfully. You can now log in." });
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                // Catch the custom THROW 50012 or 50013 from SQL
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during email confirmation.", error = ex.Message });
            }
        }

        /// <summary>
        /// Initiates the login process by verifying credentials and generating a One-Time Password (OTP).
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and Password are required." });
            }

            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            try
            {
                // Output parameters
                var userIdParam = new SqlParameter("@UserId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var usernameParam = new SqlParameter("@Username", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                var emailParam = new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                var passHashParam = new SqlParameter("@PasswordHash", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };
                var mustResetParam = new SqlParameter("@MustResetPassword", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var isConfirmedParam = new SqlParameter("@IsEmailConfirmed", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var empIdParam = new SqlParameter("@EmployeeID", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                var personIdParam = new SqlParameter("@PersonID", SqlDbType.Int) { Direction = ParameterDirection.Output };

                // Execute the stored procedure
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[sp_HandleLoginSecurity] @EmailOrUsername, @GeneratedOtp, 2, " +
                    "@UserId OUTPUT, @Username OUTPUT, @Email OUTPUT, @PasswordHash OUTPUT, @MustResetPassword OUTPUT, @IsEmailConfirmed OUTPUT, " +
                    "@EmployeeID OUTPUT, @PersonID OUTPUT",
                    new SqlParameter("@EmailOrUsername", request.Email.Trim()),
                    new SqlParameter("@GeneratedOtp", otp),
                    userIdParam, usernameParam, emailParam, passHashParam, mustResetParam, isConfirmedParam, empIdParam, personIdParam
                );

                // Null-safety: ensure password hash exists
                var dbPasswordHash = passHashParam.Value != DBNull.Value ? passHashParam.Value?.ToString() : null;
                if (string.IsNullOrEmpty(dbPasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, dbPasswordHash))
                {
                    var userId = userIdParam.Value != DBNull.Value ? (int)userIdParam.Value : 0;
                    await LogManualEntry(userId, "LOGIN_FAILURE_INVALID_PASSWORD", $"Password mismatch for {request.Email}");
                    return Unauthorized(new { message = "Invalid credentials." });
                }

                // Check if password reset is required
                if (mustResetParam.Value != DBNull.Value && (bool)mustResetParam.Value)
                {
                    return Ok(new { message = "Password reset required.", mustReset = true });
                }

                // Email null-safety
                var dbEmail = emailParam.Value != DBNull.Value ? emailParam.Value?.ToString() : null;
                var dbUsername = usernameParam.Value != DBNull.Value ? usernameParam.Value?.ToString() : "User";

                if (string.IsNullOrEmpty(dbEmail))
                {
                    return BadRequest(new { message = "User email not found. Cannot send OTP." });
                }

                // Queue OTP email
                _emailQueue.QueueEmail(new EmailMessage
                {
                    To = dbEmail,
                    Subject = "Your Login OTP",
                    Body = $"<p>Hello {dbUsername},</p><p>Your OTP is: <strong>{otp}</strong></p>"
                });

                return Ok(new { message = "OTP sent to your email. Please enter it to complete login." });
            }
            catch (SqlException ex)
            {
                // SP errors
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Catch-all
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        private async Task LogManualEntry(int userId, string desc, string note)
        {
            _context.ApplicationLogs.Add(new ApplicationLog
            {
                LogId = Guid.NewGuid().ToString("N"),
                UserId = userId,
                LogDescription = desc,
                LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                LogTime = DateTime.Now.ToString("HH:mm:ss"),
                Notes = note
            });
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Validates the OTP and issues a JWT bearer token for session management.
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            try
            {
                // 1. Fetch the user initially to check if OTP exists/expired
                var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (dbUser == null) return Unauthorized(new { message = "Email is not registered" });
                if (string.IsNullOrEmpty(dbUser.LoginOtp)) return Unauthorized(new { message = "No OTP generated" });
                if (dbUser.LoginOtpExpiry < DateTime.UtcNow) return Unauthorized(new { message = "OTP has expired" });
                if (dbUser.LoginOtp != dto.Otp) return Unauthorized(new { message = "Invalid OTP" });

                // 2. Prepare Output Parameters for the Stored Procedure
                var userIdParam = new SqlParameter("@UserId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var usernameParam = new SqlParameter("@Username", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                var roleParam = new SqlParameter("@UserRole", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var empIdParam = new SqlParameter("@EmployeeID", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                var personIdParam = new SqlParameter("@PersonID", SqlDbType.Int) { Direction = ParameterDirection.Output };

                // --- NEW PARAMETERS FOR JWT ---
                var fnameParam = new SqlParameter("@FirstName", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                var lnameParam = new SqlParameter("@LastName", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                var emailParam = new SqlParameter("@UserEmail", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                var typeParam = new SqlParameter("@UserType", SqlDbType.Int) { Direction = ParameterDirection.Output };

                // 3. Execute the updated Stored Procedure
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[sp_VerifyLoginOtp] @Email, @SubmittedOtp, @UserId OUTPUT, @Username OUTPUT, @UserRole OUTPUT, @EmployeeID OUTPUT, @PersonID OUTPUT, @FirstName OUTPUT, @LastName OUTPUT, @UserEmail OUTPUT, @UserType OUTPUT",
                    new SqlParameter("@Email", dto.Email),
                    new SqlParameter("@SubmittedOtp", dto.Otp),
                    userIdParam, usernameParam, roleParam, empIdParam, personIdParam, fnameParam, lnameParam, emailParam, typeParam
                );

                // 4. Validate procedure output
                if (userIdParam.Value == DBNull.Value) return Unauthorized(new { message = "Verification failed." });

                // 5. Map the results into a User object for the JWT Generator
                var user = new Users
                {
                    Id = (int)userIdParam.Value,
                    Username = usernameParam.Value?.ToString() ?? "",
                    UserRole = (int)roleParam.Value,
                    UserType = (int)typeParam.Value,
                    FirstName = fnameParam.Value?.ToString() ?? "User", // Prevents ArgumentNullException
                    LastName = lnameParam.Value?.ToString() ?? "",      // Prevents ArgumentNullException
                    Email = emailParam.Value?.ToString() ?? dto.Email,
                    EmployeeID = empIdParam.Value != DBNull.Value ? empIdParam.Value.ToString() : null,
                    PersonID = personIdParam.Value as int?
                };

                // 6. Generate JWT token (This will now have all claims)
                var token = GenerateJwtToken(user);

                // 7. Save active token to database
                dbUser.ActiveToken = token;
                await _context.SaveChangesAsync();

                return Ok(new { token });
            }
            catch (SqlException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Security error.", details = ex.Message });
            }
        }

        /// <summary>
        /// Initiates the Resend OTP to email credentials and generating a One-Time Password (OTP).
        /// </summary>
        /// 
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Email))
                    return BadRequest(new { message = "Email or username is required." });

                // 1. Generate new OTP
                var newOtp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

                // 2. Prepare output parameters
                var userIdParam = new SqlParameter("@UserId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var usernameParam = new SqlParameter("@Username", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                var emailParam = new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                var empIdParam = new SqlParameter("@EmployeeID", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                var personIdParam = new SqlParameter("@PersonID", SqlDbType.Int) { Direction = ParameterDirection.Output };

                // 3. Execute the new stored procedure
                await _context.Database.ExecuteSqlRawAsync(
                       @"EXEC [dbo].[sp_ResendOtp]
                       @EmailOrUsername = @EmailOrUsername,
                       @GeneratedOtp = @GeneratedOtp,
                       @UserId = @UserId OUTPUT,
                       @Username = @Username OUTPUT,
                       @Email = @Email OUTPUT,
                       @EmployeeID = @EmployeeID OUTPUT,
                       @PersonID = @PersonID OUTPUT",
                    new SqlParameter("@EmailOrUsername", request.Email.Trim()),
                    new SqlParameter("@GeneratedOtp", newOtp),
                    userIdParam, usernameParam, emailParam, empIdParam, personIdParam
                );

                // 4. Validate output values
                if (emailParam.Value == DBNull.Value || usernameParam.Value == DBNull.Value)
                    return NotFound(new { message = "User not found." });

                var email = emailParam.Value.ToString();
                var username = usernameParam.Value.ToString();

                // 5. Send the new OTP via email
                _emailQueue.QueueEmail(new EmailMessage
                {
                    To = email,
                    Subject = "Your New Login OTP",
                    Body = $"<p>Hello {username},</p><p>Your new OTP is: <strong>{newOtp}</strong></p>"
                });

                return Ok(new { message = "A new OTP has been sent to your email." });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { message = "Database error occurred.", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred.", details = ex.Message });
            }
        }

        /// <summary>
        /// Validates the Kit credentials and issues a JWT bearer token for session management.
        /// </summary>
        [HttpPost("kit-login")]
        public async Task<IActionResult> KitLogin([FromBody] KitLoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.username.ToLower());

            // Local function for logging attempts (omitted for brevity)
            void LogLoginAttempt(string description, int? userId = null, string specificNote = null)
            { /* ... */ }

            if (user == null)
            {
                Console.WriteLine("DEBUG: User not found in database.");
                return Unauthorized(new { message = "Invalid Kit credentials." });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            Console.WriteLine($"DEBUG: Password Match: {isPasswordValid}");
            Console.WriteLine($"DEBUG: UserType: {user.UserType}, IsActive: {user.IsActive}");

            // 1. Basic Validation (User Found/Active)
            if (user == null || !user.IsActive)
            {
                LogLoginAttempt("KIT_LOGIN_FAILURE_INVALID_USER", specificNote: "User not found or account inactive.");
                await _context.SaveChangesAsync();
                return Unauthorized(new { message = "Invalid Kit credentials." });
            }

            // 2. Password Verification
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                LogLoginAttempt("KIT_LOGIN_FAILURE_INVALID_PASSWORD", user.Id, specificNote: "Password mismatch.");
                await _context.SaveChangesAsync();
                return Unauthorized(new { message = "Invalid Kit credentials." });
            }

            // 🔥 CRITICAL CHECK: Enforce Kit User Type
            // Only UserType 2 (Kit User) is allowed to log in here.
            if (user.UserType != 2)
            {
                LogLoginAttempt("KIT_LOGIN_FAILURE_PORTAL_USER_BLOCKED", user.Id, specificNote: "Portal user attempted Kit login.");
                await _context.SaveChangesAsync();
                return Unauthorized(new { message = "This account is restricted to kit access." });
            }

            // 3. Must Reset Password (if applicable)
            if (user.MustResetPassword)
            {
                // Still requires password reset, even on the Kit
                return Ok(new LoginResponse
                {
                    Message = "Password reset required.",
                    Token = "",
                });
            }

            // 4. Generate Token (Kit Token)
            // Note: You might use a shorter expiration time for Kit tokens for security.
            var token = GenerateJwtToken(user);
            user.ActiveToken = token;

            LogLoginAttempt("KIT_LOGIN_SUCCESS", user.Id, specificNote: "Kit user successfully authenticated.");
            await _context.SaveChangesAsync();

            // 5. Return Token
            return Ok(new LoginResponse
            {
                Message = "Kit login successful.",
                Token = token,
            });
        }

        [HttpGet("kit-credentials")]
        public async Task<IActionResult> GetKitCredentials([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token parameter is required." });
            }

            try
            {
                // 1. Manually parse the token to get the UserId
                // Note: This assumes the token is a valid JWT you've already issued
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value
                                  ?? jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Token does not contain a valid User ID." });
                }

                // 2. Fetch the specific user
                var user = await _context.Users.FindAsync(userId);

                if (user == null || string.IsNullOrEmpty(user.KitEncryptedPgpData))
                {
                    return NotFound(new { message = "No encrypted configuration found for this user." });
                }

                // 3. Decrypt the PGP block
                string decryptedJson = await _pgpService.DecryptAsync(user.KitEncryptedPgpData);

                if (string.IsNullOrEmpty(decryptedJson))
                {
                    return BadRequest(new { message = "Failed to unlock credentials. PGP decryption returned empty." });
                }

                // 4. Return the decrypted data
                var credentialKit = JsonSerializer.Deserialize<object>(decryptedJson);

                return Ok(new
                {
                    message = "Credentials retrieved successfully.",
                    data = credentialKit
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Security error.", details = ex.Message });
            }
        }



        /// <summary>
        /// Initiates the password recovery flow by sending a reset link to the user.
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var email = dto.Email.Trim().ToLower();
            var resetToken = Guid.NewGuid().ToString("N");

            try
            {
                var userIdParam = new SqlParameter("@UserId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var usernameParam = new SqlParameter("@Username", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };

                // 1. Execute SP (Handles validation, token saving, and logging)
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[sp_HandleForgotPassword] @Email, @GeneratedToken, @UserId OUTPUT, @Username OUTPUT",
                    new SqlParameter("@Email", email),
                    new SqlParameter("@GeneratedToken", resetToken),
                    userIdParam, usernameParam);

                // 2. Only send email if a valid UserId was returned (User exists)
                if (userIdParam.Value != DBNull.Value)
                {
                    var frontendUrl = _config["FrontendUrl"];
                    var resetUrl = $"{frontendUrl}/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";

                    _emailQueue.QueueEmail(new EmailMessage
                    {
                        To = email,
                        Subject = "Password Reset Request",
                        Body = $@"<p>Hello {usernameParam.Value},</p>
                          <p>Reset your password by clicking the link below:</p>
                          <p><a href='{resetUrl}'>Reset Password</a></p>
                          <p>This link will expire in 24 hours.</p>"
                    });
                }

                // 3. Always return the same message to prevent "Email Fishing"
                return Ok(new { message = "If this email is registered, a reset link has been sent." });
            }
            catch (Exception ex)
            {
                // Log technical errors internally
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Updates a user's password using a valid reset token or administrative trigger.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> FlexiblePasswordUpdate([FromBody] FlexiblePasswordRequest request)
        {
            // 1. Basic Validation
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest(new { message = "Email and New Password are required." });
            }

            // 2. Client-side parity check (Confirm Password)
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match." });
            }

            // 3. Complexity Validation
            if (!IsPasswordValid(request.NewPassword))
            {
                return BadRequest(new { message = "Password does not meet the complexity requirements." });
            }

            try
            {
                // 4. Hash the new password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                // 5. Execute SP
                var userIdParam = new SqlParameter("@UserId", SqlDbType.Int) { Direction = ParameterDirection.Output };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[sp_FlexiblePasswordReset] @Email, @NewPasswordHash, @UserId OUTPUT",
                    new SqlParameter("@Email", request.Email.Trim().ToLower()),
                    new SqlParameter("@NewPasswordHash", hashedPassword),
                    userIdParam);

                return Ok(new { message = "Password updated successfully. You can now log in with your new credentials." });
            }
            catch (SqlException ex)
            {
                // Returns "User account not found" or other SQL custom errors
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An internal error occurred while updating the password." });
            }
        }

        private bool IsPasswordValid(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                return false;
            }
            return Regex.IsMatch(password, PasswordRegex);
        }

        /// <summary>
        /// Terminates the user session and invalidates the active server-side token.
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Check if Claim exists
            if (string.IsNullOrEmpty(userIdClaim))
            {
                await LogManualEntry(null, "LOGOUT_FAILURE_NO_CLAIM", "N/A");
                return Unauthorized();
            }

            // 2. Parse the Claim
            if (!int.TryParse(userIdClaim, out int parsedUserId))
            {
                await LogManualEntry(null, "LOGOUT_FAILURE_INVALID_CLAIM_FORMAT", userIdClaim);
                return Unauthorized();
            }

            try
            {
                // 3. Execute SP (Invalidates token and logs success/user not found)
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[sp_HandleLogout] @UserId, @ClaimedId",
                    new SqlParameter("@UserId", parsedUserId),
                    new SqlParameter("@ClaimedId", userIdClaim)
                );

                return Ok(new { message = "Logged out" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred during logout." });
            }
        }

        // Helper for C#-side logging (failures before reaching SP)
        private async Task LogManualEntry(int? userId, string desc, string claimedId)
        {
            _context.ApplicationLogs.Add(new ApplicationLog
            {
                LogId = Guid.NewGuid().ToString("N"),
                UserId = userId,
                LogDescription = desc,
                LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                LogTime = DateTime.Now.ToString("HH:mm:ss"),
                Notes = $"Logout attempt. Claimed ID: {claimedId}"
            });
            await _context.SaveChangesAsync();
        }
    }
}
