namespace SeniorCitizen.Server.DTOs
{
    public class SetQuotaRequestDto
    {
        public string SiteName { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string ScheduledTime { get; set; } // Received as string (e.g., "08:00")
        public int MaxSlots { get; set; }
    }
}
