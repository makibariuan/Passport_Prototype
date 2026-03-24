using System.ComponentModel.DataAnnotations;

namespace OnlineRegistration.Server.DTOs
{
    public class BypassLogDto
    {
        /// reason (e.g., "NO_HANDS", "BLIND").


        [MaxLength(100)]
        public string StepName { get; set; }
        public string ReasonCode { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ReasonDetails { get; set; } = string.Empty;

        public string Image { get; set; } = string.Empty;
        public string DateBypassed { get; set; }
        public string KitName { get; set; } = string.Empty;
        public string KitUser { get; set; } = string.Empty;
    }
}
