using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace OnlineRegistration.Server.Models
{
    [Table("CitizenIDApplication")]
    public class CitizenIDApplication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? PersonID { get; set; }

        [MaxLength(50)]
        public string? ApplicationCode { get; set; }

        public int? CitizenType { get; set; }

        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Email { get; set; }

        public bool? BiometricBypass { get; set; }

        // --- Government ID Fields ---
        public string? GovIDType { get; set; }
        public string? GovIDNumber { get; set; }
        public string? GovIDImage { get; set; }
        public string? GovIDExtension { get; set; }

        // --- Processing Dates ---
        public DateTime? DateSchedule { get; set; }
        public DateTime? DateCapture { get; set; }
        public DateTime? DateUpload { get; set; }
        public DateTime? DateActivate { get; set; }

        // --- Biometric Assets ---
        public string? Photo { get; set; }
        public string? Signature { get; set; }

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

        // --- Status and AFIS Logic ---
        public int? Status { get; set; }
        public bool? AFISHit { get; set; }
        public DateTime? AFISDateProcess { get; set; }
        public string? AFISPersonHit { get; set; }

        // --- NEW: ICAO Standard Quality Fields (Missing in previous version) ---
        public string? ICAO_LeftThumb { get; set; }
        public string? ICAO_LeftIndex { get; set; }
        public string? ICAO_LeftMiddle { get; set; }
        public string? ICAO_LeftRing { get; set; }
        public string? ICAO_LeftSmall { get; set; }
        public string? ICAO_RightThumb { get; set; }
        public string? ICAO_RightIndex { get; set; }
        public string? ICAO_RightMiddle { get; set; }
        public string? ICAO_RightRing { get; set; }
        public string? ICAO_RightSmall { get; set; }

        // --- NEW: AFIS NIST/Template Fields (Missing in previous version) ---
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
}