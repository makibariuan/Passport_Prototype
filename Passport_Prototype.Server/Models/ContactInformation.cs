using OnlineRegistration.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace Passport_Prototype.Server.Models
{
    public class ContactInformation
    {
        public int Id { get; set; }
        public int PassportPersonalInformationId { get; set; }
        public string? CurrentStreet { get; set; }
        public required string CurrentRegion { get; set; }
        public required string CurrentProvince { get; set; }
        public required string CurrentCityMunicipality { get; set; }
        public required string CurrentBarangay { get; set; }
        public required string CurrentPostalCode { get; set; }
        public required string CurrentCountry { get; set; }

        public string? PermanentCountry { get; set; }
        public string? PermanentPostalCode { get; set; }
        public string? PermanentRegion { get; set; }
        public string? PermanentProvince { get; set; }
        public string? PermanentCityMunicipality { get; set; }
        public string? PermanentBarangay { get; set; }
        public string? PermanentStreet { get; set; }

        public string? AddressAbroad { get; set; }
        public required string PersonalMobileNumber { get; set; }
        public required string PersonalLandlineNumber { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactNumber { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public string? Email {  get; set; }
    }
}
