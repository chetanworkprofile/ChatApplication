using ChatApplication.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ChatApplication.Services
{
    public interface IAuthService
    {
        public Task<object> CreateUser(InputUser inpUser);
        public Object Login(UserDTO request);
        public Task<Object> ForgetPassword(string email);
        public Task<Object> Verify(ResetpassModel r, string email);
        public Task<object> ChangePassword(ChangePassModel r,string email,string token);
        public Object GoogleHelper(GoogleJsonWebSignature.Payload user);
        public bool VerifyPasswordHash(string password, byte[] passwordHash);
        public string CreateToken(TokenUser user);
        public Task<object> Logout(string email, string token);

    }
}
