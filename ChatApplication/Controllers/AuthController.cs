using Microsoft.AspNetCore.Mvc;
using ChatApplication.Models;
using ChatApplication.Services;
using ChatApplication.Data;

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
    }
}
   
