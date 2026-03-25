namespace Passport_Prototype.Server.DTOs
{
    public class UnifiedProfileDTO
    {
        public CreatePassportPersonalInformationDTO Personal { get; set; } = null!;  // Personal info
        public List<FamilyMemberDTO> Family { get; set; } = new();               // Family members
        public ContactInformationDto Contact { get; set; } = null!;                   // Contact info
        public WorkInformationDto Work { get; set; } = null!;                         // Work info
    }
}
