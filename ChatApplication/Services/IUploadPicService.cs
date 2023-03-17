using ChatApplication.Models;

namespace ChatApplication.Services
{
    public interface IUploadPicService
    {
        public Task<object> FileUploadAsync(IFormFile file, string Email,string token,int type);
        public Task<object> ProfilePicUploadAsync(IFormFile file, string Email, string token);
    }
}
