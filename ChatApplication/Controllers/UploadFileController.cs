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
        public UploadFileController(ILogger<UploadFileController> logger, IConfiguration configuration, ChatAppDbContext dbContext)
        {
            uploadPicServiceInstance = new UploadPicService(configuration,dbContext);
            _logger = logger;
        }

        [HttpPost, DisableRequestSizeLimit, Authorize]
        [Route("/api/v1/UploadFiles")]
        public async Task<IActionResult> PicUploadAsync(IFormFile file, bool IsProfilePic = false)
        {
            try
            {
                _logger.LogInformation("Pic Upload method started");
                string email = User.FindFirstValue(ClaimTypes.Email);
                Response response = await uploadPicServiceInstance.PicUploadAsync(file,IsProfilePic,email);
                if (response.StatusCode == 200)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error something wrong happened ", DateTime.Now);
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
    }
}
