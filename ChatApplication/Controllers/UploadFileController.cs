using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ChatApplication.Models;
using ChatApplication.Services;
using System.Security.Claims;
using ChatApplication.Data;

namespace ChatApplication.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UploadFileController : ControllerBase
    {
        UploadPicService uploadPicServiceInstance;
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
            try
            {
                _logger.LogInformation("File/Image Upload method started");
                string? email = User.FindFirstValue(ClaimTypes.Email);
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                result = await uploadPicServiceInstance.FileUploadAsync(file,email,token,type);

                /*response2 = (ResponseWithoutData)result;

                if (response2.StatusCode != 200)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(response2);
                }*/
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error something wrong happened ", DateTime.Now);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }


        [HttpPost, DisableRequestSizeLimit, Authorize(Roles = "login")]
        [Route("/api/v1/uploadProfilePic")]
        public async Task<IActionResult> ProfilePicUploadAsync(IFormFile file)                //[FromForm] FileUpload File
        {
            //type 2 is for image and save in images folder and type 2 is for file to save in files folder
            try
            {
                _logger.LogInformation("Pic Upload method started");
                string? email = User.FindFirstValue(ClaimTypes.Email);
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                result = await uploadPicServiceInstance.ProfilePicUploadAsync(file, email,token);

                /*response2 = (ResponseWithoutData)result;

                if (response2.StatusCode != 200)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(response2);
                }*/
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error something wrong happened ", DateTime.Now);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

    }
}
