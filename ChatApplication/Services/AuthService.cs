using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ChatApplication.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ChatApplication.Data;

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
                response.Data = responseUser;
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

