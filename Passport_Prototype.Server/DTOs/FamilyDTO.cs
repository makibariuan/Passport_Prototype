using System.ComponentModel.DataAnnotations;

namespace Passport_Prototype.Server.DTOs
{
    public class FamilyMemberDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string? Relationship { get; set; }

        [Required]
        public bool? IsAlive { get; set; }

        [Required]
        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        [Required]
        public string LastName { get; set; } = null!;

        public string? Suffix { get; set; }

        public string? Citizenship { get; set; }
    }

    public class CreateFamilyDTO
    {
        public List<FamilyMemberDTO> FamilyMember { get; set; } = new List<FamilyMemberDTO>();
    }

    public class UpdateFamilyDTO
    {
        public string? Relationship { get; set; }

        public bool? IsAlive { get; set; }

        public string? FirstName { get; set; }

        public string? MiddleName { get; set; }

        public string? LastName { get; set; }

        public string? Suffix { get; set; }

        public string? Citizenship { get; set; }
    }
}
