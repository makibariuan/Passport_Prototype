using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace  OnlineRegistration.Server.Models
{
    public class ApplicationLog
    {
        [Key]
        [Column("Log_id")]
        [StringLength(50)]
        public string LogId { get; set; } = string.Empty;

        [Column("UserId")]
        public int? UserId { get; set; }

        [Column("Log_description")]
        [StringLength(50)]
        public string? LogDescription { get; set; }

        [Column("Log_time")]
        [StringLength(10)]
        public string? LogTime { get; set; }

        [Column("Log_date")]
        [StringLength(10)]
        public string? LogDate { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }
    }
}
