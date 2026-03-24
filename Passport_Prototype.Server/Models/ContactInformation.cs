using OnlineRegistration.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace Passport_Prototype.Server.Models
{
    public class ContactInformation
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        //public Users? Users { get; set; }
        public required string CurrentRegion { get; set; }
        public required string CurrentProvince { get; set; }
        public required string CurrentCityMunicipality { get; set; }
        public required string CurrentBarangay { get; set; }
        public required string CurrentPostalCode { get; set; }
        public required string PersonalMobileNumber { get; set; }
        public required string PersonalLandlineNumber { get; set; }
        public string? Email {  get; set; }
    }
}
