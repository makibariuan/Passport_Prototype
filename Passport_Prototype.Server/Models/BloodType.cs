using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRegistration.Server.Models
{
    [Table("BloodType")]
    public class BloodType
    {

        [Key]
        public int BloodTypeID { get; set; }

        [StringLength(50)]
        public string BloodTypeName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }
    }
}