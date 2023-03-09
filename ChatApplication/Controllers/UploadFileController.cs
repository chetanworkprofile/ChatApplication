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
        [Route("/api/v1/uploadFiles")]
        public async Task<IActionResult> PicUploadAsync(IFormFile file, bool IsProfilePic = false)
        {
            try
            {
                _logger.LogInformation("Pic Upload method started");
                string email = User.FindFirstValue(ClaimTypes.Email);
                result = await uploadPicServiceInstance.PicUploadAsync(file,IsProfilePic,email);

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
