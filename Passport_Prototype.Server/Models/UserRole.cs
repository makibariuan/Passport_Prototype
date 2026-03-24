using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace OnlineRegistration.Server.Models
{
    [Table("UserRole")] // Matches your SQL table name
    public class UserRole
    {
        [Key]
        [Column("RoleID")] // Maps to the SQL column [RoleID]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("RoleDesc")] // Maps to the SQL column [RoleDesc]
        public string Description { get; set; } = string.Empty;

        // Navigation property back to Users
        public ICollection<Users> Users { get; set; } = new List<Users>();
    }
}