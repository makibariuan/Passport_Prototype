using System.ComponentModel.DataAnnotations;

namespace OnlineRegistration.Server.DTOs
{
    public class RegisterDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? EmployeeID { get; set; }
        public DateTime? BirthDate { get; set; }
        //public int? UserRole { get; set; }

        // --- Government ID Upload ---
        public string? GovIDType { get; set; }
        public string? GovIDNumber { get; set; }
        public string? IDImageBase64 { get; set; }
        public string? IDFileExtension { get; set; }
    }
}
