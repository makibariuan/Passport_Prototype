using System.ComponentModel.DataAnnotations;

namespace OnlineRegistration.Server.DTOs
{
    public class RegisterDto
    {
        // Account
        public string Email { get; set; }
        public string Password { get; set; }
        public string VerificationCode { get; set; }

        // Passport Info (Populating the full table)
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public string? Suffix { get; set; }
        public DateTime Birthdate { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }
        public string? CivilStatusId { get; set; }
        public bool HasPSABirthcert { get; set; }
        public bool IsBirthLegitimate { get; set; }
        public string BirthCountry { get; set; }
        public string BirthRegion { get; set; }
        public string BirthProvince { get; set; }
        public string BirthCity { get; set; }
        public string BirthBarangay { get; set; }
    }
}
