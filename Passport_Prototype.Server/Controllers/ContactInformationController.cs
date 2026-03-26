using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using Passport_Prototype.Server.DTOs;
using Passport_Prototype.Server.Models;
using System.Security.Claims;

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
                PassportPersonalInformationId = dto.PassportPersonalInformationId,
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

        [HttpGet("My-Contact")]
        public async Task<IActionResult> GetMyContact()
        {
            var userIdString = User.FindFirstValue("id");

            if (!int.TryParse(userIdString, out int userId))
                return BadRequest("Invalid user ID.");

            var personal = await _context.PassportPersonalInformation
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Relationship == null);

            if (personal == null)
                return NotFound();

            var contact = await _context.ContactInformation
                .FirstOrDefaultAsync(c => c.PassportPersonalInformationId == personal.PassportPersonalInformationId);

            if (contact == null)
                return NotFound();

            return Ok(contact);
        }

        // READ BY ID (UserID)
        [HttpGet("{personalId}")]
        public async Task<IActionResult> GetByUserId(int personalId)
        {
            var data = await _context.ContactInformation
                .FirstOrDefaultAsync(c => c.PassportPersonalInformationId == personalId);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // UPDATE
        [HttpPatch]
        public async Task<IActionResult> Update(ContactInformationDto dto)
        {
            var entity = await _context.ContactInformation
                .FirstOrDefaultAsync(c => c.PassportPersonalInformationId == dto.PassportPersonalInformationId);

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
    }
}