using Microsoft.AspNetCore.Mvc;
using StudentManagementSystemAPI.Models;
using StudentManagementSystemAPI.Services;


namespace StudentManagementSystemAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        AuthService authService;
        TeacherService teacherService;
        Response response = new Response();
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            authService = new AuthService(configuration);
            teacherService = new TeacherService(configuration);
            _logger = logger;
        }

        [HttpPost]
        [Route("/api/v1/RegisterTeacher")]
        public IActionResult AddTeacher([FromBody] AddTeacher addTeacher)
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
                _logger.LogInformation("Add Teacher method started");
                Response response = teacherService.AddTeacher(addTeacher);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error something wrong happened ", DateTime.Now);
                return StatusCode(500, $"Internal server error: {ex}"); ;
            }
        }

        [HttpPost("TeacherLogin")]
        public ActionResult<User> TeacherLogin(UserDTO request)
        {
            _logger.LogInformation("Teacher Login attempt");
            response = authService.TeacherLogin(request);

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
    }
}
   
