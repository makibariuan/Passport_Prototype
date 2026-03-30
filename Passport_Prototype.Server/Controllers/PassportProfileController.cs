using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using Passport_Prototype.Server.DTOs;
using Passport_Prototype.Server.Models;
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

        [HttpPost]
        public async Task<IActionResult> CreateFullProfile(UnifiedProfileDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Personal
                var personal = new PassportPersonalInformation
                {
                    UserId = dto.Personal.UserId,
                    FirstName = dto.Personal.FirstName,
                    MiddleName = dto.Personal.MiddleName,
                    LastName = dto.Personal.LastName,
                    Suffix = dto.Personal.Suffix,
                    Birthdate = dto.Personal.Birthdate,
                    Gender = dto.Personal.Gender,
                    Nationality = dto.Personal.Nationality,
                    CivilStatusId = dto.Personal.CivilStatus,
                    hasPSABirthcert = dto.Personal.HasPSABirthcert,
                    BirthLegitimacy = dto.Personal.BirthLegitimacy,
                    BirthCountry = dto.Personal.BirthCountry,
                    BirthRegion = dto.Personal.BirthRegion,
                    BirthProvince = dto.Personal.BirthProvince,
                    BirthCity = dto.Personal.BirthCity,
                    BirthBarangay = dto.Personal.BirthBarangay
                };
                await _context.PassportPersonalInformation.AddAsync(personal);
                await _context.SaveChangesAsync();

                // Family
                if (dto.Family.Count > 0)
                {
                    var familyMembers = dto.Family.Select(f => new Family
                    {
                        PassportPersonalInformationId = personal.PassportPersonalInformationId,
                        FirstName = f.FirstName,
                        MiddleName = f.MiddleName,
                        LastName = f.LastName,
                        Suffix = f.Suffix,
                        Relationship = f.Relationship,
                        isAlive = f.IsAlive,
                        Citizenship = f.Citizenship
                    }).ToList();

                    await _context.Family.AddRangeAsync(familyMembers);
                }

                var contact = new ContactInformation
                {
                    PassportPersonalInformationId = personal.PassportPersonalInformationId,
                    CurrentStreet = dto.Contact.CurrentStreet,
                    CurrentRegion = dto.Contact.CurrentRegion,
                    CurrentProvince = dto.Contact.CurrentProvince,
                    CurrentCityMunicipality = dto.Contact.CurrentCityMunicipality,
                    CurrentBarangay = dto.Contact.CurrentBarangay,
                    CurrentPostalCode = dto.Contact.CurrentPostalCode,
                    CurrentCountry = dto.Contact.CurrentCountry,
                    AddressAbroad = dto.Contact.AddressAbroad,
                    PersonalMobileNumber = dto.Contact.PersonalMobileNumber,
                    PersonalLandlineNumber = dto.Contact.PersonalLandlineNumber,
                    Email = dto.Contact.Email
                };
                await _context.ContactInformation.AddAsync(contact);

                // Work
                var work = new WorkInformation
                {
                    PassportPersonalInformationId = personal.PassportPersonalInformationId,
                    Employer = dto.Work.Employer,
                    Occupation = dto.Work.Occupation,
                    OfficeAddress = dto.Work.OfficeAddress,
                    OfficeBarangay = dto.Work.OfficeBarangay,
                    OfficeCountry = dto.Work.OfficeCountry,
                    OfficeRegion = dto.Work.OfficeRegion,
                    OfficeProvince = dto.Work.OfficeProvince,
                    OfficeCityMunicipality = dto.Work.OfficeCityMunicipality,
                    OfficePostalCode = dto.Work.OfficePostalCode,
                    OfficeMobileNumber = dto.Work.OfficeMobileNumber,
                    OfficeLandlineNumber = dto.Work.OfficeLandlineNumber
                };
                await _context.WorkInformation.AddAsync(work);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Personal = personal, Family = dto.Family, Contact = contact, Work = work });
            }

            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("Personal")]
        public async Task<IActionResult> GetFullProfile()
        {
            var userIdString = User.FindFirstValue("id");

            if (!int.TryParse(userIdString, out int userId))
                return BadRequest("Invalid user ID in claims.");

            // 1️⃣ Personal Info
            var personal = await _context.PassportPersonalInformation
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Relationship == null);

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
