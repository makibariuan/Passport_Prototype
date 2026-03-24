using System.ComponentModel.DataAnnotations;

namespace OnlineRegistration.Server.DTOs
{

    // DTO for enrolling biometric data
    public class BiometricDataEnrollmentDto
    {
        public int Id { get; set; }
        public string? DateCapture { get; set; }
        public string ApplicationCode { get; set; }
        public string? Photo { get; set; }
        public string? Signature { get; set; }

        // --- ICAO Fingerprints ---
        public string? IcaoLeftThumb { get; set; }
        public string? IcaoLeftIndex { get; set; }
        public string? IcaoLeftMiddle { get; set; }
        public string? IcaoLeftRing { get; set; }
        public string? IcaoLeftSmall { get; set; }
        public string? IcaoRightThumb { get; set; }
        public string? IcaoRightIndex { get; set; }
        public string? IcaoRightMiddle { get; set; }
        public string? IcaoRightRing { get; set; }
        public string? IcaoRightSmall { get; set; }

        // --- AFIS Fingerprints ---
        public string? AfisLeftThumb { get; set; }
        public string? AfisLeftIndex { get; set; }
        public string? AfisLeftMiddle { get; set; }
        public string? AfisLeftRing { get; set; }
        public string? AfisLeftSmall { get; set; }
        public string? AfisRightThumb { get; set; }
        public string? AfisRightIndex { get; set; }
        public string? AfisRightMiddle { get; set; }
        public string? AfisRightRing { get; set; }
        public string? AfisRightSmall { get; set; }

        // --- Legacy Fingerprints (for backward compatibility) ---
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

        public string? EyeLeft { get; set; }
        public string? EyeRight { get; set; }
        public string? BiometricLeft { get; set; }
        public string? BiometricRight { get; set; }
        public string? KitUser { get; set; }
        public string? KitName { get; set; }
    }

    public class UnifiedBiometricsDto
    {
        public string ApplicationCode { get; set; }
        public int? Id { get; set; }
        public string KitUser { get; set; }
        public string KitName { get; set; }
        public DateTime? DateCapture { get; set; }
        public string Photo { get; set; }
        public string Signature { get; set; }

        // --- ISO Fingerprints (Renamed from Icao) ---
        public string? IsoLeftThumb { get; set; }
        public string? IsoLeftIndex { get; set; }
        public string? IsoLeftMiddle { get; set; }
        public string? IsoLeftRing { get; set; }
        public string? IsoLeftSmall { get; set; }
        public string? IsoRightThumb { get; set; }
        public string? IsoRightIndex { get; set; }
        public string? IsoRightMiddle { get; set; }
        public string? IsoRightRing { get; set; }
        public string? IsoRightSmall { get; set; }

        // --- AFIS Fingerprints ---
        public string? AfisLeftThumb { get; set; }
        public string? AfisLeftIndex { get; set; }
        public string? AfisLeftMiddle { get; set; }
        public string? AfisLeftRing { get; set; }
        public string? AfisLeftSmall { get; set; }
        public string? AfisRightThumb { get; set; }
        public string? AfisRightIndex { get; set; }
        public string? AfisRightMiddle { get; set; }
        public string? AfisRightRing { get; set; }
        public string? AfisRightSmall { get; set; }

        // Legacy/Generic Fingerprints (Raw Images)
        public string LeftThumb { get; set; }
        public string LeftIndex { get; set; }
        public string LeftMiddle { get; set; }
        public string LeftRing { get; set; }
        public string LeftSmall { get; set; }
        public string RightThumb { get; set; }
        public string RightIndex { get; set; }
        public string RightMiddle { get; set; }
        public string RightRing { get; set; }
        public string RightSmall { get; set; }

        public string? EyeLeft { get; set; }
        public string? EyeRight { get; set; }
        public string? BiometricLeft { get; set; }
        public string? BiometricRight { get; set; }
        public string? GovIDImage { get; set; }
    }


    public class PgpBiometricUploadDto
    {
        public string ApplicationCode { get; set; }
        public string EncryptedPgpBlock { get; set; } // The large PGP armored string
    }

    public class BiometricTemplateDataDto
    {
        // --- ISO TEMPLATES (10) ---
        public string? ISO_LeftThumb { get; set; }
        public string? ISO_LeftIndex { get; set; }
        public string? ISO_LeftMiddle { get; set; }
        public string? ISO_LeftRing { get; set; }
        public string? ISO_LeftSmall { get; set; }
        public string? ISO_RightThumb { get; set; }
        public string? ISO_RightIndex { get; set; }
        public string? ISO_RightMiddle { get; set; }
        public string? ISO_RightRing { get; set; }
        public string? ISO_RightSmall { get; set; }

        // --- AFIS TEMPLATES (10) ---
        public string? AFIS_LeftThumb { get; set; }
        public string? AFIS_LeftIndex { get; set; }
        public string? AFIS_LeftMiddle { get; set; }
        public string? AFIS_LeftRing { get; set; }
        public string? AFIS_LeftSmall { get; set; }
        public string? AFIS_RightThumb { get; set; }
        public string? AFIS_RightIndex { get; set; }
        public string? AFIS_RightMiddle { get; set; }
        public string? AFIS_RightRing { get; set; }
        public string? AFIS_RightSmall { get; set; }
    }

    public class PgpEncryptedRequest
    {
        public string EncryptedData { get; set; } = string.Empty;
    }
}

