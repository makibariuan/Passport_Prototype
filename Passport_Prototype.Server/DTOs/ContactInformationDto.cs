using System.ComponentModel.DataAnnotations;

namespace Passport_Prototype.Server.DTOs
{
    public class ContactInformationDto
    {
        public int? UserID { get; set; }

        [Required]
        public string CurrentRegion { get; set; } = string.Empty;

        [Required]
        public string CurrentProvince { get; set; } = string.Empty;

        [Required]
        public string CurrentCityMunicipality { get; set; } = string.Empty;

        [Required]
        public string CurrentBarangay { get; set; } = string.Empty;

        [Required]
        public string CurrentPostalCode { get; set; } = string.Empty;

        [Required]
        //[Phone]
        public string PersonalMobileNumber { get; set; } = string.Empty;

        //[Phone]
        public string PersonalLandlineNumber { get; set; } = string.Empty;

        //[EmailAddress]
        public string? Email { get; set; }
    }
}