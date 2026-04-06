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

        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactNumber { get; set; }

        public string? EmergencyContactRelationship { get; set; }

        public string? PermanentCountry { get; set; }

        public string? PermanentPostalCode { get; set; }

        public string? PermanentRegion { get; set; }

        public string? PermanentProvince { get; set; }

        public string? PermanentCityMunicipality { get; set; }

        public string? PermanentBarangay { get; set; }

        public string? PermanentStreet { get; set; }

        public string? Email { get; set; }
    }
}