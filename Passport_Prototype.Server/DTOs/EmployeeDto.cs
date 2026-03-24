namespace OnlineRegistration.Server.DTOs
{
    public class EmployeeListDto
    {
        // Id is mapped from PersonID in the EmployeeIDApplication table
        public int PersonID { get; set; }
        public string? EmployeeID { get; set; }
        public string? FullName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Designation { get; set; }
        public string? DepartmentName { get; set; }
        public int? Status { get; set; } // Added Status field
        public string? DateSchedule { get; set; }
    }
    public class EmployeeIDPrintDto
    {
        public int PersonID { get; set; }
        public string ApplicationCode { get; set; }
        public string Department { get; set; }
        public string FullName { get; set; }
        public string DateCapture { get; set; }
        public string Photo { get; set; }
    }
    public class EmployeeIDApplicationDto
    {
        public int PersonID { get; set; }
        public string? ApplicationCode { get; set; }
        public string? Surname { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? EmployeeID { get; set; }
        public int? DepartmentID { get; set; }
        public string? DepartmentName { get; set; }
        public string? Designation { get; set; }
        public bool? PrintDesignation { get; set; } = false;
        public DateTime? DateSchedule { get; set; }
    }

    public class EmployeeIDBiometricsDto
    {
        public int PersonID { get; set; }
        public string? ApplicationCode { get; set; }
        public string? Photo { get; set; }
        public string? Signature { get; set; }
        public string? LeftThumb { get; set; }
        public string? LeftIndex { get; set; }
        public string? LeftMiddle { get; set; }
        public string? LeftRing { get; set; }
        public string? LeftSmall { get; set; }
        public string? RightThumb { get; set; }
        public string? RightIndex { get; set; }
        public string? RightMiddle { get; set; }
        public string? RightRing { get; set; }
        public string? RightSmall { get; set; }
        public string? EyeLeft { get; set; }
        public string? EyeRight { get; set; }
        public string? BiometricLeft { get; set; }
        public string? BiometricRight { get; set; }

        public DateTime? DateCapture { get; set; }
    }


    //biometrics employee logs

    public class EmployeeAttendanceDto
    {
        public string EmployeeID { get; set; }
        public string BiometricDeviceID { get; set; }
        public int Mode { get; set; }
        public string? Data { get; set; }
        public DateTime DateLog { get; set; }
        public string? EventType { get; set; }

    }

    public class AdjudicationDecisionDto
    {
        public string ApplicationCode { get; set; }
        public bool IsConfirmedHit { get; set; } // True = Duplicate, False = Valid/Clear
        public string Remarks { get; set; }      // Reason for the decision
    }



    //Work Schedule DTOs
    // --- Request DTO (Remains the same as your structure) ---
    public class WorkScheduleRequest
    {
        public string EmployeeID { get; set; }
        public DateTime EffectiveDate { get; set; }
        public List<DayPatternDto> WeeklyPattern { get; set; }
    }

    public class DayPatternDto
    {
        public DayOfWeek? Day { get; set; }
        public bool IsRestDay { get; set; }
        public string? MorningStart { get; set; }
        public string? MorningEnd { get; set; }

        public string? BreakIn { get; set; }
        public string? BreakOut { get; set; }
        public string? AfternoonStart { get; set; }
        public string? AfternoonEnd { get; set; }
        public string? OvertimeStart { get; set; }
        public string? OvertimeEnd { get; set; }
    }

    public class DisplayScheduleDto
    {
        public string EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public DateTime EffectiveDate { get; set; }
        public List<DayScheduleView> WeeklySchedule { get; set; }
    }

    public class DayScheduleView
    {
        public string DayName { get; set; }
        public bool IsRestDay { get; set; }
        public List<ShiftView> Shifts { get; set; } = new();
    }

    public class ShiftView
    {
        public string ShiftName { get; set; } // e.g., "Morning", "Night"
        public string TimeRange { get; set; } // e.g., "08:00 AM - 05:00 PM"
    }

    // --- 1. USED BY: POST /shift-templates ---
    public class BulkScheduleTemplateDto
    {
        public string TemplateName { get; set; }
        public List<DayScheduleDto> Days { get; set; } = new();
    }

    public class DayScheduleDto
    {
        public List<int> DaysOfWeek { get; set; } // e.g., [1, 3, 5]
        public bool IsRestDay { get; set; }
        public List<ShiftSegmentDto> Shifts { get; set; } = new();
    }

    public class ShiftSegmentDto
    {
        public string ShiftType { get; set; } // e.g., "Morning", "Break", "Afternoon"
        public string StartTime { get; set; } // "HH:mm"
        public string EndTime { get; set; }   // "HH:mm"
    }

    // --- 2. USED BY: POST /assign-shift ---
    public class AssignShiftDto
    {
        public string EmployeeID { get; set; }
        public int TemplateId { get; set; } // Updated from ScheduleId to match hierarchy
        public DateTime EffectiveDate { get; set; }
    }

}
