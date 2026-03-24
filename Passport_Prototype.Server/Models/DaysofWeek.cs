using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OnlineRegistration.Server.Models
{
    [Table("Ref_DaysOfWeek")]
    public class Ref_DaysOfWeek
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // We want to manually set 0-6
        public int DayID { get; set; }

        [Required]
        [StringLength(15)]
        public string DayName { get; set; } = string.Empty;

        [Required]
        [StringLength(3)]
        public string ShortName { get; set; } = string.Empty;

        [Required]
        public int SortOrder { get; set; }
    }
}
