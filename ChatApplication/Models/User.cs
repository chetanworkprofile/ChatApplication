﻿using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace ChatApplication.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long Phone { get; set; }
        public byte[] PasswordHash { get; set; } = new byte[32];
        public DateTime DateOfBirth { get; set; }
        public string? PathToProfilePic { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string Token { get; set; } = string.Empty;

        public int? VerificationOTP { get; set; }
        public DateTime OtpUsableTill { get; set; }
        public DateTime? VerifiedAt { get; set; }

    }
}
