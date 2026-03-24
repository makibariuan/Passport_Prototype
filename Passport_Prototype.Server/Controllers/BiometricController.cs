using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRegistration.Server.Data;
using OnlineRegistration.Server.DTOs;
using OnlineRegistration.Server.Models;
using OnlineRegistration.Server.Services.Interfaces;
using System.Security.Claims;

namespace OnlineRegistration.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    [Authorize]

    public class BiometricController : ControllerBase
    {
        private readonly EmployeesDbContext _db;
        private readonly IConfiguration _config;
        private readonly IEmailQueue _emailQueue;
        public BiometricController(EmployeesDbContext db, IConfiguration config, IEmailQueue emailQueue)
        {
            _db = db;
            _config = config;
            _emailQueue = emailQueue;

        }

        /// <summary>
        /// Processes a new attendance punch from a biometric device.
        /// </summary>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Receives and processes a single biometric punch (log) from a device.
        /// </summary>
        //[HttpPost]
        //public async Task<ActionResult> PostAttendance([FromBody] EmployeeAttendanceDto dto)
        //{
        //    if (dto == null) return BadRequest("Invalid attendance data.");

        //    // 1. Save the Raw Log (Audit Trail)
        //    var attendance = new EmployeeAttendance
        //    {
        //        EmployeeID = dto.EmployeeID,
        //        BiometricDeviceID = dto.BiometricDeviceID,
        //        Mode = dto.Mode,
        //        Data = dto.Data,
        //        DateLog = dto.DateLog, // The time from the device
        //        EventType = dto.EventType,
        //    };
        //    _db.Attendance.Add(attendance);

        //    // 2. Find or Create the Processed Row for this day
        //    var today = dto.DateLog.Date;
        //    var log = await _db.AttendanceLogs_Processed
        //        .FirstOrDefaultAsync(l => l.EmployeeID.Trim() == dto.EmployeeID.Trim() && l.WorkDate == today);

        //    if (log == null)
        //    {
        //        log = new AttendanceLogs_Processed
        //        {
        //            EmployeeID = dto.EmployeeID,
        //            WorkDate = today,
        //            AttendanceStatus = "Present"
        //        };
        //        _db.AttendanceLogs_Processed.Add(log);
        //    }

        //    // 3. Get the Employee's Schedule to know where to put this punch
        //    var schedule = await _db.EmployeeWorkSchedules
        //        .FirstOrDefaultAsync(s => s.EmployeeID.Trim() == dto.EmployeeID.Trim() && s.DayOfWeek == (int)today.DayOfWeek);

        //    if (schedule != null)
        //    {
        //        var punchTime = dto.DateLog.TimeOfDay;

        //        // Logical Sloting (Same logic as your WFH controller)
        //        if (log.MorningIn == null && punchTime < schedule.MorningEnd)
        //            log.MorningIn = dto.DateLog;
        //        else if (log.MorningIn != null && log.MorningOut == null && punchTime < schedule.AfternoonStart)
        //            log.MorningOut = dto.DateLog;
        //        else if (log.AfternoonIn == null && punchTime < schedule.AfternoonEnd)
        //            log.AfternoonIn = dto.DateLog;
        //        else if (log.AfternoonIn != null && log.AfternoonOut == null)
        //            log.AfternoonOut = dto.DateLog;
        //    }

        //    // 4. Calculate Hours and Save
        //    CalculateHours(log); // Use your existing private helper method
        //    log.LastUpdated = DateTime.Now;

        //    await _db.SaveChangesAsync();
        //    return Ok(new { message = "Biometric log processed and hours updated." });
        //}

        private void CalculateHours(AttendanceLogs_Processed log)
        {
            decimal morningHours = 0;
            decimal afternoonHours = 0;
            decimal otHours = 0;

            // Calculate Morning
            if (log.MorningIn.HasValue && log.MorningOut.HasValue)
                morningHours = (decimal)(log.MorningOut.Value - log.MorningIn.Value).TotalHours;

            // Calculate Afternoon
            if (log.AfternoonIn.HasValue && log.AfternoonOut.HasValue)
                afternoonHours = (decimal)(log.AfternoonOut.Value - log.AfternoonIn.Value).TotalHours;

         

            log.TotalRegularHours = Math.Round(morningHours + afternoonHours, 2);
            //log.TotalOvertimeHours = Math.Round(otHours, 2);
        }
    }
}

//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using OnlineRegistration.Server.Data;
//using OnlineRegistration.Server.DTOs;
//using OnlineRegistration.Server.Models;
//using OnlineRegistration.Server.Services.Interfaces;

//namespace OnlineRegistration.Server.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class BiometricController : ControllerBase
//    {
//        private readonly EmployeesDbContext _db;
//        private readonly IConfiguration _config;
//        private readonly IEmailQueue _emailQueue;
//        public BiometricController(EmployeesDbContext db, IConfiguration config, IEmailQueue emailQueue)
//        {
//            _db = db;
//            _config = config;
//            _emailQueue = emailQueue;

//        }
//        // GET: api/<BiometricController>
//        [HttpGet]
//        public IEnumerable<string> Get()
//        {
//            return new string[] { "value1", "value2" };
//        }



//        // PUT api/<BiometricController>/5
//        [HttpPost]
//        public async Task<ActionResult> PostAttendance([FromBody] EmployeeAttendanceDto dto)
//        {
//            if (dto == null)
//                return BadRequest("Invalid attendance data.");
//            var attendance = new EmployeeAttendance
//            {
//                EmployeeID = dto.EmployeeID,
//                BiometricDeviceID = dto.BiometricDeviceID,
//                Mode = dto.Mode,
//                Data = dto.Data,
//                DateLog = dto.DateLog,
//                EventType = dto.EventType,
//            };

//            _db.Attendance.Add(attendance);
//            await _db.SaveChangesAsync();
//            return Ok(new { message = "Biometrics uploaded successfully" });
//        }
//    }
//}
