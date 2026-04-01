using System.ComponentModel.DataAnnotations;

namespace Passport_Prototype.Server.DTOs
{
    public class CreateApplicationDTO
    {

        public int? PassportPersonalInformationId { get; set; }

        public string Region { get; set; } = null!;

        public string Country { get; set; } = null!;

        public string Site { get; set; } = null!;

        public DateTime Schedule { get; set; }

        public string ApplicationType { get; set; } = null!;

        public string CitizenshipBasis { get; set; } = null!;

        public bool isForeignPassportHolder { get; set; }

        public bool isCourtesyLane { get; set; }

        public string DocumentType { get; set; } = null!;

        public string IdDocumentIdNumber { get; set; } = null!;

        public string IdDocumentType { get; set; } = null!;

        public IFormFile ValidId { get; set; } = null!;

        public IFormFile Certificate { get; set; } = null!;

        public string ProcessingType { get; set; } = null!;

        public string PaymentMethod { get; set; } = null!;

        public string DeliveryOption { get; set; } = null!;

        public int Amount { get; set; } = 0;

        public bool isPaid { get; set; }

        public int ApplicationStatus { get; set; } = 1;
    }

    public class ApplicationWithUserInfoDto
    {
        public string? ApplicationCode { get; set; }

        public int? PassportPersonalInformationId { get; set; }
        public string? PassportFirstName { get; set; }
        public string? PassportMiddleName { get; set; }
        public string? PassportLastName { get; set; }
        public string? BirthCity { get; set; }
        public string? BirthBarangay { get; set; }
        public string? BirthLegitimacy { get; set; }
        public string? Relationship { get; set; }
        public bool? IsAdoptee { get; set; }

        public int Id { get; set; }
        public int? PersonID { get; set; }
        public int? ApplicationType { get; set; }
        public string? EnrollmentFirstName { get; set; }
        public string? EnrollmentMiddleName { get; set; }
        public string? EnrollmentLastName { get; set; }
        public int? AFISPersonHit { get; set; }
        public int? BiometricStatus { get; set; }
        
        public int? ApplicationId { get; set; }
        public int? UserId { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public int? ApplicationStatus { get; set; }
        public string? ApplicationBarCodePath { get; set; }
        public string? ValidIdPath { get; set; }
        public string? CertificatePath { get; set; }

        public string? AppType { get; set; }
        public string? ApplicationCountry { get; set; }
        public string? ApplicationSite { get; set; }
        public DateTime? ApplicationSchedule { get; set; }
        public bool? ApplicationCourtesy { get; set; }
        public bool? ApplicationPaid { get; set; }
        public string? ApplicationCitizenshipBasis { get; set; }

        public int? Amount { get; set; }

    }

    //public class ApplicationWithUserInfoDto
    //{
    //    // Enrollment
    //    public int EnrollmentId { get; set; }
    //    public string? ApplicationCode { get; set; }
    //    public int? PersonID { get; set; }
    //    public string? FirstName { get; set; }
    //    public string? MiddleName { get; set; }
    //    public string? LastName { get; set; }
    //    public DateTime? BirthDate { get; set; }
    //    public string? Email { get; set; }
    //    public string? EmployeeID { get; set; }
    //    public string? DepartmentName { get; set; }
    //    public string? Designation { get; set; }
    //    public string? CitizenType { get; set; }
    //    public int? EnrollmentStatus { get; set; }
    //    public DateTime? EnrollmentCreatedAt { get; set; }
    //    public DateTime? DateSchedule { get; set; }

    //    public string? Photo { get; set; }
    //    public string? Signature { get; set; }

    //    // Application
    //    public int ApplicationId { get; set; }
    //    public int UserId { get; set; }
    //    public string? Region { get; set; }
    //    public string? Country { get; set; }
    //    public string? Site { get; set; }
    //    public DateTime? Schedule { get; set; }
    //    public string? AppType { get; set; }
    //    public string? CitizenshipBasis { get; set; }
    //    public bool? isForeignPassportHolder { get; set; }
    //    public bool? isCourtesyLane { get; set; }
    //    public string? DocumentType { get; set; }
    //    public string? IdDocumentIdNumber { get; set; }
    //    public string? ValidIdPath { get; set; }
    //    public string? CertificatePath { get; set; }
    //    public string? ProcessingType { get; set; }
    //    public string? ApplicationBarCodePath { get; set; }
    //    public string? PaymentMethod { get; set; }
    //    public string? DeliveryOption { get; set; }
    //    public bool? isPaid { get; set; }
    //    public int? ApplicationStatus { get; set; }

    //    // Passport
    //    public string? PassportFirstName { get; set; }
    //    public string? PassportMiddleName { get; set; }
    //    public string? PassportLastName { get; set; }
    //    public string? BirthCity { get; set; }
    //    public string? BirthBarangay { get; set; }
    //    public string? BirthLegitimacy { get; set; }
    //    public string? Relationship { get; set; }
    //    public bool? IsAdoptee { get; set; }
    //}
}
