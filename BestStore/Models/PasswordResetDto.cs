using System.ComponentModel.DataAnnotations;

namespace BestStore.Models
{
    public class PasswordResetDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "The Confirm Password field is required")]
        [Compare("Password", ErrorMessage = "Passwords did not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
