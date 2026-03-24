using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineRegistration.Server.Models;

namespace Passport_Prototype.Server.Models
{
    [Table("PassportPersonalInformation")]
    public class PassportPersonalInformation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PassportPersonalInformationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }

        [Required]
        [MaxLength(255)]
        public string? FirstName { get; set; }

        [MaxLength(255)]
        public string? MiddleName { get; set; }

        [Required]
        [MaxLength(255)]
        public string? LastName { get; set; }

        [MaxLength(50)]
        public string? Suffix { get; set; }

        [Required]
        public DateTime? Birthdate { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Gender { get; set; }

        [MaxLength(100)]
        public string? Nationality { get; set; }

        [Required]
        public string? CivilStatus { get; set; }

        [Required]
        public bool hasPSABirthcert { get; set; }

        [Required]
        public bool isBirthLegitimate { get; set; }

        [Required]
        [MaxLength(100)]
        public string? BirthCountry { get; set; }

        [Required]
        [MaxLength(100)]
        public string? BirthRegion { get; set; }

        [Required]
        [MaxLength(100)]
        public string? BirthProvince { get; set; }

        [Required]
        [MaxLength(100)]
        public string? BirthCity { get; set; }

        [Required]
        [MaxLength(100)]
        public string? BirthBarangay { get; set; }
    }
}