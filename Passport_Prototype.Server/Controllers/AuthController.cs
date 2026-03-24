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


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // 1. Basic Validation
            if (dto == null) return BadRequest("Invalid request.");

            // 2. Validate Password strength
            if (!Regex.IsMatch(dto.Password, PasswordRegex))
            {
                return BadRequest("Password must be 8+ chars, with uppercase, lowercase, number, and special character.");
            }

            // 3. Verify OTP (Placeholder logic - you'll need to check your OTP table/cache)
            // if (!VerifyCode(dto.Email, dto.VerificationCode)) { return BadRequest("Invalid or expired verification code."); }

            // 4. Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Username == dto.Email))
            {
                return BadRequest("This email is already registered.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var newUser = new Users
                {
                    Username = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    UserType = 1, // Defaulting to System
                    UserRole = 2, // Assigning "System User" as a flat integer
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = _hasher.HashPassword(null!, dto.Password),

                    // Fully populate the passport info table
                    PassportInfo = new PassportPersonalInformation
                    {
                        FirstName = dto.FirstName,
                        MiddleName = dto.MiddleName,
                        LastName = dto.LastName,
                        Suffix = dto.Suffix,
                        Birthdate = dto.Birthdate,
                        Gender = dto.Gender,
                        Nationality = dto.Nationality,
                        CivilStatusId = dto.CivilStatusId,
                        hasPSABirthcert = dto.HasPSABirthcert,
                        isBirthLegitimate = dto.IsBirthLegitimate,
                        BirthCountry = dto.BirthCountry,
                        BirthRegion = dto.BirthRegion,
                        BirthProvince = dto.BirthProvince,
                        BirthCity = dto.BirthCity,
                        BirthBarangay = dto.BirthBarangay
                    }
                };

                _context.Users.Add(newUser);

                // This single SaveChangesAsync inserts into BOTH Users and PassportPersonalInformation
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Registration successful!",
                    userId = newUser.Id,
                    token = GenerateJwtToken(newUser)
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Check for InnerException to see specific SQL constraints failing
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Database Error: {errorMsg}");
            }
        }
    }
    }

