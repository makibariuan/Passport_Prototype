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

        public int PassportPersonalInformationId { get; set; }

        public string? Relationship { get; set; }

        public bool? isAlive { get; set; }

        public string? FirstName { get; set; }

        public string? MiddleName { get; set; }

        public string? LastName { get; set; }

        public string? Suffix { get; set; }

        public string? Citizenship { get; set; }
    }
}
