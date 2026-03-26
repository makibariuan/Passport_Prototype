using System.ComponentModel.DataAnnotations;

namespace Passport_Prototype.Server.Models
{
    public class Application
    {
        [Key]
        public int ApplicationId { get; set; }

        public string ApplicationCode { get; set; }

        public int PassportPersonalInformationId { get; set; }

        [Required]
        public int? UserId { get; set; }

        [Required]
        public string? Region { get; set; }

        [Required]
        public string? Country { get; set; }

        [Required]
        public string? Site { get; set; }

        [Required]
        public DateTime? Schedule {  get; set; }

        [Required]
        public string? ApplicationType { get; set; }

        [Required]
        public string? CitizenshipBasis { get; set; }

        [Required]
        public bool? isForeignPassportHolder { get; set; }

        [Required]
        public bool? isCourtesyLane { get; set; }

        [Required]
        public string? DocumentType { get; set; }

        [Required]
        public string? IdDocumentIdNumber { get; set; }

        [Required]
        public string? IdDocumentType { get; set; }

        [Required]
        public string? ValidIdPath { get; set; }

        [Required]
        public string? CertificatePath { get; set; }

        [Required]
        public string? ProcessingType { get; set; }

        [Required]
        public string? PaymentMethod { get; set; }

        [Required]
        public string? DeliveryOption { get; set; }

        [Required]
        public bool? isPaid { get; set; }

        [Required]
        public int? ApplicationStatus { get; set; }
    }
}
