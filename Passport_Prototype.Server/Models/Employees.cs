using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Employees", Schema = "dbo")] // Maps to the dbo.Employees table
public class Employee
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; } // PK, int, not null


    public string Surname { get; set; } // nvarchar(max), not null


    public string FirstName { get; set; } // nvarchar(max), not null

    public string? MiddleName { get; set; } // nvarchar(max), not null (assuming this needs to be nullable if the DB allows empty string/null)

    public DateTime DateOfBirth { get; set; } // datetime2(7), not null

    public string EmployeeID { get; set; } // nvarchar(max), not null

}

[Table("EmployeeWorkSchedules")]
public class EmployeeWorkSchedule
{
    [Key]
    public int ScheduleId { get; set; }
    public int DayOfWeek { get; set; }
    public bool IsRestDay { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation property: One schedule has many segments
    public virtual ICollection<ShiftSegment> ShiftSegments { get; set; } = new List<ShiftSegment>();
}

[Table("AttendanceLogs_Processed")]
public class AttendanceLogs_Processed
{
    [Key]
    public int ProcessedId { get; set; }
    public string EmployeeID { get; set; }
    public DateTime WorkDate { get; set; }

    // Actual Time Captures
    public DateTime? TimeIn { get; set; }
    public DateTime? TimeOut { get; set; }
    public DateTime? MorningIn { get; set; }
    public DateTime? MorningOut { get; set; }

    public DateTime? BreakIn { get; set; }
    public DateTime? BreakOut { get; set; }
    public DateTime? AfternoonIn { get; set; }
    public DateTime? AfternoonOut { get; set; }


    public decimal TotalRegularHours { get; set; }
    //public decimal TotalOvertimeHours { get; set; }
    public string AttendanceStatus { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;

}

public class DailyTimeRecord
{
    [Key]
    public int DtrId { get; set; }

    [Required]
    public string EmployeeID { get; set; }

    public DateTime WorkDate { get; set; }

    public int? ScheduleId { get; set; }

    public DateTime EffectiveDate { get; set; }

    public decimal TotalRegularHours { get; set; }

    public decimal TotalOvertimeHours { get; set; }

    public string AttendanceStatus { get; set; } // Present, Late, Undertime, Absent

    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

public class EmployeeScheduleAssignment
{
    [Key]
    public int AssignmentId { get; set; }
    public string EmployeeID { get; set; }
    public int TemplateId { get; set; } // Points to the new hierarchy
    public DateTime EffectiveDate { get; set; }
    public bool IsActive { get; set; } = true;

    [ForeignKey("TemplateId")]
    public virtual ScheduleTemplate Template { get; set; }
}

[Table("ScheduleTemplates")]
public class ScheduleTemplate
{
    [Key]
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } // e.g., "Standard Office Shift"
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // A template consists of multiple days (e.g., Monday through Friday)
    public virtual ICollection<DaySchedule> Days { get; set; } = new List<DaySchedule>();

}

[Table("DaySchedules")]
public class DaySchedule
{
    [Key]
    public int DayScheduleId { get; set; }

    public int TemplateId { get; set; } // Foreign Key to ScheduleTemplate
    public int DayOfWeek { get; set; }  // 0=Sun, 1=Mon, etc.
    public bool IsRestDay { get; set; }

    [ForeignKey("TemplateId")]
    public virtual ScheduleTemplate Template { get; set; }

    // Each day has its own collection of shift segments (Morning, Break, Afternoon, etc.)
    public virtual ICollection<ShiftSegment> ShiftSegments { get; set; } = new List<ShiftSegment>();
}

[Table("ShiftSegments")]
public class ShiftSegment
{
    [Key]
    public int SegmentId { get; set; }

    public int DayScheduleId { get; set; } // Updated FK
    public string ShiftType { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    [ForeignKey("DayScheduleId")]
    public virtual DaySchedule DaySchedule { get; set; }
}