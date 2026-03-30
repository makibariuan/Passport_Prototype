using System.ComponentModel.DataAnnotations;

namespace Passport_Prototype.Server.DTOs
{
    public class ContactInformationDto
    {
        public int PassportPersonalInformationId { get; set; }

        public string CurrentStreet {  get; set; } = string.Empty;

        public string CurrentRegion { get; set; } = string.Empty;

        public string CurrentProvince { get; set; } = string.Empty;

        public string CurrentCityMunicipality { get; set; } = string.Empty;

        public string CurrentBarangay { get; set; } = string.Empty;

        public string CurrentPostalCode { get; set; } = string.Empty;

        public required string CurrentCountry { get; set; }

        public string? AddressAbroad { get; set; }

        public string PersonalMobileNumber { get; set; } = string.Empty;

        public string PersonalLandlineNumber { get; set; } = string.Empty;

        public string? Email { get; set; }
    }
}