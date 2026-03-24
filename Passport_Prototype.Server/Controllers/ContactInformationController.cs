using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using Passport_Prototype.Server.DTOs;
using Passport_Prototype.Server.Models;

namespace Passport_Prototype.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactInformationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContactInformationController(AppDbContext context)
        {
            _context = context;
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(ContactInformationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = new ContactInformation
            {
                UserId = dto.UserId,
                CurrentRegion = dto.CurrentRegion,
                CurrentProvince = dto.CurrentProvince,
                CurrentCityMunicipality = dto.CurrentCityMunicipality,
                CurrentBarangay = dto.CurrentBarangay,
                CurrentPostalCode = dto.CurrentPostalCode,
                PersonalMobileNumber = dto.PersonalMobileNumber,
                PersonalLandlineNumber = dto.PersonalLandlineNumber,
                Email = dto.Email
            };

            _context.ContactInformation.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // READ ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.ContactInformation
                //.Include(c => c.Users)
                .ToListAsync();

            return Ok(data);
        }

        // READ BY ID (UserID)
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var data = await _context.ContactInformation
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // UPDATE
        [HttpPatch("{userId}")]
        public async Task<IActionResult> Update(int userId, ContactInformationDto dto)
        {
            var entity = await _context.ContactInformation
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (entity == null)
                return NotFound();

            entity.CurrentRegion = dto.CurrentRegion;
            entity.CurrentProvince = dto.CurrentProvince;
            entity.CurrentCityMunicipality = dto.CurrentCityMunicipality;
            entity.CurrentBarangay = dto.CurrentBarangay;
            entity.CurrentPostalCode = dto.CurrentPostalCode;
            entity.PersonalMobileNumber = dto.PersonalMobileNumber;
            entity.PersonalLandlineNumber = dto.PersonalLandlineNumber;
            entity.Email = dto.Email;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // DELETE
        [HttpDelete("{userId}")]
        public async Task<IActionResult> Delete(int userId)
        {
            var entity = await _context.ContactInformation
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (entity == null)
                return NotFound();

            _context.ContactInformation.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}