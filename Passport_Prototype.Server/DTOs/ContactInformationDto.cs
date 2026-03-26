using System.ComponentModel.DataAnnotations;

namespace Passport_Prototype.Server.DTOs
{
    public class ContactInformationDto
    {
        public int PassportPersonalInformationId { get; set; }

        public string CurrentRegion { get; set; } = string.Empty;

        public string CurrentProvince { get; set; } = string.Empty;

        public string CurrentCityMunicipality { get; set; } = string.Empty;

        public string CurrentBarangay { get; set; } = string.Empty;

        public string CurrentPostalCode { get; set; } = string.Empty;

        public string PersonalMobileNumber { get; set; } = string.Empty;

        public string PersonalLandlineNumber { get; set; } = string.Empty;

        public string? Email { get; set; }
    }
}