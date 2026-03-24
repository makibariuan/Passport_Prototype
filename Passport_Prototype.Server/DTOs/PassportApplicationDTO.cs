using System.ComponentModel.DataAnnotations;

namespace Passport_Prototype.Server.DTOs
{
    public class CreateApplicationDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Region { get; set; } = null!;

        [Required]
        public string Country { get; set; } = null!;

        [Required]
        public string Site { get; set; } = null!;

        [Required]
        public DateTime Schedule { get; set; }

        [Required]
        public string ApplicationType { get; set; } = null!;

        [Required]
        public string CitizenshipBasis { get; set; } = null!;

        [Required]
        public bool isForeignPassportHolder { get; set; }

        [Required]
        public bool isCourtesyLane { get; set; }

        [Required]
        public string DocumentType { get; set; } = null!;

        [Required]
        public string IdDocumentIdNumber { get; set; } = null!;

        [Required]
        public string IdDocumentType { get; set; } = null!;

        [Required]
        public IFormFile ValidId { get; set; } = null!;

        [Required]
        public IFormFile Certificate { get; set; } = null!;

        [Required]
        public string ProcessingType { get; set; } = null!;

        [Required]
        public string PaymentMethod { get; set; } = null!;

        [Required]
        public string DeliveryOption { get; set; } = null!;

        [Required]
        public bool isPaid { get; set; }

        [Required]
        public string ApplicationStatus { get; set; } = null!;
    }


}
