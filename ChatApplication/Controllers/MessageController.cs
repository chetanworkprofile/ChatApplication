/*using ChatApplication.Data;
using ChatApplication.Models;
using ChatApplication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ChatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {

        //UploadPicService uploadPicServiceInstance;
        private readonly ILogger<UploadFileController> _logger;
        object result = new object();
        ResponseWithoutData response2 = new ResponseWithoutData();
        Response response = new Response();
        public MessageController(ILogger<UploadFileController> logger, IConfiguration configuration, ChatAppDbContext dbContext)
        {
            //uploadPicServiceInstance = new UploadPicService(configuration, dbContext);
            _logger = logger;
        }

       *//* [HttpPost]
        [Route("/api/v1/message/send")]
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
        }*//*
    }
}
*/