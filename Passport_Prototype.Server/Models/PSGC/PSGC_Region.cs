using SeniorCitizen.Server.Models.PSGC;

public class PSGC_Region
{
    public string RegionCode { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;

    // One region has many provinces
    public ICollection<PSGC_Province> Provinces { get; set; } = new List<PSGC_Province>();
}
