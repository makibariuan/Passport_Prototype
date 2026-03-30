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

        public int UserId { get; set; }

        [MaxLength(255)]
        public string? FirstName { get; set; }

        [MaxLength(255)]
        public string? MiddleName { get; set; }

        [MaxLength(255)]
        public string? LastName { get; set; }

        [MaxLength(50)]
        public string? Suffix { get; set; }

        public DateTime? Birthdate { get; set; }

        [MaxLength(50)]
        public string? Gender { get; set; }

        [MaxLength(100)]
        public string? Nationality { get; set; }

        [Column("CivilStatus")] // This tells EF the DB column is named "CivilStatus"
        public string? CivilStatusId { get; set; }

        public bool? hasPSABirthcert { get; set; }

        public string? BirthLegitimacy { get; set; }

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

        public string? Relationship { get; set; }

        public bool? IsAdoptee { get; set; }
    }
}