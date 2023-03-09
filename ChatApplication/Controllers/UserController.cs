using ChatApplication.Data;
using ChatApplication.Models;
using ChatApplication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        [Route("/api/v1/users/get")]
        public IActionResult GetUsers(Guid? UserId = null, string? FirstName = null, string? LastName = null, string? Email = null, long Phone = -1, String OrderBy = "Id", int SortOrder = 1, int RecordsPerPage = 100, int PageNumber = 0)          // sort order   ===   e1 for ascending  -1 for descending
        {
            try
            {
                _logger.LogInformation("Get Students method started");
                result = userService.GetUsers(UserId, FirstName,LastName, Email, Phone, OrderBy, SortOrder, RecordsPerPage, PageNumber);
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
