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
        public string Email { get; set; } = "email@chatapp.com";
        public long Phone { get; set; } = 9999999999;
        [Required]
        public string Password { get; set; } = "fgh@98gh!#cf$5";
        public DateTime DateOfBirth { get; set; }
        //[Url]
        //public string? PathToProfilePic { get; set; }
    }
}
