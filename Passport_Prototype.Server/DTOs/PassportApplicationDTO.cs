using System.ComponentModel.DataAnnotations;

namespace Passport_Prototype.Server.DTOs
{
    public class CreateApplicationDTO
    {
        [Required]
        public int? UserId { get; set; }

        [Required]
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

        public bool isPaid { get; set; }

        public string ApplicationStatus { get; set; } = null!;
    }


}
