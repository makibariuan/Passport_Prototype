using SeniorCitizen.Server.Models;

namespace SeniorCitizen.Server.Services
{
    public class MenuProvider
    {
        public static List<MenuItem> GetFullMenu()
        {
            return new List<MenuItem>
        {
            new MenuItem { 
                Title = "Dashboard", 
                Url = "/dashboard", 
                Icon = "home", 
                AllowedRoles = new List<int> { 1, 2, 3, 4, 5, 6 } },
            


            // HR & Super Admin Only (Role 6 & 1)
            new MenuItem {
                Title = "Adjudication",
                Url = "/hr/adjudication",
                Icon = "gavel",
                AllowedRoles = new List<int> { 1, 6 }
            },

            // Kit Operator & System User (Role 3 & 2)
            //new MenuItem {
            //    Title = "Biometric Capture",
            //    Url = "/capture",
            //    Icon = "fingerprint",
            //    AllowedRoles = new List<int> { 2, 3 }
            //},

            // Citizen Only (Role 5)
            new MenuItem {
                Title = "My Application",
                Url = "/citizen/status",
                Icon = "person",
                AllowedRoles = new List<int> { 5 }
            }
        };
       }
    }
}
