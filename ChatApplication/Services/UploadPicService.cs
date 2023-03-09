using Microsoft.AspNetCore.Mvc;
using ChatApplication.Models;
using System.Text;
using System.Text.Json;
using ChatApplication.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Services
{
    public class UploadPicService:IUploadPicService
    {
        Response response;
        private readonly ChatAppDbContext DbContext;
        private readonly IConfiguration _configuration;

        public UploadPicService(IConfiguration configuration, ChatAppDbContext dbContext)
        {
            response = new Response();
            this._configuration = configuration;
            DbContext = dbContext;
        }

        public async Task<Response> PicUploadAsync(IFormFile file,bool IsProfilePic,string Email)
        {
            var folderName = Path.Combine("Assets","Images");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
 
            if (file.Length > 0)
            {
                var fileName = string.Concat(
                                    Path.GetFileNameWithoutExtension(file.FileName),
                                    DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                    Path.GetExtension(file.FileName)
                                    );

                var fullPath = Path.Combine(pathToSave, fileName);

                using (var stream = System.IO.File.Create(fullPath))
                {
                    await file.CopyToAsync(stream);
                }

                if (IsProfilePic == true)
                {
                    var user = await DbContext.Users.Where(u => u.Email == Email).FirstOrDefaultAsync();
                    user.PathToProfilePic = fullPath;
                    DbContext.SaveChangesAsync();
                }

                response.StatusCode= 200;
                response.Message = "File Uploaded Successfully";
                response.Data = Path.Combine(folderName,fileName);
                return response;
            }
            response.Message = "Please provide a file for successful upload";
            response.StatusCode= 400;
            response.Data= string.Empty;
            return response;
        }

    }
}