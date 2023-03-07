using ChatApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatApplication.Services
{
    public interface IAuthService
    {
        public Task<Response> CreateUser(InputUser inpUser);
        public Response Login(UserDTO request);
        public Task<Response> Verify(VerificationModel v);
        public Task<Response> ForgetPassword(string email);
        public Task<Response> ChangePassword(ChangePassModel r,string email);
        public Task<Response> ResetPassword(ResetpassModel r,string email);

    }
}
