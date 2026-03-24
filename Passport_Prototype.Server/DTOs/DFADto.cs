namespace SeniorCitizen.Server.DTOs
{
    public class DFADto
    {
        public class PhotoSignatureDto
        {
            public string? ApplicationCode { get; set; }
            public string? Photo { get; set; }     // Base64 string or File URL
            public string? Signature { get; set; } // Base64 string or File URL
        }

    }
}
