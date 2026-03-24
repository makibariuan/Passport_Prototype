using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using OnlineRegistration.Server.DTOs;
using OnlineRegistration.Server.Models;
using Passport_Prototype.Server.DTOs;
using Passport_Prototype.Server.Models;

[ApiController]
[Route("api/[controller]")]
public class PassportPersonalInformationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PassportPersonalInformationsController(AppDbContext context)
    {
        _context = context;
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> Create(CreatePassportPersonalInformationDTO dto)
    {
        var passportPersonalInformation = new PassportPersonalInformation
        {
            UserId = dto.UserId,
            FirstName = dto.FirstName,
            MiddleName = dto.MiddleName,
            LastName = dto.LastName,
            Suffix = dto.Suffix,
            Birthdate = dto.Birthdate,
            Gender = dto.Gender,
            Nationality = dto.Nationality,
            CivilStatus = dto.CivilStatus,
            hasPSABirthcert = dto.HasPSABirthcert,
            isBirthLegitimate = dto.IsBirthLegitimate,
            BirthCountry = dto.BirthCountry,
            BirthRegion = dto.BirthRegion,
            BirthProvince = dto.BirthProvince,
            BirthCity = dto.BirthCity,
            BirthBarangay = dto.BirthBarangay
        };

        var newPassportPersonalInformation = await _context.PassportPersonalInformation.AddAsync(passportPersonalInformation);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = newPassportPersonalInformation.Entity.PassportPersonalInformationId }, newPassportPersonalInformation.Entity);
    }

    // READ
    [HttpGet]
    public async Task<IActionResult> GetAll(int? pageNumber, int? pageSize)
    {
        var query = _context.PassportPersonalInformation.AsQueryable();

        var totalPages = 1;
        var totalDocuments = await query.CountAsync();

        if (pageNumber.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip((pageNumber.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);

            totalPages = (int)totalDocuments / (int)pageSize;
        }

        var passportPersonalInformations = await query.ToListAsync();
        return Ok(new { totalPages, totalDocuments, passportPersonalInformations, });
    }

    // READ BY ID
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetById(int userId)
    {
        var passportPersonalInformation = await _context.PassportPersonalInformation.FirstOrDefaultAsync(p => p.UserId == userId);

        if (passportPersonalInformation == null)
            return NotFound();

        return Ok(passportPersonalInformation);
    }

    // UPDATE
    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, UpdatePassportPersonalInformationDTO dto)
    {
        var passportPersonalInformation = await _context.PassportPersonalInformation.FindAsync(id);

        if (passportPersonalInformation == null)
            return NotFound();

        // Manual mapping
        passportPersonalInformation.FirstName = dto.FirstName;
        passportPersonalInformation.MiddleName = dto.MiddleName;
        passportPersonalInformation.LastName = dto.LastName;
        passportPersonalInformation.Suffix = dto.Suffix;
        passportPersonalInformation.Birthdate = dto.Birthdate.Value;
        passportPersonalInformation.Gender = dto.Gender;
        passportPersonalInformation.Nationality = dto.Nationality;
        passportPersonalInformation.CivilStatus = dto.CivilStatus;
        passportPersonalInformation.hasPSABirthcert = dto.HasPSABirthcert.Value;
        passportPersonalInformation.isBirthLegitimate = dto.IsBirthLegitimate.Value;
        passportPersonalInformation.BirthCountry = dto.BirthCountry;
        passportPersonalInformation.BirthRegion = dto.BirthRegion;
        passportPersonalInformation.BirthProvince = dto.BirthProvince;
        passportPersonalInformation.BirthCity = dto.BirthCity;
        passportPersonalInformation.BirthBarangay = dto.BirthBarangay;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}