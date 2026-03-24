using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRegistration.Server.Models
{
    [Table("AppointmentQuotas")]
    public class AppointmentQuota
    {
        [Required]
        [StringLength(100)]
        public string SiteName { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public TimeSpan ScheduledTime { get; set; }

        [Required]
        public int MaxSlots { get; set; }
    }
}