using System.ComponentModel.DataAnnotations;

namespace SeniorCitizen.Server.DTOs
{
    public class ReassignApplicationDto
    {
        // The ID of the specific enrollment record you want to update
        public string?  ApplicationCode { get; set; }

    }
}
