using OnlineRegistration.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Passport_Prototype.Server.Models
{
    [Table("Family")]

    public class Family
    {
        [Key]
        public int FamilyId { get; set; }

        [Required]
        public int? UserId { get; set; }

        [Required]
        public string? Relationship { get; set; }

        [Required]
        public bool? isAlive { get; set; }

        [Required]
        public string? FirstName { get; set; }

        public string? MiddleName { get; set; }

        [Required]
        public string? LastName { get; set; }

        public string? Suffix { get; set; }

        public string? Citizenship { get; set; }
    }
}
