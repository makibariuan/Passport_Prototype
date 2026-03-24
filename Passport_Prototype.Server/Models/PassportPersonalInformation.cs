using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineRegistration.Server.Models;

namespace Passport_Prototype.Server.Models
{
    [Table("PassportPersonalInformation")] // Updated to match schema
    public class PassportPersonalInformation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PassportPersonalInformationId { get; set; }

        [Required]
        public int UserId { get; set; } // Foreign Key to Users Table

        [ForeignKey("UserId")]
        public virtual Users? User { get; set; } // Navigation property

        [Required]
        [MaxLength(255)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? MiddleName { get; set; }

        [Required]
        [MaxLength(255)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Suffix { get; set; }

        [Required]
        public DateTime Birthdate { get; set; }

        [MaxLength(50)]
        public string? Gender { get; set; }

        [MaxLength(100)]
        public string? Nationality { get; set; }

        [Column("CivilStatus")] // This tells EF the DB column is named "CivilStatus"
        public string? CivilStatusId { get; set; }

        public bool hasPSABirthcert { get; set; }

        public bool isBirthLegitimate { get; set; }

        [MaxLength(100)]
        public string? BirthCountry { get; set; }

        [MaxLength(100)]
        public string? BirthRegion { get; set; }

        [MaxLength(100)]
        public string? BirthProvince { get; set; }

        [MaxLength(100)]
        public string? BirthCity { get; set; }

        [MaxLength(100)]
        public string? BirthBarangay { get; set; }
    }
}