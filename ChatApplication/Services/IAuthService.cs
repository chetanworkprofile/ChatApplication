using ChatApplication.Models;

namespace ChatApplication.Services
{
    public interface IAuthService
    {
        public Task<Response> CreateUser(InputUser inpUser);
        public Response Login(UserDTO request);

    }
}
