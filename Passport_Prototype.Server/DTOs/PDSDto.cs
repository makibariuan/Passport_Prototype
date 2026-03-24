using OnlineRegistration.Server.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineRegistration.Server.DTOs
{


    public class ChildDto
    {
        public int ChildID { get; set; }
        public int PersonID { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

    public class EducationDto
    {
        public string? SchoolName { get; set; } = string.Empty;
        public string? Degree { get; set; } = string.Empty;

        // Use int? if only storing year, DateTime? if full date
        public int? AttendanceFrom { get; set; } = null;
        public int? AttendanceTo { get; set; } = null;

        public string? HighestLevel { get; set; } = string.Empty;
        public int? YearGraduated { get; set; } = null;
        public string? Honors { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;

        public int? OrderIndex { get; set; } // For ordering multiple education records
    }

    public class CivilServiceEligibilityDTO
    {
        public int EligibilityID { get; set; }
        public int PersonID { get; set; }
        public string? CareerService { get; set; }
        public string? Rating { get; set; }
        public DateTime? DateOfExamination { get; set; }
        public string? PlaceOfExamination { get; set; }
        public string? LicenseNumber { get; set; }
        public DateTime? LicenseValidity { get; set; }
        public DateTime? DateReleased { get; set; }
    }

    public class WorkExperienceDTO
    {
        public int WorkExperienceID { get; set; }
        public int PersonID { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string PositionTitle { get; set; } = string.Empty;
        public string DepartmentAgencyCompany { get; set; } = string.Empty;
        public decimal? MonthlySalary { get; set; }
        public string? SalaryGradeStep { get; set; }
        public string StatusOfAppointment { get; set; } = string.Empty;
        public bool IsGovernmentService { get; set; }
        public string? Description { get; set; }
    }

    // Voluntary Work
    public class VoluntaryWorkDTO
    {
        public int WorkId { get; set; }
        public int PersonID { get; set; }
        public string Organization { get; set; } = string.Empty;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? NumberOfHours { get; set; }
        public string? Position { get; set; }
    }

    // Training / Learning & Development
    public class TrainingDTO
    {
        public int TrainingId { get; set; }
        public int PersonID { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal? NumberOfHours { get; set; }
        public string? TypeOfLD { get; set; } // Managerial, Supervisory, Technical
        public string? ConductedBy { get; set; }
    }

    // Skills & Hobbies
    public class SkillHobbyDTO
    {
        public int SkillId { get; set; }
        public int PersonID { get; set; }
        public string SkillOrHobby { get; set; } = string.Empty;
    }

    // Distinctions
    public class DistinctionDTO
    {
        public int DistinctionId { get; set; }
        public int PersonID { get; set; }
        public string Distinction { get; set; } = string.Empty;
    }

    // Memberships
    public class MembershipDTO
    {
        public int MembershipId { get; set; }
        public int PersonID { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
    }

    //pds4
    public class PdsSectionIVDto
    {
        // Primary Key for the C4 table
        public int PdsSectionIVId { get; set; } // Maps to C4ID

        // --- 34. Relatives ---
        public bool Q34a_RelatedWithin3rd { get; set; }
        public bool Q34b_RelatedWithin4th { get; set; }
        public string? Q34_Details { get; set; }

        // --- 35. Administrative/Criminal Charges ---
        public bool Q35a_AdminOffense { get; set; }
        public bool Q35b_CriminallyCharged { get; set; }

        public string? Q35a_AdminDetails { get; set; }
        public string? Q35b_CriminalDetails { get; set; }

        public DateTime? Q35b_DateFiled { get; set; }
        public string? Q35b_Status { get; set; }

        // --- 36. Convicted of Crime ---
        public bool Q36_Convicted { get; set; }
        public string? Q36_Details { get; set; }

        // --- 37. Separation from Service ---
        public bool Q37_Separated { get; set; }
        public string? Q37_Details { get; set; }

        // --- 38. Election History ---
        public bool Q38a_Candidate { get; set; }
        public bool Q38b_ResignedForCampaign { get; set; }

        public string? Q38a_Details { get; set; }
        public string? Q38b_Details { get; set; }

        // --- 39. Country of Origin ---
        public bool Q39_Country { get; set; }
        public string? Q39_Details_Country { get; set; }


        // --- 40. Indigenous Group ---
        public bool Q40a_IndigenousGroup { get; set; }
        public string? Q40a_Details { get; set; }

        public bool Q40b_Disability { get; set; }
        public string? Q40b_Details_IDNo { get; set; }

        public bool Q40c_SoloParent { get; set; }
        public string? Q40c_Details_IDNo { get; set; }
    }

    public class ReferenceDto
    {
        public int ReferenceID { get; set; } // Only needed for update/read operations
        public string? Name { get; set; } = string.Empty;
        public string? OfficeOrResidentialAddress { get; set; }
        public string? ContactNoOrEmail { get; set; }
    }


    public class PdsSectionVDeclarationDto
    {
        public int DeclarationID { get; set; } // Only needed for update/read operations

        // Q. 42 ID Details
        public string? GovernmentIssuedID { get; set; }
        public string? LicensePassportIDNo { get; set; }
        public DateTime? DateOfIssuance { get; set; }
        public string? PlaceOfIssuance { get; set; }
        public DateTime? DateAccomplished { get; set; }
        public string? Signature { get; set; }
        public string? AdminSignature { get; set; }

        // File Keys for documents/images (These will hold the fileKey returned by the 'uploadfile' endpoint)
        public string? PhotoFileKey { get; set; }
        public string? PhotoOriginalName { get; set; }
        public string? RightThumbmarkFileKey { get; set; }
        public DateTime? DateSchedule { get; set; }
    }

    public class PersonalInformationDto
    {
        public int PersonID { get; set; }
        public string? CsIdNo { get; set; }
        public string Surname { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? NameExtension { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        public int? SexID { get; set; }
        public int? CivilStatusID { get; set; }

        public string? BirthCertificateFileKey { get; set; } // Input for the file service
        public string? MarriageCertificateFileKey { get; set; } // Input for the file service

        // ⭐ CRITICAL FIX: Add the JsonPropertyName attribute
        [JsonPropertyName("birthCertificateOriginalName")]
        public string? BirthCertificateOriginalName { get; set; } // This is the C# name

        // ⭐ FIX: Do the same for the Marriage Certificate name
        [JsonPropertyName("marriageCertificateOriginalName")]
        public string? MarriageCertificateOriginalName { get; set; }

        public string? Citizenship { get; set; }
        public decimal? HeightCM { get; set; }
        public decimal? WeightKG { get; set; }
        public string? BloodType { get; set; }
        public string? GsisID { get; set; }
        public string? PagibigID { get; set; }
        public string? PhilhealthID { get; set; }
        public string? SssNo { get; set; }
        public string? Tin { get; set; }
        public string AgencyEmployeeNo { get; set; }

        // 🔹 UPDATED ADDRESS FIELDS (Match these to your DB Entity)
        public string? ResHouseBlockLot { get; set; }
        public string? ResStreet { get; set; }
        public string? ResSubdivisionVillage { get; set; }
        public int? ResBarangay { get; set; }
        public string? ResCityMunicipality { get; set; }
        public string? ResProvince { get; set; }
        public string? ResZip { get; set; }

        public string? PermHouseBlockLot { get; set; }
        public string? PermStreet { get; set; }
        public string? PermSubdivisionVillage { get; set; }
        public int? PermBarangay { get; set; }
        public string? PermCityMunicipality { get; set; }
        public string? PermProvince { get; set; }
        public string? PermZip { get; set; }
        public string? TelephoneNo { get; set; }
        public string? MobileNo { get; set; }
        public string? Email { get; set; }

        public int? DepartmentID { get; set; }
        public string? Designation { get; set; }

        // 🔹 Spouse (optional)
        public string? SpouseSurname { get; set; }
        public string? SpouseFirstName { get; set; }
        public string? SpouseMiddleName { get; set; }
        public string? SpouseNameExtension { get; set; }
        public string? SpouseOccupation { get; set; }
        public string? SpouseEmployer { get; set; }
        public string? SpouseBusinessAddress { get; set; }
        public string? SpouseTelephone { get; set; }

        // 🔹 Father (optional)
        public string? FatherSurname { get; set; }
        public string? FatherFirstName { get; set; }
        public string? FatherMiddleName { get; set; }
        public string? FatherNameExtension { get; set; }

        // 🔹 Mother (optional)
        public string? MotherSurname { get; set; }
        public string? MotherFirstName { get; set; }
        public string? MotherMiddleName { get; set; }
        public string? MotherNameExtension { get; set; }

        public string? GovIDType { get; set; }
        public string? GovIDNumber { get; set; }
        public string? GovIDImage { get; set; } // Base64 Content
        public string? GovIDExtension { get; set; }

        public List<ChildDto> Children { get; set; } = new();
        public List<EducationDto> EducationRecords { get; set; } = new();
        public List<VoluntaryWorkDTO> VoluntaryWorks { get; set; } = new();
        public List<TrainingDTO> Trainings { get; set; } = new();
        public List<SkillHobbyDTO> SkillsHobbies { get; set; } = new();
        public List<DistinctionDTO> Distinctions { get; set; } = new();
        public List<MembershipDTO> Memberships { get; set; } = new();
        public List<CivilServiceEligibilityDTO> CivilServiceEligibilities { get; set; } = new();
        public List<WorkExperienceDTO> WorkExperiences { get; set; } = new();
        public PdsSectionIVDto? PdsSectionIV { get; set; }
        public List<ReferenceDto> References { get; set; } = new();

        public PdsSectionVDeclarationDto? Declaration { get; set; }


    }


}
