using System.ComponentModel.DataAnnotations;

namespace BestStore.Models
{
    public class PasswordDto
    {
        [Required(ErrorMessage = "The Current Password field is required"), MaxLength(100)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "The New Password field is required"), MaxLength(100)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Passsword field is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
