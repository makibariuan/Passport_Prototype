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
    public class WorkInformationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WorkInformationController(AppDbContext context)
        {
            _context = context;
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(WorkInformationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = new WorkInformation
            {
                PassportPersonalInformationId = dto.PassportPersonalInformationId,
                Occupation = dto.Occupation,
                OfficeAddress = dto.OfficeAddress,
                OfficeCountry = dto.OfficeCountry,
                OfficeRegion = dto.OfficeRegion,
                OfficeProvince = dto.OfficeProvince,
                OfficeCityMunicipality = dto.OfficeCityMunicipality,
                OfficePostalCode = dto.OfficePostalCode,
                OfficeMobileNumber = dto.OfficeMobileNumber,
                OfficeLandlineNumber = dto.OfficeLandlineNumber
            };

            _context.WorkInformation.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // READ ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.WorkInformation
                //.Include(w => w.UserId)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("My-Work")]
        public async Task<IActionResult> GetMyWork()
        {
            var userIdString = User.FindFirstValue("id");

            if (!int.TryParse(userIdString, out int userId))
                return BadRequest("Invalid user ID.");

            var personal = await _context.PassportPersonalInformation
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Relationship == null);

            if (personal == null)
                return NotFound();

            var work = await _context.WorkInformation
                .FirstOrDefaultAsync(w => w.PassportPersonalInformationId == personal.PassportPersonalInformationId);

            if (work == null)
                return NotFound();

            return Ok(work);
        }

        // READ BY ID
        [HttpGet("{personalId}")]
        public async Task<IActionResult> GetById(int personalId)
        {
            var data = await _context.WorkInformation
                //.Include(w => w.Users)
                .FirstOrDefaultAsync(w => w.PassportPersonalInformationId == personalId);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // UPDATE
        [HttpPatch]
        public async Task<IActionResult> Update(WorkInformationDto dto)
        {
            var entity = await _context.WorkInformation
                .FirstOrDefaultAsync(w => w.PassportPersonalInformationId == dto.PassportPersonalInformationId);

            if (entity == null)
                return NotFound();

            entity.PassportPersonalInformationId = dto.PassportPersonalInformationId;
            entity.Occupation = dto.Occupation;
            entity.OfficeAddress = dto.OfficeAddress;
            entity.OfficeCountry = dto.OfficeCountry;
            entity.OfficeRegion = dto.OfficeRegion;
            entity.OfficeProvince = dto.OfficeProvince;
            entity.OfficeCityMunicipality = dto.OfficeCityMunicipality;
            entity.OfficePostalCode = dto.OfficePostalCode;
            entity.OfficeMobileNumber = dto.OfficeMobileNumber;
            entity.OfficeLandlineNumber = dto.OfficeLandlineNumber;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // DELETE
        [HttpDelete("{personalId}")]
        public async Task<IActionResult> Delete(int personalId)
        {
            var entity = await _context.WorkInformation
                .FirstOrDefaultAsync(w => w.PassportPersonalInformationId == personalId);

            if (entity == null)
                return NotFound();

            _context.WorkInformation.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}