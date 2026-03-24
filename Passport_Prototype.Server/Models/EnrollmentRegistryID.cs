using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeniorCitizen.Server.Models
{
    [Table("EnrollmentRegistryID", Schema = "dbo")]
    public class EnrollmentRegistryID
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int PersonID { get; set; }
        public string? ApplicationCode { get; set; }
        public int ApplicationType { get; set; } // 1 for Employee, 2 for Citizen

        // --- Personal Info ---
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Email { get; set; }

        // --- Employee Specific ---
        public string? EmployeeID { get; set; }
        public int? DepartmentID { get; set; }
        public string? DepartmentName { get; set; }
        public string? Designation { get; set; }
        public byte? PrintDesignation { get; set; }

        // --- Citizen Specific ---
        public int? CitizenType { get; set; }

        // --- System Metadata ---
        public int? Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? DateSchedule { get; set; }
        public DateTime? DateCapture { get; set; }
        public DateTime? DateUpload { get; set; }
        public DateTime? DateActivate { get; set; }

        public string? KitUser { get; set; }
        public string? KitName { get; set; }

        // --- Identity Verification ---
        public string? GovIDType { get; set; }

        public string? GovIDNumber { get; set; }

        public string? GovIDImage { get; set; } // Base64 or File Path

        public string? GovIDExtension { get; set; }

        // --- Standard Biometrics ---
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

        public bool BiometricBypass { get; set; } = false;

        // --- ISO Standardized Biometrics ---
        public string? ISO_L_Thumb { get; set; }
        public string? ISO_L_Index { get; set; }
        public string? ISO_L_Middle { get; set; }
        public string? ISO_L_Ring { get; set; }
        public string? ISO_L_Small { get; set; }

        public string? ISO_R_Thumb { get; set; }
        public string? ISO_R_Index { get; set; }
        public string? ISO_R_Middle { get; set; }
        public string? ISO_R_Ring { get; set; }
        public string? ISO_R_Small { get; set; }

        // --- AFIS Proprietary Biometrics ---
        public string? AFIS_L_Thumb { get; set; }
        public string? AFIS_L_Index { get; set; }
        public string? AFIS_L_Middle { get; set; }
        public string? AFIS_L_Ring { get; set; }
        public string? AFIS_L_Small { get; set; }

        public string? AFIS_R_Thumb { get; set; }
        public string? AFIS_R_Index { get; set; }
        public string? AFIS_R_Middle { get; set; }
        public string? AFIS_R_Ring { get; set; }
        public string? AFIS_R_Small { get; set; }

        // --- AFIS Results ---
        public int? AFISHit { get; set; }
        public DateTime? AFISDateProcess { get; set; }
        public int? AFISPersonHit { get; set; }

        public int? BiometricStatus { get; set; }
    }
}
