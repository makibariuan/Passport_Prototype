using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using Passport_Prototype.Server.DTOs;
using Passport_Prototype.Server.Models;
using System.Security.Claims;
using System.Diagnostics;

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

        // UPDATE
        [HttpPatch]
        public async Task<IActionResult> Update([FromBody]List<UpdateFamilyDTO> dtos)
        {
            var personalInfoId = dtos[0].passportPersonalInformationId;

            var families = await _context.Family
                .Where(f => f.PassportPersonalInformationId == personalInfoId)
                .ToListAsync();

            if (families == null || !families.Any())
                return NotFound($"No family records found. id = {personalInfoId}");

            foreach (var dto in dtos)
            {
                var family = families.FirstOrDefault(f => f.FamilyId == dto.FamilyId);

                if (family == null)
                    continue; // or return NotFound($"Family with ID {dto.Id} not found");

                // Manual mapping
                family.FirstName = dto.FirstName;
                family.MiddleName = dto.MiddleName;
                family.LastName = dto.LastName;
                family.Suffix = dto.Suffix;
                family.Relationship = dto.Relationship;
                family.isAlive = dto.IsAlive ?? false; // safe handling
                family.Citizenship = dto.Citizenship;
            }

            await _context.SaveChangesAsync();

            return NoContent();
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
