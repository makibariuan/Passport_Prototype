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
            //CivilStatusId = dto.CivilStatusId,
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

        if (pageNumber.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip((pageNumber.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        var passportPersonalInformations = await query.ToListAsync();
        return Ok(passportPersonalInformations);
    }

    // READ BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var passportPersonalInformation = await _context.PassportPersonalInformation.FindAsync(id);

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
        if (dto.FirstName != null) passportPersonalInformation.FirstName = dto.FirstName;
        if (dto.MiddleName != null) passportPersonalInformation.MiddleName = dto.MiddleName;
        if (dto.LastName != null) passportPersonalInformation.LastName = dto.LastName;
        if (dto.Suffix != null) passportPersonalInformation.Suffix = dto.Suffix;
        if (dto.Birthdate.HasValue) passportPersonalInformation.Birthdate = dto.Birthdate.Value;
        if (dto.Gender != null) passportPersonalInformation.Gender = dto.Gender;
        if (dto.Nationality != null) passportPersonalInformation.Nationality = dto.Nationality;
        //if (dto.CivilStatusId != null) passportPersonalInformation.CivilStatusId = dto.CivilStatusId;
        if (dto.HasPSABirthcert.HasValue) passportPersonalInformation.hasPSABirthcert = dto.HasPSABirthcert.Value;
        if (dto.IsBirthLegitimate.HasValue) passportPersonalInformation.isBirthLegitimate = dto.IsBirthLegitimate.Value;
        if (dto.BirthCountry != null) passportPersonalInformation.BirthCountry = dto.BirthCountry;
        if (dto.BirthRegion != null) passportPersonalInformation.BirthRegion = dto.BirthRegion;
        if (dto.BirthProvince != null) passportPersonalInformation.BirthProvince = dto.BirthProvince;
        if (dto.BirthCity != null) passportPersonalInformation.BirthCity = dto.BirthCity;
        if (dto.BirthBarangay != null) passportPersonalInformation.BirthBarangay = dto.BirthBarangay;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var passportPersonalInformation = await _context.PassportPersonalInformation.FindAsync(id);

        if (passportPersonalInformation == null)
            return NotFound();

        _context.PassportPersonalInformation.Remove(passportPersonalInformation);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}