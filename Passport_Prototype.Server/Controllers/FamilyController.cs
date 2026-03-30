using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using OnlineRegistration.Server.Models;
using Passport_Prototype.Server.DTOs;
using Passport_Prototype.Server.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace Passport_Prototype.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FamiliesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FamiliesController(AppDbContext context)
        {
            _context = context;
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CreateFamilyDTO dto)
        {
            if (dto.FamilyMember.Count <= 0)
            {
                return BadRequest("Atleast 1 family member");
            }

            var Members = new List<Family>();

            foreach (var familyMember in dto.FamilyMember)
            {
                var member = new Family
                {
                    PassportPersonalInformationId = familyMember.PassportPersonalInformationId,
                    FirstName = familyMember.FirstName,
                    MiddleName = familyMember.MiddleName,
                    LastName = familyMember.LastName,
                    Suffix = familyMember.Suffix,
                    Relationship = familyMember.Relationship,
                    isAlive = familyMember.IsAlive,
                    Citizenship = familyMember.Citizenship
                };
                Members.Add(member);
            }

            await _context.Family.AddRangeAsync(Members);
            await _context.SaveChangesAsync();

            return Created();
        }

        // READ BY ID
        [HttpGet("My-Family")]
        public async Task<IActionResult> GetById()
        {
            var userIdString = User.FindFirstValue("id");

            if (!int.TryParse(userIdString, out int userId))
            {
                throw new Exception("Invalid user ID in claims.");
            }

            var passportPersonalInformation = await _context.PassportPersonalInformation.FirstOrDefaultAsync(p => p.UserId == userId && p.Relationship == null);

            var family = await _context.Family.Where(f => f.PassportPersonalInformationId == passportPersonalInformation.PassportPersonalInformationId).ToListAsync();

            if (family == null)
                return NotFound();

            return Ok(family);
        }

        [HttpGet("{personalId}")]
        public async Task<IActionResult> GetFamilyByPersonalId(int personalId)
        {
            var family = await _context.Family
                .Where(f => f.PassportPersonalInformationId == personalId)
                .ToListAsync();

            if (!family.Any())
                return NotFound();

            return Ok(family);
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> Upsert([FromBody] List<UpdateFamilyDTO> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest("No data provided.");

            var personalInfoId = dtos[0].passportPersonalInformationId;

            // Fetch existing family records for this personal info
            var families = await _context.Family
                .Where(f => f.PassportPersonalInformationId == personalInfoId)
                .ToListAsync();

            foreach (var dto in dtos)
            {
                var family = families.FirstOrDefault(f => f.FamilyId == dto.FamilyId);

                if (family != null)
                {
                    // ✅ UPDATE
                    family.FirstName = dto.FirstName;
                    family.MiddleName = dto.MiddleName;
                    family.LastName = dto.LastName;
                    family.Suffix = dto.Suffix;
                    family.Relationship = dto.Relationship;
                    family.isAlive = dto.IsAlive ?? false;
                    family.Citizenship = dto.Citizenship;
                }
                else
                {
                    // ✅ CREATE FAMILY
                    var newFamily = new Family
                    {
                        PassportPersonalInformationId = (int)dto.passportPersonalInformationId!,
                        FirstName = dto.FirstName,
                        MiddleName = dto.MiddleName,
                        LastName = dto.LastName,
                        Suffix = dto.Suffix,
                        Relationship = dto.Relationship,
                        isAlive = dto.IsAlive ?? false,
                        Citizenship = dto.Citizenship
                    };

                    _context.Family.Add(newFamily);
                    families.Add(newFamily); // Add to local list for later checks
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Upsert");
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var family = await _context.Family.FindAsync(id);

            if (family == null)
                return NotFound();

            _context.Family.Remove(family);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
