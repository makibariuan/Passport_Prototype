using System.Text.Json.Serialization;

namespace SeniorCitizen.Server.Models.PSGC
{
    public class PSGC_CityMunicipality
    {
        public string CityMunCode { get; set; } = string.Empty;
        public string CityMunName { get; set; } = string.Empty;
        public string ProvinceCode { get; set; } = string.Empty;
        public string RegionCode { get; set; } = string.Empty;
        public bool IsCity { get; set; }

        // Navigation to parent province
        [JsonIgnore]
        public PSGC_Province? Province { get; set; }

        // One city/municipality has many barangays
        public ICollection<PSGC_Barangay> Barangays { get; set; } = new List<PSGC_Barangay>();
    }
}