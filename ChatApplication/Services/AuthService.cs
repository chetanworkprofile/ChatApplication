using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ChatApplication.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ChatApplication.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Services
{
    public class AuthService : IAuthService
    {
        Response response = new Response();
        TokenUser tokenUser = new TokenUser();
        private readonly ChatAppDbContext DbContext;
        private readonly IConfiguration _configuration;
        

        public AuthService(IConfiguration configuration,ChatAppDbContext dbContext)
        {
            this._configuration = configuration;
            DbContext = dbContext;
        }


        public async Task<Response> CreateUser(InputUser inpUser)
        {
            var DbUsers = DbContext.Users;
            bool existingUser = DbUsers.Where(u => u.Email == inpUser.Email).Any();
            if (!existingUser)
            {
                var user = new User()
                {
                    UserId = Guid.NewGuid(),
                    PasswordHash = CreatePasswordHash(inpUser.Password),
                    FirstName = inpUser.FirstName,
                    LastName = inpUser.LastName,
                    Email = inpUser.Email,
                    DateOfBirth = inpUser.DateOfBirth,
                    CreatedAt= DateTime.Now,
                    UpdatedAt= DateTime.Now,
                    VerifiedAt= DateTime.Now,
                    IsDeleted = false,
                    PathToProfilePic = null
                };
                var responseUser = new ResponseUser()
                {
                    UserId = user.UserId,
                    FirstName= user.FirstName,
                    LastName= user.LastName,
                    Email = user.Email,
                    Phone= user.Phone,
                    DateOfBirth= user.DateOfBirth,
                    CreatedAt= user.CreatedAt,
                    UpdatedAt= user.UpdatedAt,
                };

                var tokenUser = new TokenUser()
                {
                    Email= inpUser.Email,
                    FirstName= inpUser.FirstName,
                   /* LastName= inpUser.LastName,
                    UserId = user.UserId*/
                };

                string token = CreateToken(tokenUser);
                await DbContext.Users.AddAsync(user);
                await DbContext.SaveChangesAsync();


                response.StatusCode = 200;
                response.Message = "User added";
                response.Data = token;
                return response;
            }
            else
            {
                response.StatusCode = 409;
                response.Message = "Email already registered please try another";
                response.Data = string.Empty;
                return response;
            }
        }


        public Response Login(UserDTO request)
        {
            //int index = details.Teacher.FindIndex(t => t.Username == request.Username);
            var userExists = DbContext.Users.Where(u => u.Email == request.Email).FirstOrDefault();
            if (userExists == null)
            {
                response.StatusCode = 404;
                response.Message = "User not found";
                response.Data = string.Empty;
                return response;
            }
            else if(request.Password == null)
            {
                response.StatusCode = 403;
                response.Message = "Null/Wrong password.";
                response.Data = string.Empty;
                return response;
            }
            else if (!VerifyPasswordHash(request.Password, userExists.PasswordHash))
            {
                response.StatusCode = 403;
                response.Message = "Wrong password.";
                response.Data = string.Empty;
                return response;
            }
            tokenUser.Email = userExists.Email;
            tokenUser.FirstName = userExists.FirstName;
            string token = CreateToken(tokenUser);
            response.StatusCode = 200;
            response.Message = "Login Successful";
            response.Data = token;
            return response;
        }

        public async Task<Response> Verify(VerificationModel v)
        {
            //var user = await DbContext.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == v.email);
            if(user == null)
            {
                response.StatusCode = 404;
                response.Message = "user not found";
                response.Data = string.Empty;
                return response;
            }
            if (v.token!= "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTUxMiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJkYXdpbmRlckBleGFtcGxlLmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJEYXdpbmRlciIsImV4cCI6MTY3ODI1NjU2N30.Z8UGf51PBe5Ua9Ox9bajkRB-nr1KA_tPdczX1v-ep6gy-0Gwuk4YcWynB8Il0ro2GnBUEBtVHqwV8HYm43AUew")//(user == null)
            {
                response.StatusCode = 400;
                response.Message = "Invalid Token";
                response.Data = string.Empty;
                return response;
            }
            
            //user.VerifiedAt= DateTime.UtcNow;
            await DbContext.SaveChangesAsync();
            var tokenUser = new TokenUser()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                /* LastName= inpUser.LastName,
                 UserId = user.UserId*/
            };

            string returntoken = CreateToken(tokenUser);
            response.StatusCode = 200;
            response.Message = "User Verified";
            response.Data = returntoken;
            return response;
        }

        public async Task<Response> ForgetPassword(ForgetPassModel f)
        {
            //var user = await DbContext.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == f.Email);

            if(f.Password != f.ConfirmPassword)
            {
                response.StatusCode = 400;
                response.Message = "Password and confirm password do not match";
                response.Data = string.Empty;
                return response;
            }

            if (user == null)
            {
                response.StatusCode = 404;
                response.Message = "User not found";
                response.Data = string.Empty;
                return response;
            }
            try
            {
                byte[] pass = CreatePasswordHash(f.Password);
                user.PasswordHash = pass;

                await DbContext.SaveChangesAsync();
                var responseUser = new ResponseUser()
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    DateOfBirth = user.DateOfBirth,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                };
                /*var tokenUser = new TokenUser()
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    *//* LastName= inpUser.LastName,
                     UserId = user.UserId*//*
                };*/

                //string returntoken = CreateToken(tokenUser);
                response.StatusCode = 200;
                response.Message = "Password reset successful";
                response.Data = responseUser;
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Data = ex.Data;
                return response;
            }
        }

        internal string CreateToken(TokenUser user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FirstName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        public byte[] CreatePasswordHash(string password)
        {
            byte[] salt = Encoding.ASCII.GetBytes(_configuration.GetSection("AppSettings:Password_Salt").Value!);
            byte[] passwordHash;
            using (var hmac = new HMACSHA512(salt))
            {
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
            return passwordHash;
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash)
        {
            byte[] salt = Encoding.ASCII.GetBytes(_configuration.GetSection("AppSettings:Password_Salt").Value!);
            using (var hmac = new HMACSHA512(salt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}

