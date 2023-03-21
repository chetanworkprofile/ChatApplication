using ChatApplication.Models;
using System.Net.Mail;
using System.Net;
using ChatApplication.Data;
using ChatApplication.Hubs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ChatApplication.Controllers;

namespace ChatApplication.Services
{
    //class used to provide service to another auth service file 
    public class SecondaryAuthService
    {
        Response response = new Response();
        ResponseWithoutData response2 = new ResponseWithoutData();
        TokenUser tokenUser = new TokenUser();
        object result = new object();
        private readonly ChatAppDbContext DbContext;
        private readonly IConfiguration _configuration;
        ChatAppHub chatAppHub;


        public SecondaryAuthService(IConfiguration configuration, ChatAppDbContext dbContext, ILogger<AuthController> logger)
        {
            this._configuration = configuration;
            DbContext = dbContext;
            chatAppHub = new ChatAppHub(dbContext,logger);
        }
        internal ResponseWithoutData SendEmail(string email, int value)
        {
            try
            {
                MailMessage message = new MailMessage();

                // set the sender and recipient email addresses
                message.From = new MailAddress("verification@chatapp.chicmic.co.in");
                message.Subject = "Mail Verification by ChatApplication. Verify your account";
                message.To.Add(new MailAddress(email));

                // set the subject and body of the email
                //message.Subject = "Verify your account";
                message.Body = "Please verify your reset password attempt. Your One Time Password for verification is " + value;             /*"Please click on the following link to verify your account: http://192.180.2.159:4040/api/v1/verify?email=" + email+"&value"+value;    //verificationCode;*/

                // create a new SmtpClient object
                SmtpClient client = new SmtpClient();

                // set the SMTP server credentials and port
                client.Credentials = new NetworkCredential(_configuration.GetSection("MailCredentials:email").Value!, _configuration.GetSection("MailCredentials:password").Value!);
                client.Host = "mail.chicmic.co.in";
                client.Port = 587;
                client.EnableSsl = true;

                // send the email
                client.Send(message);
                response2.StatusCode = 200;
                response2.Message = "Verification Email Sent successfully";
                //response.Data = string.Empty;
                response2.Success = true;
                return response2;
            }
            catch (Exception ex)
            {
                response2.StatusCode = 500;
                response2.Message = ex.Message;
                //response.Data = ex.Data;
                Console.WriteLine(ex);
                response2.Success = false;
                return response2;
            }
        }

        //function used to create token 
        public string CreateToken(TokenUser user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Role,user.Role)
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

        //used to create password hash that is stored in database
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

        //to verify password with hash stored in database
        public bool VerifyPasswordHash(string password, byte[] passwordHash)
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
