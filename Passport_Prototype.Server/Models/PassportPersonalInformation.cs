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

        [MaxLength(100)]
        public string? CurrentCountry { get; set; }

        [MaxLength(100)]
        public string? CurrentRegion { get; set; }

        [MaxLength(100)]
        public string? CurrentProvince { get; set; }

        [MaxLength(100)]
        public string? CurrentCity { get; set; }

        [MaxLength(100)]
        public string? CurrentBarangay { get; set; }

        [MaxLength(100)]
        public string? CurrentStreet { get; set; }

        [MaxLength(100)]
        public string? CurrentUnit { get; set; }

        [MaxLength(100)]
        public string? PermanentCountry { get; set; }

        [MaxLength(100)]
        public string? PermanentRegion { get; set; }

        [MaxLength(100)]
        public string? PermanentProvince { get; set; }

        [MaxLength(100)]
        public string? PermanentCity { get; set; }

        [MaxLength(100)]
        public string? PermanentBarangay { get; set; }

        [MaxLength(100)]
        public string? PermanentStreet { get; set; }

        [MaxLength(100)]
        public string? PermanentUnit { get; set; }

        public string? Relationship { get; set; }

        public bool? IsAdoptee { get; set; }
    }
}