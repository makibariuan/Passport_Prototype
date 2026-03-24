using Passport_Prototype.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineRegistration.Server.Models
{
    // Ensure all interfaces (IResettableUser, IStatusUser) are correctly defined elsewhere.
    [Table("Users")]
    public class Users : IResettableUser, IStatusUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        // This links the User to their Passport Personal Information (One-to-One)
        // You can now use: _context.Users.Include(u => u.PassportInfo)
        public virtual PassportPersonalInformation? PassportInfo { get; set; }
        public int? PersonID { get; set; }
        public int UserType { get; set; } // 1=System, 2=Kit 
        public int UserRole { get; set; } //1 = Super Admin, 2 = System User, 3 = Kit User
        public string? EmployeeID { get; set; }
        public string? Username { get; set; } // Matches DB NULL-ability
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PasswordHash { get; set; }
        public string? Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool MustResetPassword { get; set; } = true;
        public bool IsActive { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ActiveToken { get; set; }
        public int? OneTimePIN { get; set; }
        public bool? IsEmailConfirmed { get; set; }
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailTokenExpiry { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public string? LoginOtp { get; set; }
        public DateTime? LoginOtpExpiry { get; set; }
        public string? KitEncryptedPgpData { get; set; }
    }
}