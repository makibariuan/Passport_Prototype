using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using Passport_Prototype.Server.DTOs;
using Passport_Prototype.Server.Models;

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
                //UserID = dto.UserID,
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
                .Include(w => w.Users)
                .ToListAsync();

            return Ok(data);
        }

        // READ BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _context.WorkInformation
                .Include(w => w.Users)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // UPDATE
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, WorkInformationDto dto)
        {
            var entity = await _context.WorkInformation
                .FirstOrDefaultAsync(w => w.Id == id);

            if (entity == null)
                return NotFound();

            entity.UserID = dto.UserID;
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.WorkInformation
                .FirstOrDefaultAsync(w => w.Id == id);

            if (entity == null)
                return NotFound();

            _context.WorkInformation.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}