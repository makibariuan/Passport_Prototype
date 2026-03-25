using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using OnlineRegistration.Server.Data;
using OnlineRegistration.Server.DTOs;
using OnlineRegistration.Server.Models;
using OnlineRegistration.Server.Services.Interfaces;
using Passport_Prototype.Server.Models;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;


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
                new Claim("firstname", user.FirstName ?? ""),
                new Claim("lastname", user.LastName ?? ""),
                new Claim("usertype", user.UserType.ToString()?? ""),
                new Claim("userrole", user.UserRole.ToString()?? ""),
                new Claim("PersonID", user.PersonID.ToString()?? ""),
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

        [HttpPost("initiate-registration")]
        public async Task<IActionResult> InitiateRegistration([FromBody] InitiateRegistrationDto dto) // Changed from string email
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "Email is required." });

            var emailLower = dto.Email.Trim().ToLower(); // Access via dto.Email

            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == emailLower);

                // 2. Check if they are already fully registered
                if (existingUser != null && !string.IsNullOrEmpty(existingUser.PasswordHash))
                {
                    return BadRequest(new { message = "This email is already registered." });
                }

                var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

                if (existingUser == null)
                {
                    existingUser = new Users
                    {
                        Email = emailLower,
                        Username = emailLower,
                        IsActive = false,
                        IsEmailConfirmed = false,
                        CreatedAt = DateTime.UtcNow,
                        UserType = 1, // Defaulting as you did in previous steps
                        UserRole = 2
                    };
                    _context.Users.Add(existingUser);
                }

                // 3. Update OTP fields
                existingUser.LoginOtp = otp;
                existingUser.LoginOtpExpiry = DateTime.UtcNow.AddMinutes(10);

                // 4. Save changes
                await _context.SaveChangesAsync();

                // 5. Queue Email
                _emailQueue.QueueEmail(new EmailMessage
                {
                    To = emailLower,
                    Subject = "Registration Verification Code",
                    Body = $"<p>Your verification code is: <strong>{otp}</strong></p>"
                });

                return Ok(new { message = "Verification code sent to your email." });
            }
            catch (Exception ex)
            {
                // Log the full stack trace for debugging
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new { message = "Error initiating registration.", details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPut("complete-registration")]
        public async Task<IActionResult> CompleteRegistration([FromBody] RegisterDto dto)
        {
            var emailLower = dto.Email.Trim().ToLower();

            // 1. Find the skeleton record
            var userRecord = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == emailLower);

            // 2. Security Gate: Ensure the email was verified first
            if (userRecord == null)
            {
                return BadRequest(new { message = "Email verification required before completing registration." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 3. Hash password using BCrypt
                userRecord.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                // 4. Finalize Profile
                userRecord.FirstName = dto.FirstName;
                userRecord.LastName = dto.LastName;
                userRecord.IsActive = true;
                userRecord.UserType = 1;
                userRecord.UserRole = 2;
                userRecord.LoginOtp = null;
                userRecord.LoginOtpExpiry = null;
                userRecord.MustResetPassword = false;

                // 5. Populate Passport Details
                userRecord.PassportInfo = new PassportPersonalInformation
                {
                    FirstName = dto.FirstName,
                    MiddleName = dto.MiddleName,
                    LastName = dto.LastName,
                    Suffix = dto.Suffix
                    //Birthdate = dto.Birthdate,
                    //Gender = dto.Gender,
                    //Nationality = dto.Nationality,
                    //CivilStatusId = dto.CivilStatusId,
                    //hasPSABirthcert = dto.HasPSABirthcert,
                    //isBirthLegitimate = dto.IsBirthLegitimate,
                    //BirthCountry = dto.BirthCountry,
                    //BirthRegion = dto.BirthRegion,
                    //BirthProvince = dto.BirthProvince,
                    //BirthCity = dto.BirthCity,
                    //BirthBarangay = dto.BirthBarangay
                };

                _context.Users.Update(userRecord);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Registration completed successfully!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Failed to complete registration.", details = ex.Message });
            }
        }

        [HttpPost("verify-registration-otp")]
        public async Task<IActionResult> VerifyRegistrationOtp([FromBody] VerifyOtpDto dto)
        {
            // 1. Basic Validation
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.VerificationCode))
            {
                return BadRequest(new { message = "Email and Verification Code are required." });
            }

            try
            {
                // 2. Find the "Pre-registered" user record using the Email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email.Trim().ToLower());

                if (user == null)
                {
                    return BadRequest(new { message = "Registration record not found for this email." });
                }

                // 3. 🔥 THE MATCH CHECK: Compare the provided code with the DB value
                // Using .Trim() on the input to prevent hidden space mismatches
                if (user.LoginOtp == null || user.LoginOtp != dto.VerificationCode.Trim())
                {
                    return BadRequest(new { message = "Invalid verification code. Please check your email and try again." });
                }

                // 4. Check Expiry
                if (user.LoginOtpExpiry == null || user.LoginOtpExpiry < DateTime.UtcNow)
                {
                    return BadRequest(new { message = "Verification code has expired. Please request a new one." });
                }

                // 5. Success Logic: Mark Email as Confirmed and NULLIFY the OTP fields
                // This ensures the same code cannot be used twice.
                user.IsEmailConfirmed = true;
                user.LoginOtp = null;
                user.LoginOtpExpiry = null;

                // Note: We keep IsActive = false until they finish the PUT (CompleteRegistration) step.
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Email verified successfully!",
                    email = user.Email // Returning email helps the frontend navigate to the next step
                });
            }
            catch (Exception ex)
            {
                // Capture inner exception details for database-level issues
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred during verification.", details = errorMsg });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and Password are required." });
            }

            try
            {
                // 1. Find the user (supports Email or Username login)
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email.Trim() || u.Username == request.Email.Trim());

                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid credentials." });
                }




                // 3. Verify Password (using BCrypt)
                try
                {
                    if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    {
                        return Unauthorized(new { message = "Invalid credentials." });
                    }
                }
                catch (BCrypt.Net.SaltParseException)
                {
                    // This catches the "Invalid salt version" error
                    // It means the database has a non-bcrypt hash
                    return Unauthorized(new { message = "Your account requires a password reset due to security updates." });
                }



                // 5. Generate 6-digit OTP and Expiry (10 minutes)
                var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
                user.LoginOtp = otp;
                user.LoginOtpExpiry = DateTime.UtcNow.AddMinutes(10);

                // 6. Save to Database
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // 7. Queue OTP email
                _emailQueue.QueueEmail(new EmailMessage
                {
                    To = user.Email,
                    Subject = "Your Login OTP",
                    Body = $@"<p>Hello {user.FirstName},</p>
                              <p>Your verification code is: <strong>{otp}</strong></p>
                              <p>This code will expire in 10 minutes.</p>"
                });

                return Ok(new
                {
                    message = "OTP sent to your email. Please enter it to complete login.",
                    email = user.Email // Helpful for the frontend to know where it was sent
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            try
            {
                // 1. Fetch user and validate OTP details
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null) return Unauthorized(new { message = "User not found." });
                if (string.IsNullOrEmpty(user.LoginOtp)) return Unauthorized(new { message = "No OTP session found." });
                if (user.LoginOtpExpiry < DateTime.UtcNow) return Unauthorized(new { message = "OTP has expired." });
                if (user.LoginOtp != dto.VerificationCode) return Unauthorized(new { message = "Invalid OTP." });

                // 2. Generate JWT token
                var token = GenerateJwtToken(user);

                // 3. Security Cleanup: Clear OTP and save the active token
                user.LoginOtp = null;
                user.LoginOtpExpiry = null;
                user.ActiveToken = token;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();


                return Ok(new
                {
                    token,
                    message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Security error.", details = ex.Message });
            }
        }

        [HttpPost("kit-login")]
        public async Task<IActionResult> KitLogin([FromBody] KitLoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and Password are required." });
            }

            try
            {
                // 1. Fetch the user including Role if needed for claims
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == request.username.ToLower());

                // 2. Basic Validation (User Found/Active)
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid Kit credentials." });
                }

                // 3. Password Verification
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {

                    return Unauthorized(new { message = "Invalid Kit credentials." });
                }

                // 4. 🔥 CRITICAL CHECK: Enforce Kit User Type (Type 2)
                if (user.UserType != 2)
                {

                    return Unauthorized(new { message = "This account is restricted to portal access only." });
                }

                //// 5. Must Reset Password Check
                //if (user.MustResetPassword)
                //{
                //    return Ok(new LoginResponse
                //    {
                //        Message = "Password reset required.",
                //        Token = "",
                //    });
                //}

                // 6. Generate Token and update ActiveToken
                var token = GenerateJwtToken(user);
                user.ActiveToken = token;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();



                return Ok(new LoginResponse
                {
                    Message = "Kit login successful.",
                    Token = token,
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
            if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest(new { message = "Email is required." });

            var email = dto.Email.Trim().ToLower();
            var resetToken = Guid.NewGuid().ToString("N");

            try
            {
                // 1. Find the user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                // 2. If user exists, update token and expiry (24 hours)
                if (user != null)
                {
                    user.PasswordResetToken = resetToken;

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    // 3. Queue the email
                    var frontendUrl = _config["FrontendUrl"];
                    var resetUrl = $"{frontendUrl}/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";

                    _emailQueue.QueueEmail(new EmailMessage
                    {
                        To = email,
                        Subject = "Password Reset Request",
                        Body = $@"<p>Hello {user.FirstName},</p>
                              <p>Reset your password by clicking the link below:</p>
                              <p><a href='{resetUrl}'>Reset Password</a></p>
                              <p>This link will expire in 24 hours.</p>"
                    });

                }

                // 4. Always return the same message to prevent "Email Fishing"
                return Ok(new { message = "If this email is registered, a reset link has been sent." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates a user's password using a valid reset token.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> FlexiblePasswordUpdate([FromBody] FlexiblePasswordRequest request)
        {
            // 1. Validation Logic
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.NewPassword))
                return BadRequest(new { message = "Email and New Password are required." });

            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match." });

            

            try
            {
                // 2. Fetch User and validate token (if your request DTO includes the token)
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLower());

                if (user == null) return NotFound(new { message = "User account not found." });

           
                // 4. Update Password and Clear Token
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.PasswordResetToken = null; // Clear token after use
                user.MustResetPassword = false; // User has now reset it
                user.ActiveToken = null; // Force logout everywhere else

                _context.Users.Update(user);
                await _context.SaveChangesAsync();


                return Ok(new { message = "Password updated successfully. You can now log in." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An internal error occurred.", error = ex.Message });
            }
        }

        /// <summary>
        /// Terminates the user session and invalidates the active server-side token.
        /// </summary>
        [HttpPost("logout")]
        [Authorize] // Ensure the user is actually authenticated to call this
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int parsedUserId))
            {
                return Unauthorized();
            }

            try
            {
                var user = await _context.Users.FindAsync(parsedUserId);
                if (user != null)
                {
                    // Invalidate the token on the server side
                    user.ActiveToken = null;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during logout.", error = ex.Message });
            }
        }
    }
    }

