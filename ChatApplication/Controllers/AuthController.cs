﻿using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("/api/v1/UserLogin")]
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

        [HttpPost]
        [Route("/api/v1/forget-password")]
        public ActionResult<User> ForgetPassword(string Email)
        {
            _logger.LogInformation("forget password attempt");
            try
            {
                response = authService.ForgetPassword(Email).Result;

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

        [HttpPost("/api/v1/verify")]
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
                if(response.Message == "User Verified")
                {
                    return Ok(response);
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

        [HttpPost,Authorize]
        [Route("/api/v1/reset-password")]
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
        }

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

        [HttpPost, Authorize]
        [Route("/api/v1/change-password")]
        public ActionResult<User> ChangePasswod(ChangePassModel r)
        {
            _logger.LogInformation("reset password attempt");
            try
            {
                string email = User.FindFirstValue(ClaimTypes.Email);
                response = authService.ChangePassword(r,email).Result;

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

        [HttpPost]
        [Route("/api/v1/GoogleAuth")]
        public async Task<IActionResult> GoogleAuth(string Token)
        {
            try
            {
                var GoogleUser = await GoogleJsonWebSignature.ValidateAsync(Token);
                response =  authService.GoogleHelper(GoogleUser);
                return Ok(response);
            }
            catch (Exception x)
            {
                response.StatusCode = 400;
                response.Message = x.Message;
                response.Data = x.Data;
                return BadRequest(response);
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
   
