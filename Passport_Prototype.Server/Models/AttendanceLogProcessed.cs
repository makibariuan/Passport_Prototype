using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class AttendanceLogProcessed
{
    [Key]
    public int ProcessedId { get; set; }

    [Required]
    [MaxLength(100)]
    public string EmployeeID { get; set; }

    public string ApplicationCode { get; set; }

    [Required]
    public DateTime WorkDate { get; set; }

    public DateTime? MorningIn { get; set; }
    public DateTime? MorningOut { get; set; }
    public DateTime? BreakIn { get; set; }
    public DateTime? BreakOut { get; set; }
    public DateTime? AfternoonIn { get; set; }
    public DateTime? AfternoonOut { get; set; }
    public DateTime? OvertimeIn { get; set; }
    public DateTime? OvertimeOut { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal TotalRegularHours { get; set; } = 0;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal TotalOvertimeHours { get; set; } = 0;

    public string AttendanceStatus { get; set; } = "Present";

    public DateTime LastUpdated { get; set; } = DateTime.Now;
}