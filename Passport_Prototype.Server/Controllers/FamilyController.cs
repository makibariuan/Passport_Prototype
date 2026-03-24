using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OnlineRegistration.Server.Data;
using Passport_Prototype.Server.DTOs;
using Passport_Prototype.Server.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Passport_Prototype.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                    UserId = familyMember.UserId,
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var family = await _context.Family.Where(f => f.UserId == id).ToListAsync();

            if (family == null)
                return NotFound();

            return Ok(family);
        }

        // UPDATE
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, UpdateFamilyDTO dto)
        {
            var family = await _context.Family.FindAsync(id);

            if (family == null)
                return NotFound();

            // Manual mapping
            if (dto.FirstName != null) family.FirstName = dto.FirstName;
            if (dto.MiddleName != null) family.MiddleName = dto.MiddleName;
            if (dto.LastName != null) family.LastName = dto.LastName;
            if (dto.Suffix != null) family.Suffix = dto.Suffix;
            if (dto.Relationship != null) family.Relationship = dto.Relationship;
            if (dto.IsAlive.HasValue) family.isAlive = dto.IsAlive.Value;
            if (dto.Citizenship != null) family.Citizenship = dto.Citizenship;

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
