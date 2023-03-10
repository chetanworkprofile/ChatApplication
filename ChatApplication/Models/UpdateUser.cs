using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace ChatApplication.Models
{
    public class UpdateUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long Phone { get; set; } = -1;
        //public byte[] PasswordHash { get; set; } = new byte[32];
        public DateTime DateOfBirth { get; set; } = DateTime.MinValue;
        public string? PathToProfilePic { get; set; } = null;

    }
}
