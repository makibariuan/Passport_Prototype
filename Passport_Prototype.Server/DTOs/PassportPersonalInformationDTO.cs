using System.ComponentModel.DataAnnotations;

namespace Passport_Prototype.Server.DTOs
{
    public class CreatePassportPersonalInformationDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        [Required]
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
    }

    public class UpdatePassportPersonalInformationDTO
    {
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
