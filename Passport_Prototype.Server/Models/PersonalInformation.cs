using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineRegistration.Server.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    [Table("PersonalInformation")]
    public class PersonalInformation
    {
        [Key]
        // We set DatabaseGeneratedOption.None because we manually populate 
        // this ID using the User.Id in the Register method.
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PersonID { get; set; }

        public string? CsIdNo { get; set; }
        public string? Surname { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? NameExtension { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        public int? SexID { get; set; }
        public int? CivilStatusID { get; set; }

        public string? BirthCertificatePath { get; set; } // New database column mapping
        public string? BirthCertificateOriginalName { get; set; }

        public string? MarriageCertificatePath { get; set; } // New database column mapping
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
        public string? AgencyEmployeeNo { get; set; }
        public string? ResHouseBlockLot { get; set; }
        public string? ResStreet { get; set; }
        public string? ResSubdivisionVillage { get; set; }
        public int? ResBarangay { get; set; } // Matches the int FK in your DB
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
        public string? Designation { get; set; } = string.Empty;

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

        //gov id
        public string? GovIDType { get; set; }
        public string? GovIDNumber { get; set; }
        public string? GovIDImage { get; set; } // Base64 Content
        public string? GovIDExtension { get; set; }

        // 🔹 Navigation properties (Children / Education)
        public ICollection<Child> Children { get; set; } = new List<Child>();
        public ICollection<Education> EducationRecords { get; set; } = new List<Education>();
        public ICollection<CivilServiceEligibility> CivilServiceEligibilities { get; set; } = new List<CivilServiceEligibility>();
        public ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
        public ICollection<VoluntaryWork> VoluntaryWorks { get; set; } = new List<VoluntaryWork>();
        public ICollection<Training> Trainings { get; set; } = new List<Training>();
        public ICollection<SkillHobby> SkillsHobbies { get; set; } = new List<SkillHobby>();
        public ICollection<Distinction> Distinctions { get; set; } = new List<Distinction>();
        public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
        public PdsSectionIV? PdsSectionIV { get; set; }
        public ICollection<PersonalInformationReference> PersonalInformationReferences { get; set; } = new List<PersonalInformationReference>();
        public PdsSectionVDeclaration? Declaration { get; set; }
    }

    [Table("PersonalInformation_CivilServiceEligibility")]
    public class CivilServiceEligibility
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EligibilityID { get; set; }
        public int PersonID { get; set; }
        public string? CareerService { get; set; }
        public string? Rating { get; set; }
        public DateTime? DateOfExamination { get; set; }
        public string? PlaceOfExamination { get; set; }
        public string? LicenseNumber { get; set; }
        public DateTime? LicenseValidity { get; set; }
        public DateTime? DateReleased { get; set; }

        // Navigation
        [JsonIgnore] // 👈 prevents cycles
        public PersonalInformation Person { get; set; }
    }

    [Table("PersonalInformation_WorkExperience")]
    public class WorkExperience
    {
        [Key]
        public int WorkExperienceId { get; set; }

        [ForeignKey("PersonalInformation")]
        public int PersonID { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        [Required, MaxLength(150)]
        public string PositionTitle { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string DepartmentAgencyCompany { get; set; } = string.Empty;

        public decimal? MonthlySalary { get; set; }

        [MaxLength(20)]
        public string? SalaryGradeStep { get; set; }

        [Required, MaxLength(100)]
        public string StatusOfAppointment { get; set; } = string.Empty;

        public bool IsGovernmentService { get; set; }

        public string? Description { get; set; }

        // Navigation property
        [JsonIgnore] // prevents reference loops
        public PersonalInformation Person { get; set; }
    }


    [Table("PersonalInformation_Children")]
    public class Child
    {
        public int ChildId { get; set; }
        public int PersonID { get; set; }

        [Column("Child_Name")]
        public string FullName { get; set; } = string.Empty;

        [Column("Child_DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        // Navigation
        [JsonIgnore] // 👈 prevents cycles
        public PersonalInformation? Person { get; set; }
    }

    [Table("PersonalInformation_Education")]
    public class Education
    {
        public int EducationId { get; set; }
        public int PersonID { get; set; }
        public string Level { get; set; } = string.Empty;

        public string? SchoolName { get; set; } = string.Empty;

        public int? OrderIndex { get; set; }

        [Column("DegreeCourse")]
        public string? Degree { get; set; }


        public int? AttendanceFrom { get; set; }
        public int? AttendanceTo { get; set; }
        public string? HighestLevel { get; set; }
        public int? YearGraduated { get; set; }
        public string? Honors { get; set; }

        // Navigation
        [JsonIgnore] // 👈 prevents cycles
        public PersonalInformation? Person { get; set; }
    }

    [Table("PersonalInformation_VoluntaryWork")]
    public class VoluntaryWork
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorkId { get; set; }
        public int PersonID { get; set; }
        public string Organization { get; set; } = string.Empty;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? NumberOfHours { get; set; }
        public string? Position { get; set; }

        // Navigation
        [JsonIgnore]
        public PersonalInformation? Person { get; set; }
    }

    [Table("PersonalInformation_Training")]
    public class Training
    {
        public int TrainingId { get; set; }
        public int PersonID { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal? NumberOfHours { get; set; }
        public string? TypeOfLD { get; set; }
        public string? ConductedBy { get; set; }

        // Navigation
        [JsonIgnore]
        public PersonalInformation? Person { get; set; }
    }

    [Table("PersonalInformation_SkillsHobbies")]
    public class SkillHobby
    {
        // 🔑 Add this Primary Key property
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SkillId { get; set; }
        public int PersonID { get; set; }
        public string SkillOrHobby { get; set; } = string.Empty;

        [JsonIgnore]
        public PersonalInformation? Person { get; set; }
    }

    [Table("PersonalInformation_Distinctions")]
    public class Distinction
    {
        public int DistinctionId { get; set; }
        public int PersonID { get; set; }
        public string DistinctionName { get; set; } = string.Empty;

        [JsonIgnore]
        public PersonalInformation? Person { get; set; }
    }

    [Table("PersonalInformation_Memberships")]
    public class Membership
    {
        public int MembershipId { get; set; }
        public int PersonID { get; set; }
        public string OrganizationName { get; set; } = string.Empty;

        [JsonIgnore]
        public PersonalInformation? Person { get; set; }
    }

    [Table("EmployeeIDApplication")]
    public class EmployeeIDApplication
    {
        [Key]
        public int PersonID { get; set; }
        public string? Surname { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? EmployeeID { get; set; }
        public int? DepartmentID { get; set; }
        public string? DepartmentName { get; set; }
        public string? Designation { get; set; }
        public byte? PrintDesignation { get; set; }
        public string? ApplicationCode { get; set; }
        public DateTime? DateSchedule { get; set; }
        public DateTime? DateCapture { get; set; }
        public DateTime? DateUpload { get; set; }
        //public DateTime? DateActivate { get; set; }

        // --- Added for Government ID Upload ---
        public string? GovIDType { get; set; }      // e.g., "Passport", "Driver's License"
        public string? GovIDNumber { get; set; }    // The actual ID number
        public string? GovIDImage { get; set; }     // Base64 string of the ID
        public string? GovIDExtension { get; set; } // e.g., ".jpg", ".png"

        // --- Added for Biometric ---
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
        public int? Status { get; set; }

        public int? AFISHit { get; set; }
        public DateTime? AFISDateProcess { get; set; }
        public int? AFISPersonHit { get; set; }
    }


    [Table("PersonalInformation_C4")]
    public class PdsSectionIV
    {
        [Key]
        [Column("C4ID")]
        public int PdsSectionIVId { get; set; }

        [Column("PersonID")]
        public int PersonID { get; set; }

        // --- Q34 (Relatives) ---
        [Column("IsRelatedThirdDegree")]
        public bool Q34a_RelatedWithin3rd { get; set; }

        [Column("IsRelatedFourthDegree")]
        public bool Q34b_RelatedWithin4th { get; set; }

        [Column("RelatedDetails")]
        public string? Q34_Details { get; set; }

        // --- Q35 (Admin/Criminal Charges) ---
        [Column("FoundGuiltyAdminOffense")]
        public bool Q35a_AdminOffense { get; set; }

        [Column("ChargedCriminally")]
        public bool Q35b_CriminallyCharged { get; set; }

        [Column("AdminOffenseDetails")]
        public string? Q35a_Details { get; set; }

        [Column("CriminalChargeDetails")]
        public string? Q35b_Details { get; set; }

        [Column("DateFiled")]
        public DateTime? Q35b_DateFiled { get; set; }

        [Column("CaseStatus")]
        public string? Q35b_Status { get; set; }

        // --- Q36 (Convicted of Crime) ---
        [Column("ConvictedOfCrime")]
        public bool Q36_Convicted { get; set; }

        [Column("ConvictionDetails")]
        public string? Q36_Details { get; set; }

        // 🛑 REMOVED: HasPendingCase, PendingCaseDetails, ConvictedFinalJudgment, ConvictedFinalDetails
        // These 4 fields have been successfully deleted from the model.

        // --- Q37 (Separation) ---
        [Column("SeparatedFromService")]
        public bool Q37_Separated { get; set; }

        [Column("SeparationDetails")]
        public string? Q37_Details { get; set; }

        // --- Q38 (Election) ---
        [Column("IsCandidateElection")]
        public bool Q38a_Candidate { get; set; }

        [Column("ResignedForCampaign")]
        public bool Q38b_ResignedForCampaign { get; set; }

        [Column("ElectionDetails")] // Q38a Details
        public string? Q38a_Details { get; set; }

        [Column("ResignedCampaignDetails")] // Q38b Details
        public string? Q38b_Details { get; set; }

        // Q39 remaining fields (Type corrections applied)
        [Column("Country")] //
        public bool Q39_Country { get; set; }

        [Column("CountryDetails")]
        public string? Q39_Details_Country { get; set; }

        // --- Q40 (Indigenous, Disability, Solo Parent) ---
        [Column("IsIndigenousPerson")]
        public bool Q40a_IndigenousGroup { get; set; }

        [Column("IndigenousDetails")]
        public string? Q40a_Details { get; set; }

        [Column("HasDisability")]
        public bool Q40b_Disability { get; set; }

        [Column("DisabilityDetails")]
        public string? Q40b_Details_IDNo { get; set; }

        [Column("IsSoloParent")]
        public bool Q40c_SoloParent { get; set; }

        [Column("SoloParentIDNumber")]
        public string? Q40c_Details_IDNo { get; set; }

        // Navigation Property to main PDS
        [JsonIgnore]
        public PersonalInformation? Person { get; set; }
    }

    [Table("PersonalInformation_References")]
    public class PersonalInformationReference
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReferenceID { get; set; }

        // Foreign Key
        public int PersonID { get; set; }

        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? OfficeOrResidentialAddress { get; set; }

        [StringLength(100)]
        public string? ContactNoOrEmail { get; set; }

        [JsonIgnore]
        public PersonalInformation? Person { get; set; } = null!;
    }

    [Table("PdsSectionV_Declaration")] // Maps to the declaration table
    public class PdsSectionVDeclaration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeclarationID { get; set; }

        // Foreign Key (1-to-1 relationship)
        public int PersonID { get; set; }

        // --- Q. 42 ID Details ---
        [StringLength(100)]
        public string? GovernmentIssuedID { get; set; }

        [StringLength(100)]
        public string? LicensePassportIDNo { get; set; }

        public DateTime? DateOfIssuance { get; set; }

        [StringLength(100)]
        public string? PlaceOfIssuance { get; set; }

        public DateTime? DateAccomplished { get; set; }

        [StringLength(255)]
        public string? SignaturePath { get; set; }

        [StringLength(255)]
        public string? AdminSignature { get; set; }

        // --- Q. 43 Photo ---
        [StringLength(255)]
        public string? PhotoPath { get; set; }

        [StringLength(255)]
        public string? PhotoOriginalName { get; set; }

        // Note: Using 'RightThumbmarkPath' to map to the PDS thumbmark box
        [StringLength(255)]
        public string? RightThumbmarkPath { get; set; }

        public DateTime? DateSchedule { get; set; }

        // Navigation Property back to the parent PersonalInformation record
        [JsonIgnore]
        [ForeignKey("PersonID")]
        public PersonalInformation Person { get; set; } = null!;
    }
}
