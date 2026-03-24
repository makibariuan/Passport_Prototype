using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using Passport_Prototype.Server.DTOs;
using Passport_Prototype.Server.Models;

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
            var family = new Family
            {
                UserId = dto.UserId,
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,
                Suffix = dto.Suffix,
                Relationship = dto.Relationship,
                isAlive = dto.IsAlive,
                Citizenship = dto.Citizenship
            };

            await _context.Family.AddAsync(family);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = family.UserId }, family);
        }

        // READ
        [HttpGet]
        public async Task<IActionResult> GetAll(int? pageNumber, int? pageSize)
        {
            var query = _context.Family.AsQueryable();

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                query = query
                    .Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }

            var families = await query.ToListAsync();
            return Ok(families);
        }

        // READ BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var family = await _context.Family.FindAsync(id);

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
