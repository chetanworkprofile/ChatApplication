using System.ComponentModel.DataAnnotations;

namespace ChatApplication.Models
{
    public class InputUser
    {
        [Required]
        public string FirstName { get; set; } = "Firstname";
        [Required]
        public string LastName { get; set; } = "Lastname";
        [EmailAddress]
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = "email@chatapp.com";
        public long Phone { get; set; } = 9999999999;
        [Required(ErrorMessage = "Password is required")]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "fgh@98gh!#cf$5";
        public DateTime DateOfBirth { get; set; }
        //[Url]
        //public string? PathToProfilePic { get; set; }
    }
}
