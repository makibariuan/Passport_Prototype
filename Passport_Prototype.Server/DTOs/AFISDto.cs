namespace OnlineRegistration.Server.DTOs
{
    public class PersonWithHitDto
    {
        public int PersonA { get; set; }        // The ID of the person being checked
        public int PersonB { get; set; }        // The ID of the person they matched with
        public float Score { get; set; }
        public DateTime DateDetected { get; set; }
    }

    public class PersonValidatedDto
    {
        public int PersonId { get; set; }
        public DateTime ValidatedAt { get; set; }
        public bool HasHit { get; set; }
    }
}
