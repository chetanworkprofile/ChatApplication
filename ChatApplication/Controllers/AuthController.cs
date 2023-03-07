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

namespace ChatApplication.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        IAuthService authService;
        Response response = new Response();
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration,ChatAppDbContext dbContext, ILogger<AuthController> logger)
        {
            authService = new AuthService(configuration,dbContext);
            _logger = logger;
        }

        [HttpPost]
        [Route("/api/v1/RegisterUser")]
        public IActionResult RegisterUser([FromBody] InputUser inpUser)
        {
            /*if(!ModelState.IsValid)
            {
                response.StatusCode = 400;
                response.Message = "Invalid input";
                response.Data = ValidationProblem(ModelState);
                return BadRequest(response);
            }*/
            try
            {
                _logger.LogInformation("Register User method started");
                Response response = authService.CreateUser(inpUser).Result;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error something wrong happened ", DateTime.Now);
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Data= ex.Data;
                return StatusCode(500, response);
            }
        }

        [HttpPost("UserLogin")]
        public ActionResult<User> UserLogin(UserDTO request)
        {
            _logger.LogInformation("User Login attempt");
            try
            {
                response = authService.Login(request);

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
                response.StatusCode=500;
                response.Message = ex.Message;
                response.Data= ex.Data;
                return StatusCode(500, response);
            }
        }

        [HttpPost("verify")]
        public ActionResult<User> Verify(VerificationModel v)
        {
            _logger.LogInformation("verification attempt");
            try
            {
                response = authService.Verify(v).Result;

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
        }

        [HttpPost, Authorize]
        [Route("forget-password")]
        public ActionResult<User> ForgetPasswod(ForgetPassModel f)
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
        }

        [HttpPost, Authorize]
        [Route("reset-password")]
        public ActionResult<User> ResetPasswod(ResetPassModel r)
        {
            _logger.LogInformation("reset password attempt");
            try
            {
                response = authService.ResetPassword(r).Result;

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
        }


        /*[HttpGet]
        [Route("/login-google")]
        *//*[AllowAnonymous]*//*
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
        }
*/
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
   
