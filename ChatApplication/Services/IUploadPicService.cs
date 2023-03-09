using ChatApplication.Models;

namespace ChatApplication.Services
{
    public interface IUploadPicService
    {
        public Task<Response> PicUploadAsync(IFormFile file, bool IsProfilePic, string Email);
    }
}
