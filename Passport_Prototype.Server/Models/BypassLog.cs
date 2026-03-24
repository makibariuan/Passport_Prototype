using OnlineRegistration.Server.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("BypassLogs")]
public class BypassLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int BDEID { get; set; }

    // --- FIX: Add '?' to allow NULL values from the DB ---
    public string? KitName { get; set; } 
    public string? KitUser { get; set; } 
    public string? StepName { get; set; } 
    public string? ReasonCode { get; set; } 
    public string? ReasonDetails { get; set; }
    public string? Image { get; set; }
    public DateTime DateBypassed { get; set; }
}