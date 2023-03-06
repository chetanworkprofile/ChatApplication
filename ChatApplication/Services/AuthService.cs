using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ChatApplication.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ChatApplication.Services
{
    public class AuthService : IAuthService
    {
        Response response = new Response();
        User user = new User();
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            this._configuration = configuration;
            data = File.ReadAllText(Constants.path);
            details = JsonSerializer.Deserialize<JsonData>(data, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
        }

        public Response TeacherLogin(UserDTO request)
        {
            int index = details.Teacher.FindIndex(t => t.Username == request.Username);
            if (index == -1)
            {
                response.StatusCode = 404;
                response.Message = "User not found";
                response.Data = string.Empty;
                return response;
            }
            else if (!VerifyPasswordHash(request.Password, details.Teacher[index].PasswordHash))
            {
                response.StatusCode = 403;
                response.Message = "Wrong password.";
                response.Data = string.Empty;
                return response;
            }
            user.Username = request.Username;
            user.UserRole = "Teacher";
            string token = CreateToken(user);
            response.StatusCode = 200;
            response.Message = "Login Successful";
            response.Data = token;
            return response;
        }

        internal string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.UserRole)
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







// just a backup in case there's a need to add student login and authorization
/*        [HttpPost("StudentRegistration")]
        public ActionResult<User> StudentRegisteraton(UserDTO request)
        {
            user.PasswordHash = CreatePasswordHash(request.Password);
            user.Username = request.Username;
            user.UserRole = "Student";
            //details.Users.Add(user);
            string token = CreateToken(user);

            return Ok(token);
        }

        [HttpPost("StudentLogin")]
        public ActionResult<User> StudentLogin(UserDTO request)
        {
            if (user.Username != request.Username)
            {
                return BadRequest("User not found");
            }
            if (!VerifyPasswordHash(request.Password, user.PasswordHash))
            {
                return BadRequest("Wrong password.");
            }
            user.UserRole = "Student";
            string token = CreateToken(user);

            return Ok(token);
        }*/