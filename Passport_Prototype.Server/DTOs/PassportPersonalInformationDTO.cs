using OnlineRegistration.Server.Models;
using SixLabors.ImageSharp.Formats.Gif;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Passport_Prototype.Server.DTOs
{
    public class PassportPersonalInformationDTO
    {
        public int PassportPersonalInformationId { get; set; }

        [Required]
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
    }
    public class CreatePassportPersonalInformationDTO
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = null!;

        public string? Suffix { get; set; }

        public DateTime Birthdate { get; set; }

        public string Gender { get; set; } = null!;

        public string Nationality { get; set; } = null!;

        public string CivilStatus { get; set; } = null!;

        public bool HasPSABirthcert { get; set; }

        public string BirthLegitimacy { get; set; }

        public string BirthCountry { get; set; } = null!;

        public string BirthRegion { get; set; } = null!;

        public string BirthProvince { get; set; } = null!;

        public string BirthCity { get; set; } = null!;

        public string BirthBarangay { get; set; } = null!;

        public string Relationship { get; set; }
    }

    public class UpdatePassportPersonalInformationDTO
    {
        public int? PassportPersonalInformationId { get; set; }

        public string? FirstName { get; set; }

        public string? MiddleName { get; set; }

        public string? LastName { get; set; }

        public string? Suffix { get; set; }

        public DateTime? Birthdate { get; set; }

        public string? Gender { get; set; }

        public string? Nationality { get; set; }

        public string? CivilStatus { get; set; }

        public bool? HasPSABirthcert { get; set; }

        public string? BirthLegitimacy { get; set; }

        public string? BirthCountry { get; set; }

        public string? BirthRegion { get; set; }

        public string? BirthProvince { get; set; }

        public string? BirthCity { get; set; }

        public string? BirthBarangay { get; set; }
    }
}
