using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data; 
using OnlineRegistration.Server.Models;
using OnlineRegistration.Server.DTOs;
using System.Threading.Tasks;

namespace OnlineRegistration.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AFISController : ControllerBase
    {
        // Changed to AppDbContext for citizen data
        private readonly AppDbContext _context; // Biometric/Citizen DB
        private readonly EmployeesDbContext _db;  // HR/Employee DB
        private readonly IConfiguration _config;

        public AFISController(AppDbContext context, EmployeesDbContext db, IConfiguration config)
        {
            _context = context;
            _db = db;
            _config = config;
        }

        /// <summary>
        /// Records a biometric "Hit" when a duplicate identity is detected by AFIS.
        /// </summary>
        /// 
        [HttpPost("hit")]
        public async Task<IActionResult> UpdatePersonWithHit([FromBody] PersonWithHitDto personWithHit)
        {
            var currentUsername = "AFIS_SERVICE";

            // Use a transaction to ensure both DBs update or neither does
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var enrollment = await _db.EnrollmentRegistries
                    .FirstOrDefaultAsync(p => p.PersonID == personWithHit.PersonA);

                if (enrollment == null)
                {
                    LogAFISResult("AFIS_HIT_FAILURE_NOT_FOUND", personWithHit.PersonA, "BDE ID not found.", currentUsername);
                    await _context.SaveChangesAsync();
                    return NotFound(new { message = "Enrollment record not found" });
                }

                // 1. Update Enrollment
                enrollment.Status = 3;
                enrollment.BiometricStatus = 3;
                enrollment.AFISHit = 1;
                enrollment.AFISPersonHit = personWithHit.PersonB;
                enrollment.AFISDateProcess = personWithHit.DateDetected;


                LogAFISResult("AFIS_HIT_SUCCESS", personWithHit.PersonA, $"Duplicate for: {enrollment.ApplicationCode}.", currentUsername);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync(); // Finalize both updates
                return Ok(new { message = "Hit records synced." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error syncing AFIS hit", details = ex.Message });
            }
        }

        /// <summary>
        /// Clears an applicant for ID issuance after a successful "No-Hit" biometric search.
        /// </summary>
        [HttpPost("validated")]
        public async Task<IActionResult> UpdatePersonNoHit([FromBody] PersonValidatedDto personValidated)
        {
            var currentUsername = "AFIS_SERVICE";

            // 1. Update Biometric Enrollment Table (_context)
            var enrollment = await _db.EnrollmentRegistries
                .FirstOrDefaultAsync(p => p.PersonID == personValidated.PersonId);

            if (enrollment == null)
            {
                LogAFISResult("AFIS_VALIDATED_FAILURE_NOT_FOUND", personValidated.PersonId, "Enrollment ID not found.", currentUsername);
                await _context.SaveChangesAsync();
                return BadRequest(new { message = "Enrollment record not found" });
            }

            // Skip if a hit was already manually or previously identified
            if (enrollment.AFISHit == 1)
            {
                return Ok(new { message = "Skipped: Record already marked as HIT" });
            }

            // 🔥 FIX: If the person already has a HIT (Status 3), 
            // DO NOT let a "Validated" message from a different finger clear them.
            if (enrollment.BiometricStatus == 3 || enrollment.AFISHit == 1)
            {
                return Ok(new { message = "Keep as Hit. This finger was clear, but others matched." });
            }

            enrollment.Status = 4;
            enrollment.BiometricStatus = 4; // Status 4 = Validated/Cleared
            enrollment.AFISHit = 0;
            enrollment.AFISDateProcess = personValidated.ValidatedAt;

  
            LogAFISResult("AFIS_VALIDATED_SUCCESS", personValidated.PersonId, $"Validation successful for Code: {enrollment.ApplicationCode}", currentUsername);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Validation records updated in both tables" });
        }

        private void LogAFISResult(string description, int personId, string resultDetails, string accessedByUsername)
        {
            var logEntry = new ApplicationLog
            {
                LogId = Guid.NewGuid().ToString("N"),
                LogDescription = description,
                LogDate = DateTime.Now.ToString("yyyy-MM-dd"),
                LogTime = DateTime.Now.ToString("HH:mm:ss"),
                Notes = $"BDE ID: {personId}. Result: {resultDetails}. Performed by: {accessedByUsername}"
            };
            _context.ApplicationLogs.Add(logEntry);
        }
    }
}


//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using OnlineRegistration.Server.Data;
//using OnlineRegistration.Server.Models;
//using OnlineRegistration.Server.Services.Interfaces;
//using OnlineRegistration.Server.DTOs;
//using System.Text.Json;

//namespace OnlineRegistration.Server.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AFISController : ControllerBase
//    {
//        private readonly EmployeesDbContext _db;
//        private readonly IConfiguration _config;

//        public AFISController(EmployeesDbContext db, IConfiguration config)
//        {
//            _db = db;
//            _config = config;

//        }

//        [HttpPost("hit")]
//        public async Task<IActionResult> UpdatePersonWithHit([FromBody] PersonWithHitDto personWithHit)
//        {
//            var person = await _db.EmployeeIDApplications
//                .FirstOrDefaultAsync(p => p.PersonID == personWithHit.PersonA);

//            if (person == null)
//            {
//                return BadRequest(new { message = "Person not found" });
//            }

//            person.Status = 3;
//            person.AFISHit = 1; // found duplicate
//            person.AFISPersonHit = personWithHit.PersonB;
//            person.AFISDateProcess = personWithHit.DateDetected;
//            await _db.SaveChangesAsync();

//            return Ok(new { message = "Record updated" });
//        }

//        [HttpPost("validated")]
//        public async Task<IActionResult> UpdatePersonNoHit([FromBody] PersonValidatedDto personValidated)
//        {
//            var person = await _db.EmployeeIDApplications
//                .FirstOrDefaultAsync(p => p.PersonID == personValidated.PersonId);

//            if (person == null)
//            {
//                return BadRequest(new { message = "Person not found" });
//            }

//            if(person.AFISPersonHit != null)
//                return Ok(new { message = "Record not updated due to hit" });

//            person.Status = 4;
//            person.AFISHit = 0; // found duplicate
//            person.AFISDateProcess = personValidated.ValidatedAt;
//            await _db.SaveChangesAsync();

//            return Ok(new { message = "Record updated" });
//        }

//    }
//}
