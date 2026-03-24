using System.ComponentModel.DataAnnotations;

namespace Passport_Prototype.Server.DTOs
{
    public class WorkInformationDto
    {
        public int? UserId { get; set; }

        public string? Occupation { get; set; }

        public string? OfficeAddress { get; set; }

        public string? OfficeCountry { get; set; }

        public string? OfficeRegion { get; set; }

        public string? OfficeProvince { get; set; }

        public string? OfficeCityMunicipality { get; set; }

        public string? OfficePostalCode { get; set; }

        //[Phone]
        public string? OfficeMobileNumber { get; set; }

        //[Phone]
        public string? OfficeLandlineNumber { get; set; }
    }
}