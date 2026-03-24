using System.Text.Json.Serialization;

namespace SeniorCitizen.Server.Models.PSGC;
public class PSGC_Province
{
    public string ProvinceCode { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public string RegionCode { get; set; } = string.Empty;

    // Navigation to parent region
    [JsonIgnore]
    public PSGC_Region? Region { get; set; }

    // One province has many cities/municipalities
    public ICollection<PSGC_CityMunicipality> Cities { get; set; } = new List<PSGC_CityMunicipality>();
}
