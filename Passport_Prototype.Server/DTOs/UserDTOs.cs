using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnlineRegistration.Server.DTOs
{
    // =========================================================
    //               USER CREATION/UPDATE DTOs
    // =========================================================

    public class UserCreateDto
    {
        [Required]
        public int UserType { get; set; } 
        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int UserRole { get; set; }
        public string? Password { get; set; }

    }

    public class UserReadDto
    {
        public int Id { get; set; }
        public int UserType { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int UserRole { get; set; }
        public string RoleDesc { get; set; } = string.Empty;
        public string? LatestKitName { get; set; }
        public string? KitEncryptedPgpData { get; set; }

    }

    public class UpdateUserRoleRequestDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int NewUserRole { get; set; }
    }
}