namespace SeniorCitizen.Server.DTOs
{
    public class PersoExportDto
    {
        // Basic Demographics
        public string? AgencyEmployeeNo { get; set; }
        public string? Surname { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? NameExtension { get; set; }
        public string? DateOfBirth { get; set; }
        public string? SexName { get; set; }
        public string? Citizenship { get; set; }
        public decimal? HeightCM { get; set; }
        public decimal? WeightKG { get; set; }
        public string? BloodType { get; set; }

        // Address and Contact
        public string? ResidentialAddress { get; set; }
        public string? ResidentialZip { get; set; }
        public string? PermanentAddress { get; set; }
        public string? PermanentZip { get; set; }
        public string? TelephoneNo { get; set; }
        public string? MobileNo { get; set; }
        public string? Email { get; set; }

        // Job Info (Consolidated from Unified Registry)
        public string? DepartmentName { get; set; }
        public string? Designation { get; set; }

        // Core Biometrics (Base64 Strings)
        public string? Photo { get; set; }
        public string? Signature { get; set; }

        // Standardized Fingerprint Sets
        // Renamed from Icao to Iso to match the new database schema
        public FingerprintSetDto Iso_Fingerprints { get; set; } = new();
        public FingerprintSetDto Afis_Fingerprints { get; set; } = new();

        // Iris / Facial features
        public string? EyeLeft { get; set; }
        public string? EyeRight { get; set; }
        public string? DateCapture { get; set; }
    }

    public class FingerprintSetDto
    {
        public string? LeftThumb { get; set; }
        public string? LeftIndex { get; set; }
        public string? LeftMiddle { get; set; }
        public string? LeftRing { get; set; }
        public string? LeftSmall { get; set; }
        public string? RightThumb { get; set; }
        public string? RightIndex { get; set; }
        public string? RightMiddle { get; set; }
        public string? RightRing { get; set; }
        public string? RightSmall { get; set; }
    }
}
