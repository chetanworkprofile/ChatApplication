﻿using ChatApplication.Models;

namespace ChatApplication.Services
{
    public interface IUploadPicService
    {
        public Task<object> PicUploadAsync(IFormFile file, bool IsProfilePic, string Email);
    }
}
