using System.ComponentModel.DataAnnotations;

namespace ChatApplication.Models
{
    public class ResetPassModel
    {
        [Required]
        public string Email { get; set; } = "abc@abc.com";

        [Required, MinLength(8, ErrorMessage = "Please enter at least 8 characters")]
        public string Password { get; set; } = string.Empty;
        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
