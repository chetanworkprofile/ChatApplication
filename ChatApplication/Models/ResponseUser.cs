
namespace ChatApplication.Models
{
    public class ResponseUser
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long Phone { get; set; }
        //public byte[] PasswordHash { get; set; } = new byte[32];
        public DateTime DateOfBirth { get; set; }
        public string PathToProfilePic { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
