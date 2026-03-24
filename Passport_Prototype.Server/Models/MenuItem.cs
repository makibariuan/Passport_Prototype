namespace SeniorCitizen.Server.Models
{
    public class MenuItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public List<int> AllowedRoles { get; set; }
        public List<MenuItem> Children { get; set; } = new List<MenuItem>();
    }
}
