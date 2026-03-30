using System.ComponentModel.DataAnnotations;

namespace Passport_Prototype.Server.Models
{
    public class Application
    {
        [Key]
        public int ApplicationId { get; set; }

        public string ApplicationCode { get; set; }

        public int PassportPersonalInformationId { get; set; }

        public int? UserId { get; set; }

        public string? Region { get; set; }

        public string? Country { get; set; }

        public string? Site { get; set; }

        public DateTime? Schedule {  get; set; }

        public string? ApplicationType { get; set; }

        public string? CitizenshipBasis { get; set; }

        public bool? isForeignPassportHolder { get; set; }

        public bool? isCourtesyLane { get; set; }

        public string? DocumentType { get; set; }

        public string? IdDocumentIdNumber { get; set; }

        public string? IdDocumentType { get; set; }

        public string? ValidIdPath { get; set; }

        public string? CertificatePath { get; set; }

        public string? ProcessingType { get; set; }

        public string? PaymentMethod { get; set; }

        public string? DeliveryOption { get; set; }

        public bool? isPaid { get; set; }

        [Required]
        public int? ApplicationStatus { get; set; }
        public string? ApplicationBarCodePath { get; set; }
    }
}
