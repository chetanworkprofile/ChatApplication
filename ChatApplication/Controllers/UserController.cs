using ChatApplication.Data;
using ChatApplication.Models;
using ChatApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        IUserService userService;
        Response response = new Response();
        object result = new object();
        private readonly ILogger<AuthController> _logger;

        public UserController(IConfiguration configuration, ChatAppDbContext dbContext, ILogger<AuthController> logger)
        {
            userService = new UserService(configuration, dbContext);
            _logger = logger;
        }

        [HttpGet, Authorize(Roles = "login")]
        [Route("/api/v1/users/get")]
        public IActionResult GetUsers(Guid? UserId = null, string? FirstName = null, string? LastName = null, string? Email = null, long Phone = -1, String OrderBy = "Id", int SortOrder = 1, int RecordsPerPage = 100, int PageNumber = 0)          // sort order   ===   e1 for ascending  -1 for descending
        {
            try
            {
                _logger.LogInformation("Get Students method started");
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                string? email = User.FindFirstValue(ClaimTypes.Email);
                result = userService.GetUsers(email,token,UserId, FirstName,LastName, Email, Phone, OrderBy, SortOrder, RecordsPerPage, PageNumber);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error something wrong happened ", DateTime.Now);
                return StatusCode(500, $"Internal server error: {ex}"); ;
            }
        }

        [HttpPut ,Authorize(Roles = "login")]
        [Route("/api/v1/users/update")]
        public IActionResult UpdateStudent(UpdateUser u)
        {
            try
            {
                string? email = User.FindFirstValue(ClaimTypes.Email);
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                _logger.LogInformation("Update user method started");
                result = userService.UpdateUser(email,u,token).Result;
                /*if (response.StatusCode == 200)
                {
                    return Ok(response);
                }*/
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error something wrong happened ", DateTime.Now);
                return StatusCode(500, $"Internal server error: {ex}"); ;
            }
        }

        [HttpDelete, Authorize(Roles = "login")]
        [Route("/api/v1/user/delete")]
        public IActionResult DeleteUser(string Password)
        {
            try
            {
                string? email = User.FindFirstValue(ClaimTypes.Email);
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                _logger.LogInformation("Delete Student method started");
                result = userService.DeleteUser(email,token,Password).Result;
                /*if (response.StatusCode == 200)
                {
                    return Ok(response);
                }*/
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error something wrong happened ", DateTime.Now);
                return StatusCode(500, $"Internal server error: {ex}"); ;
            }
        }


    }
}
