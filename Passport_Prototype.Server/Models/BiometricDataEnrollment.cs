using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRegistration.Server.Models
{
    [Table("BiometricDataEnrollment")]
    public class BiometricDataEnrollment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? CitizenID { get; set; }

        [Required]
        [MaxLength(50)]
        public string ApplicationCode { get; set; } = string.Empty;

        // --- Date Fields ---
        public DateTime? DateCapture { get; set; }
        public DateTime? DateUpload { get; set; }

        // --- Biometric Data Fields ---
        public string? Photo { get; set; }
        public string? Signature { get; set; }

        // --- Left Hand Fingerprints ---
        public string? LeftThumb { get; set; }
        public string? LeftIndex { get; set; }
        public string? LeftMiddle { get; set; }
        public string? LeftRing { get; set; }
        public string? LeftSmall { get; set; }

        // --- Right Hand Fingerprints ---
        public string? RightThumb { get; set; }
        public string? RightIndex { get; set; }
        public string? RightMiddle { get; set; }
        public string? RightRing { get; set; }
        public string? RightSmall { get; set; }

        // --- ICAO Fingerprints (Images) ---
        [Column("ICAO_LeftThumb")] public string? IcaoLeftThumb { get; set; }
        [Column("ICAO_LeftIndex")] public string? IcaoLeftIndex { get; set; }
        [Column("ICAO_LeftMiddle")] public string? IcaoLeftMiddle { get; set; }
        [Column("ICAO_LeftRing")] public string? IcaoLeftRing { get; set; }
        [Column("ICAO_LeftSmall")] public string? IcaoLeftSmall { get; set; }
        [Column("ICAO_RightThumb")] public string? IcaoRightThumb { get; set; }
        [Column("ICAO_RightIndex")] public string? IcaoRightIndex { get; set; }
        [Column("ICAO_RightMiddle")] public string? IcaoRightMiddle { get; set; }
        [Column("ICAO_RightRing")] public string? IcaoRightRing { get; set; }
        [Column("ICAO_RightSmall")] public string? IcaoRightSmall { get; set; }

        // --- AFIS Fingerprints (Templates) ---
        [Column("AFIS_LeftThumb")] public string? AfisLeftThumb { get; set; }
        [Column("AFIS_LeftIndex")] public string? AfisLeftIndex { get; set; }
        [Column("AFIS_LeftMiddle")] public string? AfisLeftMiddle { get; set; }
        [Column("AFIS_LeftRing")] public string? AfisLeftRing { get; set; }
        [Column("AFIS_LeftSmall")] public string? AfisLeftSmall { get; set; }
        [Column("AFIS_RightThumb")] public string? AfisRightThumb { get; set; }
        [Column("AFIS_RightIndex")] public string? AfisRightIndex { get; set; }
        [Column("AFIS_RightMiddle")] public string? AfisRightMiddle { get; set; }
        [Column("AFIS_RightRing")] public string? AfisRightRing { get; set; }
        [Column("AFIS_RightSmall")] public string? AfisRightSmall { get; set; }


        // --- Eye/Iris Data ---
        public string? EyeLeft { get; set; }
        public string? EyeRight { get; set; }

        // --- General/Other Biometric Data ---
        public string? BiometricLeft { get; set; }
        public string? BiometricRight { get; set; }

        public int? HasBypass { get; set; }

        // --- Status Fields ---
        public int Hit { get; set; }
        public int Status { get; set; }

        // --- AFIS Fields ---
        public int? AFISHit { get; set; }
        public DateTime? AFISDateProcess { get; set; }
        public int? AFISPersonHit { get; set; }

        // --- Kit Fields ---
        public string? KitUser { get; set; }
        public string? KitName { get; set; }

        public string? GovIDImage { get; set; }

        //public virtual CitizenIDApplication? CitizenIDApplication { get; set; }
    }
}