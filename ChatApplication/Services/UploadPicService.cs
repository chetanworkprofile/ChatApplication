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
        ResponseWithoutData response2 = new ResponseWithoutData();
        private readonly ChatAppDbContext DbContext;
        private readonly IConfiguration _configuration;

        public UploadPicService(IConfiguration configuration, ChatAppDbContext dbContext)
        {
            response = new Response();
            this._configuration = configuration;
            DbContext = dbContext;
        }

        public async Task<object> FileUploadAsync(IFormFile file,string Email,string token,int type)
        {
            User? user = user = await DbContext.Users.Where(u => u.Email == Email).FirstOrDefaultAsync();

            if (token != user.Token)
            {
                response2.StatusCode = 401;
                response2.Message = "Invalid/expired token. Login First";
                response2.Success = false;
                return response2;
            }
            if (file == null)
            {
                response2.Message = "Please provide a file for successful upload";
                response2.StatusCode = 400;
                response2.Success = false;
                return response2;
            }
            if (file.Length > 0)
            {
                string folderName;
                if (type == 2)
                {
                    folderName = Path.Combine("Assets", "Images"); 
                }
                else
                {
                    folderName = Path.Combine("Assets", "Files");
                }
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
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
                FileUploadResponse res = new FileUploadResponse()
                {
                    FileName = fileName,
                     PathToFile= Path.Combine(folderName, fileName)
            };
                response.StatusCode= 200;
                response.Message = "File Uploaded Successfully";
                response.Success = true;
                response.Data = res;
                return response;
            }
            response2.Message = "Please provide a file for successful upload";
            response2.StatusCode = 400;
            response2.Success = false;
            return response2;
        }


        public async Task<object> ProfilePicUploadAsync(IFormFile file, string Email, string token)
        {
            User? user = user = await DbContext.Users.Where(u => u.Email == Email).FirstOrDefaultAsync();
            var folderName = Path.Combine("Assets", "ProfilePics");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            if (token != user.Token)
            {
                response2.StatusCode = 401;
                response2.Message = "Invalid/expired token. Login First";
                response2.Success = false;
                return response2;
            }
            if (file == null)
            {
                response2.Message = "Please provide a file for successful upload";
                response2.StatusCode = 400;
                response2.Success = false;
                return response2;
            }
            if (file.Length > 0)
            {
                var fileName = string.Concat(
                                    Email,
                                    DateTime.Now.ToString("yyyyMMddHHmmssfff"), 
                                    Path.GetFileNameWithoutExtension(file.FileName),
                                    Path.GetExtension(file.FileName)
                                    );

                var fullPath = Path.Combine(pathToSave, fileName);

                using (var stream = System.IO.File.Create(fullPath))
                {
                    await file.CopyToAsync(stream);
                }
                user.PathToProfilePic = Path.Combine(folderName, fileName);
                await DbContext.SaveChangesAsync();

                response.StatusCode = 200;
                response.Message = "File Uploaded Successfully";
                ResponseUser responseUser = new ResponseUser()
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    CreatedAt = user.CreatedAt,
                    Phone = user.Phone,
                    PathToProfilePic = user.PathToProfilePic,
                    UpdatedAt = user.UpdatedAt
                };
                FileResponseData data = new FileResponseData()
                {
                    User = responseUser,
                    FileName = fileName,
                    PathToPic = Path.Combine(folderName, fileName)
                };
                response.Data = data;
                response.Success = true;
                return response;
            }
            response2.Message = "Please provide a file for successful upload";
            response2.StatusCode = 400;
            response2.Success = false;
            return response2;
        }


    }
}