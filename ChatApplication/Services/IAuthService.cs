using ChatApplication.Models;

namespace ChatApplication.Services
{
    public interface IAuthService
    {
        public Task<Response> CreateUser(InputUser inpUser);
        public Response Login(UserDTO request);
        public Task<Response> Verify(VerificationModel v);
        public Task<Response> ForgetPassword(ForgetPassModel f);
        public Task<Response> ResetPassword(ResetPassModel r);

    }
}
