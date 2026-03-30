using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using Passport_Prototype.Server.DTOs;
using Passport_Prototype.Server.Models;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace Passport_Prototype.Server.Controllers
{

    [ApiController]
    [Route("/api/[controller]")]

    public class PassportProfileController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PassportProfileController(AppDbContext context) {
            _context = context;
        }

        [HttpGet("{personalId}")]
        public async Task<IActionResult> GetFullProfile(int personalId)
        {
            // 1️⃣ Personal Info
            var personal = await _context.PassportPersonalInformation
                .FirstOrDefaultAsync(p => p.PassportPersonalInformationId == personalId);

            if (personal == null)
                return NotFound("Personal information not found.");

            // 2️⃣ Family
            var family = await _context.Family
                .Where(f => f.PassportPersonalInformationId == personal.PassportPersonalInformationId)
                .ToListAsync();

            // 3️⃣ Contact
            var contact = await _context.ContactInformation
                .FirstOrDefaultAsync(c => c.PassportPersonalInformationId == personal.PassportPersonalInformationId);

            // 4️⃣ Work
            var work = await _context.WorkInformation
                .FirstOrDefaultAsync(w => w.PassportPersonalInformationId == personal.PassportPersonalInformationId);

            // Build nested object
            var fullProfile = new
            {
                Personal = personal,
                Family = family,
                Contact = contact,
                Work = work
            };

            return Ok(fullProfile);
        }

        // GET all profiles by userId (nested objects)
        [HttpGet("Family")]
        public async Task<IActionResult> GetFamilyProfilesByUserId()
        {
            var userIdString = User.FindFirstValue("id");

            if (!int.TryParse(userIdString, out int userId))
                return BadRequest("Invalid user ID in claims.");

            // Get all passport profiles for the user
            var personalProfiles = await _context.PassportPersonalInformation
                .Where(p => p.UserId == userId && p.Relationship != null)
                .ToListAsync();

            if (!personalProfiles.Any())
                return NotFound("No profiles found for this user.");

            // Build list of nested profiles
            var profiles = new List<object>();

            foreach (var personal in personalProfiles)
            {
                var family = await _context.Family
                    .Where(f => f.PassportPersonalInformationId == personal.PassportPersonalInformationId)
                    .ToListAsync();

                var contact = await _context.ContactInformation
                    .FirstOrDefaultAsync(c => c.PassportPersonalInformationId == personal.PassportPersonalInformationId);

                var work = await _context.WorkInformation
                    .FirstOrDefaultAsync(w => w.PassportPersonalInformationId == personal.PassportPersonalInformationId);

                profiles.Add(new
                {
                    Personal = personal,
                    Family = family,
                    Contact = contact,
                    Work = work
                });
            }

            return Ok(profiles);
        }

        [HttpGet("Profiles")]
        public async Task<IActionResult> GetAllProfilesByUserId()
        {
            var userIdString = User.FindFirstValue("id");

            if (!int.TryParse(userIdString, out int userId))
                return BadRequest("Invalid user ID in claims.");

            // Step 1: Get raw data from DB (ONLY simple fields)
            var data = await _context.PassportPersonalInformation
                .Where(p => p.UserId == userId)
                .Select(p => new
                {
                    p.PassportPersonalInformationId,
                    p.FirstName,
                    p.MiddleName,
                    p.LastName,
                    p.Suffix,
                    p.Relationship
                })
                .ToListAsync();

            if (!data.Any())
                return NotFound("No profiles found for this user.");

            // Step 2: Transform in memory (safe)
            var profiles = data.Select(p => new
            {
                p.PassportPersonalInformationId,

                FullName = string.Join(" ",
                    new[]
                    {
                p.FirstName,
                p.MiddleName,
                p.LastName,
                p.Suffix
                    }.Where(x => !string.IsNullOrWhiteSpace(x))
                ),

                Relationship = p.Relationship
            });

            return Ok(profiles);
        }
    }
}
