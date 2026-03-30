using OnlineRegistration.Server.Models;

namespace Passport_Prototype.Server.Models
{
    public class WorkInformation
    {
        public int Id { get; set; }
        public int PassportPersonalInformationId { get; set; }
        public string? Occupation { get; set; }
        public string? Employer { get; set; }
        public string? OfficeAddress { get; set; }
        public string? OfficeBarangay { get; set; }
        public string? OfficeCountry { get; set; }
        public string? OfficeRegion { get; set; }
        public string? OfficeProvince { get; set; }
        public string? OfficeCityMunicipality { get; set; }
        public string? OfficePostalCode { get; set; }
        public string? OfficeMobileNumber { get; set; }
        public string? OfficeLandlineNumber { get; set; }
    }
}
