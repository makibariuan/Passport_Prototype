using System.Text.Json.Serialization;

namespace SeniorCitizen.Server.Models.PSGC
{
    public class PSGC_Barangay
    {
        public string BarangayCode { get; set; } = string.Empty;
        public string BarangayName { get; set; } = string.Empty;
        public string CityMunCode { get; set; } = string.Empty;

        // Navigation to parent city/municipality
        [JsonIgnore]
        public PSGC_CityMunicipality? CityMunicipality { get; set; }
    }
}