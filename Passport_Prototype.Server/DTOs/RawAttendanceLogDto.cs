namespace SeniorCitizen.Server.DTOs
{
    public class RawAttendanceLogDto
    {
        public string EmployeeID { get; set; }
        public DateTime Timestamp { get; set; } // Changed to DateTime
        public string DeviceID { get; set; }
        public int LogType { get; set; }        // Changed to int
        public string StatusFlag { get; set; }
        public string AdditionalFlag { get; set; }
    }
}
