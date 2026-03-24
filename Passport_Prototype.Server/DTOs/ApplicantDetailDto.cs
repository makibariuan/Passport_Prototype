namespace SeniorCitizen.Server.DTOs
{
    public class ApplicantDetailDto
    {
        public int PersonId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // --- Added for Government ID Display ---
        public string? GovIDType { get; set; }
        public string? GovIDNumber { get; set; }
        public string? GovIDImage { get; set; }     // Base64 string for the image
        public string? GovIDExtension { get; set; } // e.g., ".jpg" or ".png"
    }


    public class ApplicantReadDto
    {
        public int CitizenID { get; set; } // Matches "Id AS CitizenID" in SP

        public string? ApplicationCode { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public int? LatestBiometricStatus { get; set; }

        public string LatestKitName { get; set; } = string.Empty;

        // --- Added fields from the updated Stored Procedure ---

        public string? EmployeeID { get; set; }

        public int ApplicationType { get; set; } // 1 for Employee, 2 for Citizen
    }


    //public class CompleteApplicantDetailDto
    //{
    //    public int PersonId { get; set; }
    //    public string ApplicationCode { get; set; } = string.Empty;

    //    public string FirstName { get; set; } = string.Empty;
    //    public string LastName { get; set; } = string.Empty;

    //}

    public class SearchApplicantDetailDto
    {
        public string ApplicationCode { get; set; } = string.Empty;
        public string KitName { get; set; } = string.Empty;
    }
}
