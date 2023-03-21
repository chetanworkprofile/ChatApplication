using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ChatApplication.Models;
using ChatApplication.Services;
using System.Security.Claims;
using ChatApplication.Data;

namespace ChatApplication.Controllers
{
    //controller to handle file upload 
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UploadFileController : ControllerBase
    {
        UploadPicService uploadPicServiceInstance;      //service dependency
        private readonly ILogger<UploadFileController> _logger;
        object result = new object();
        ResponseWithoutData response2 = new ResponseWithoutData();
        Response response = new Response();
        public UploadFileController(ILogger<UploadFileController> logger, IConfiguration configuration, ChatAppDbContext dbContext)
        {
            uploadPicServiceInstance = new UploadPicService(configuration,dbContext);
            _logger = logger;
        }

        [HttpPost, DisableRequestSizeLimit, Authorize(Roles ="login")]
        [Route("/api/v1/uploadFile")]
        public async Task<IActionResult> FileUploadAsync(int type, IFormFile file)
        {
            //type 2 is for image and save in images folder and type 2 is for file to save in files folder
            _logger.LogInformation("File/Image Upload method started");
            try
            {
                string? email = User.FindFirstValue(ClaimTypes.Email);                                //extracting email from header token
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();       //getting token from authorization header
                result = await uploadPicServiceInstance.FileUploadAsync(file,email,token,type);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }


        [HttpPost, DisableRequestSizeLimit, Authorize(Roles = "login")]
        [Route("/api/v1/uploadProfilePic")]
        public async Task<IActionResult> ProfilePicUploadAsync(IFormFile file)                //[FromForm] FileUpload File
        {
            _logger.LogInformation("Pic Upload method started");
            try
            {
                string? email = User.FindFirstValue(ClaimTypes.Email);
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                result = await uploadPicServiceInstance.ProfilePicUploadAsync(file, email,token);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

    }
}
