using OnlineRegistration.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Passport_Prototype.Server.Models
{
    [Table("PersonalInformation")] 
    public class PassportPersonalInformation { 
        [Key] 
        public int PassportPersonalInformationId { get; set; } 
            
        [Required] 
        public int? UserId { get; set; } 
        public Users? User { get; set; } 
            
        [Required]
        public string? FirstName { get; set; } 
        public string? MiddleName { get; set; } 
           
        [Required]
        public string? LastName { get; set; } 
        public string? Suffix { get; set; } 
            
        [Required]
        public DateTime? Birthdate { get; set; } 
            
        [Required] 
        public string? Gender { get; set; } 
            
        [Required] 
        public string? Nationality { get; set; } 
            
        [Required]
        public string? CivilStatusId { get; set; }

        [Required]
        public bool? hasPSABirthcert { get; set; } 
            
        [Required]
        public bool? isBirthLegitimate { get; set; } 
            
        [Required]
        public string? BirthCountry { get; set; } 
            
        [Required]
        public string? BirthRegion { get; set; } 
            
        [Required]
        public string? BirthProvince { get; set; }
            
        [Required]
        public string? BirthCity { get; set; }

        [Required]
        public string? BirthBarangay { get; set; } 
    }
}
