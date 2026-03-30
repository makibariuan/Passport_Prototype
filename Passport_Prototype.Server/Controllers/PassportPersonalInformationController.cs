using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using OnlineRegistration.Server.DTOs;
using OnlineRegistration.Server.Models;
using Passport_Prototype.Server.DTOs;
using Passport_Prototype.Server.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class PassportPersonalInformationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PassportPersonalInformationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateProfile(CreatePassportPersonalInformationDTO dto)
    {
        var userIdString = User.FindFirstValue("id");

        if (!int.TryParse(userIdString, out int userId))
            return BadRequest("Invalid user ID in claims.");

        await _context.PassportPersonalInformation.AddAsync(new PassportPersonalInformation
        {
            FirstName = "",
            LastName = "",
            Relationship = dto.Relationship,
            UserId = userId,
        });

        await _context.SaveChangesAsync();

        return Created();
    }

    // READ BY ID
    [HttpGet,Route("My-Profile")]
    public async Task<IActionResult> GetById()
    {
        var userIdString = User.FindFirstValue("id");

        if (!int.TryParse(userIdString, out int userId))
        {
            throw new Exception("Invalid user ID in claims.");
        }

        var passportPersonalInformation = await _context.PassportPersonalInformation.FirstOrDefaultAsync(p => p.UserId == userId && p.Relationship == null);

        if (passportPersonalInformation == null)
            return NotFound();

        return Ok(passportPersonalInformation);
    }

    [HttpGet("{personalId}")]
    public async Task<IActionResult> GetByPersonalId(int personalId)
    {
        var personal = await _context.PassportPersonalInformation
            .FirstOrDefaultAsync(p => p.PassportPersonalInformationId == personalId);

        if (personal == null)
            return NotFound();

        return Ok(personal);
    }

    // UPDATE
    [HttpPatch]
    public async Task<IActionResult> Update(UpdatePassportPersonalInformationDTO dto)
    {
        var passportPersonalInformation = await _context.PassportPersonalInformation.FirstOrDefaultAsync(p => p.PassportPersonalInformationId == dto.PassportPersonalInformationId);

        if (passportPersonalInformation == null)
            return NotFound();

        // Manual mapping
        passportPersonalInformation.FirstName = dto.FirstName;
        passportPersonalInformation.MiddleName = dto.MiddleName;
        passportPersonalInformation.LastName = dto.LastName;
        passportPersonalInformation.Suffix = dto.Suffix;
        passportPersonalInformation.Birthdate = dto.Birthdate;
        passportPersonalInformation.Gender = dto.Gender;
        passportPersonalInformation.Nationality = dto.Nationality;
        passportPersonalInformation.CivilStatusId = dto.CivilStatus;
        passportPersonalInformation.hasPSABirthcert = dto.HasPSABirthcert ?? false;
        passportPersonalInformation.BirthLegitimacy = dto.BirthLegitimacy;
        passportPersonalInformation.BirthCountry = dto.BirthCountry;
        passportPersonalInformation.BirthRegion = dto.BirthRegion;
        passportPersonalInformation.BirthProvince = dto.BirthProvince;
        passportPersonalInformation.BirthCity = dto.BirthCity;
        passportPersonalInformation.BirthBarangay = dto.BirthBarangay;
        passportPersonalInformation.IsAdoptee = dto.IsAdoptee;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}