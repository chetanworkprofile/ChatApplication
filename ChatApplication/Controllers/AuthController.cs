using Microsoft.AspNetCore.Mvc;
using ChatApplication.Models;
using ChatApplication.Services;
using ChatApplication.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ChatApplication;
using Google.Apis.Auth;

namespace ChatApplication.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        IAuthService authService;
        Response response = new Response();
        ResponseWithoutData response2 = new ResponseWithoutData();
        object result = new object();
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration,ChatAppDbContext dbContext, ILogger<AuthController> logger)
        {
            authService = new AuthService(configuration,dbContext);
            _logger = logger;
        }

        [HttpPost]
        [Route("/api/v1/user/register")]
        public IActionResult RegisterUser([FromBody] InputUser inpUser)
        {
            if (!ModelState.IsValid)
            {
                response2.StatusCode = 400;
                response2.Message = "Invalid Input/One or more fields are invalid";
                response2.Success = false;
                return BadRequest(response2);
            }
            try
            {
                _logger.LogInformation("Register User method started");
                result = authService.CreateUser(inpUser).Result;
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error something wrong happened ", DateTime.Now);
                //ResponseWithoutData response = new ResponseWithoutData();
                response2.StatusCode = 500;
                response2.Message = ex.Message;
                response2.Success = false;
                return StatusCode(500, response2);
            }
        }

        [HttpPost("/api/v1/user/login")]
        public ActionResult<User> UserLogin(UserDTO request)
        {
            _logger.LogInformation("User Login attempt");
            try
            {
                result = authService.Login(request);

               /* response2 = (ResponseWithoutData)result;

                if (response.StatusCode == 404)
                {
                    return BadRequest(response2);
                }
                else if (response.StatusCode == 403)
                {
                    return BadRequest(response2);
                }*/
                return Ok(result);
            }
            catch (Exception ex)
            {
                response2.StatusCode=500;
                response2.Message = ex.Message;
                response2.Success = false;
                return StatusCode(500, response2);
            }
        }

        [HttpPost]
        [Route("/api/v1/forgetPassword")]
        public ActionResult<User> ForgetPassword(string Email)
        {
            _logger.LogInformation("forget password attempt");
            try
            {
                result = authService.ForgetPassword(Email).Result;

                /*response2 = (ResponseWithoutData)result;

                if (response2.StatusCode == 404)
                {
                    return BadRequest(response2);
                }
                else if (response2.StatusCode == 403)
                {
                    return BadRequest(response2);
                }*/
                return Ok(result);
            }
            catch (Exception ex)
            {
                response2.StatusCode = 500;
                response2.Message = ex.Message;
                response2.Success = false;
                return StatusCode(500, response2);
            }
        }

        [HttpPost, Authorize(Roles = "resetpassword")]
        [Route("/api/v1/resetPassword")]
        public ActionResult<User> Verify(ResetpassModel r)
        {
            _logger.LogInformation("verification attempt");
            try
            {
                string? email = User.FindFirstValue(ClaimTypes.Email);
                result = authService.Verify(r,email).Result;

                /*response2 = (ResponseWithoutData)result;

                if (response2.StatusCode == 404)
                {
                    return NotFound(response2);
                }
                else if (response2.StatusCode == 400)
                {
                    return BadRequest(response2);
                }*/
                return Ok(result);
            }
            catch (Exception ex)
            {
                response2.StatusCode = 500;
                response2.Message = ex.Message;
                response2.Success = false;
                return StatusCode(500, response);
            }
        }

        /*[HttpPost,Authorize]
        [Route("/api/v1/resetPassword")]
        public ActionResult<User> ResetPassword(ResetpassModel r)
        {
            _logger.LogInformation("reset password attempt");
            try
            {
                string email = User.FindFirstValue(ClaimTypes.Email);
                response = authService.ResetPassword(r,email).Result;

                if (response.StatusCode == 404)
                {
                    return BadRequest(response);
                }
                else if (response.StatusCode == 403)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Data = ex.Data;
                return StatusCode(500, response);
            }
        }*/

        /*[HttpPost, Authorize]
        [Route("forget-password")]
        public ActionResult<User> ForgetPassword(ForgetPassModel f)
        {
            _logger.LogInformation("forget password attempt");
            try
            {
                response = authService.ForgetPassword(f).Result;

                if (response.StatusCode == 404)
                {
                    return BadRequest(response);
                }
                else if (response.StatusCode == 403)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Data = ex.Data;
                return StatusCode(500, response);
            }
        }*/

        [HttpPost, Authorize(Roles ="login")]
        [Route("/api/v1/changePassword")]
        public ActionResult<User> ChangePasswod(ChangePassModel r)
        {
            _logger.LogInformation("reset password attempt");
            try
            {
                string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                /*var user = HttpContext.User;
                string email = user.FindFirst(ClaimTypes.Email)?.Value;*/
                string? email = User.FindFirstValue(ClaimTypes.Email);
                result = authService.ChangePassword(r,email,token).Result;

                /*response2 = (ResponseWithoutData)result;

                if (response2.StatusCode == 404)
                {
                    return BadRequest(response2);
                }
                else if (response2.StatusCode == 403)
                {
                    return BadRequest(response2);
                }*/
                return Ok(result);
            }
            catch (Exception ex)
            {
                response2.StatusCode = 500;
                response2.Message = ex.Message;
                response2.Success = false;
                return StatusCode(500, response2);
            }
        }

        [HttpPost]
        [Route("/api/v1/googleAuth")]
        public async Task<IActionResult> GoogleAuth(string Token)
        {
            try
            {
                var GoogleUser = await GoogleJsonWebSignature.ValidateAsync(Token);
                result =  authService.GoogleHelper(GoogleUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                response2.StatusCode = 400;
                response2.Message = ex.Message;
                response2.Success = false;
                Console.WriteLine(ex);
                return BadRequest(response2);
            }
        }

        [HttpPost, Authorize(Roles = "login")]
        [Route("/api/v1/user/logout")]
        public ActionResult<User> Logout()
        {
            _logger.LogInformation("user logout attempt");
            try
            {
                string? email = User.FindFirstValue(ClaimTypes.Email);
                string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                result = authService.Logout(email,token).Result;

                /*response2 = (ResponseWithoutData)result;

                if (response2.StatusCode == 404)
                {
                    return BadRequest(response2);
                }
                else if (response2.StatusCode == 403)
                {
                    return BadRequest(response2);
                }*/
                return Ok(result);
            }
            catch (Exception ex)
            {
                response2.StatusCode = 500;
                response2.Message = ex.Message;
                response2.Success = false;
                return StatusCode(500, response2);
            }
        }

        /*[HttpGet]
        [Route("/login-google")]
        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                response.StatusCode = 500;
                response.Message = $"Error from external provider: {remoteError}";
                response.Data = string.Empty;
                return StatusCode(500, response);
            }

            var info = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            var claims = info.Principal.Claims.ToList();
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
            var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;

            // Use the email address to find the user in your application's database
            // If the user doesn't exist, create a new user account
            // Sign the user in using cookie authentication

            return LocalRedirect(returnUrl);
        }*/

        /*public async Task Login()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });
            return Ok(claims);
        }*/
    }
}
   
