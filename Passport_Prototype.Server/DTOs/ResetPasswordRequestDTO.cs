using System.ComponentModel.DataAnnotations;


// DTO for resetting a user's password  
public class ResetUserPasswordRequest
{
    [Required]
    public string Username { get; set; }
}

public class FlexiblePasswordRequest
{
    public string Email { get; set; }
    public string NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}