using System.ComponentModel.DataAnnotations;

namespace OnlineRegistration.Server.Models
{
    public class EducationLevel
    {
        public int Level_ID { get; set; }     // Matches SQL Level_ID
        public string Code { get; set; }      // Matches SQL Code
        public string Display_Name { get; set; } // Matches SQL Display_Name
    }
}
