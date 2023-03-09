﻿namespace ChatApplication.Models
{
    public class ResponseDataObj
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } =  string.Empty;
        public string FirstName = string.Empty;
        public string LastName = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
