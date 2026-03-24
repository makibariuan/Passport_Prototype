using System.ComponentModel.DataAnnotations;

namespace OnlineRegistration.Server.Models
{
    public class LoginRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class KitLoginRequest
    {
        [Required]
        public string username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Message { get; set; }
        public string Token { get; set; }
        //public string UserRole { get; set; }
        //public bool MustResetPassword { get; set; }
    }
}