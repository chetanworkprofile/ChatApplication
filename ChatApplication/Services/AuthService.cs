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

//for sending mail
using System.Net;
using System.Net.Mail;

using Google.Apis.Auth;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using ChatApplication.Hubs;

namespace ChatApplication.Services
{
    public class AuthService : IAuthService
    {
        Response response = new Response();
        ResponseWithoutData response2 = new ResponseWithoutData();
        TokenUser tokenUser = new TokenUser();
        object result = new object();
        private readonly ChatAppDbContext DbContext;
        private readonly IConfiguration _configuration;
        ChatAppHub chatAppHub;


        public AuthService(IConfiguration configuration,ChatAppDbContext dbContext)
        {
            this._configuration = configuration;
            DbContext = dbContext;
            chatAppHub = new ChatAppHub(dbContext);
        }


        public async Task<object> CreateUser(InputUser inpUser)
        {
            var DbUsers = DbContext.Users;
            bool existingUser = DbUsers.Where(u => u.Email == inpUser.Email).Any();
            if (!existingUser)
            {
                TimeSpan ageTimeSpan = DateTime.Now - inpUser.DateOfBirth;
                int age = (int)(ageTimeSpan.Days / 365.25);

                // Perform your DOB validation here based on your specific requirements
                if (age < 12)
                {
                    // The user is not enough
                    response2.StatusCode = 200;
                    response2.Message = "Not allowed to register. User is underage. Must be atleast 12 years old";
                    response2.Success = false;
                    return response2;
                }
                else if (age > 150)
                {
                    response2.StatusCode = 400;
                    response2.Message = "Not allowed to register. User is overage.Must be atmost 150 years old";
                    response2.Success = false;
                    return response2;
                }
                string regexPatternEmail = "^[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,4}$";
                if (!Regex.IsMatch(inpUser.Email, regexPatternEmail))
                {
                    response2.StatusCode = 400;
                    response2.Message = "Please Enter Valid Email";
                    response2.Success = false;
                    return response2;
                }
                string regexPatternPassword = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$";
                if (!Regex.IsMatch(inpUser.Password, regexPatternPassword))
                {
                    response2.StatusCode = 400;
                    response2.Message = "Please Enter Valid Password. Must contain atleast one uppercase letter, one lowercase letter, one number and one special chararcter and must be atleast 8 characters long";
                    response2.Success = false;
                    return response2;
                }
                string regexPatternPhone = "^[6-9]\\d{9}$";
                if (!Regex.IsMatch(inpUser.Phone.ToString(), regexPatternPhone))
                {
                    response2.StatusCode = 400;
                    response2.Message = "Please Enter Valid PhoneNo";
                    response2.Success = false;
                    return response2;
                }
                var user = new User()
                {
                    UserId = Guid.NewGuid(),
                    PasswordHash = CreatePasswordHash(inpUser.Password),
                    FirstName = inpUser.FirstName,
                    LastName = inpUser.LastName,
                    Email = inpUser.Email,
                    DateOfBirth = inpUser.DateOfBirth,
                    Phone= inpUser.Phone,
                    CreatedAt= DateTime.Now,
                    UpdatedAt= DateTime.Now,
                    VerifiedAt= DateTime.Now,
                    OtpUsableTill = DateTime.Now,
                    IsDeleted = false,
                    PathToProfilePic = null
                };
                /*var responseUser = new ResponseUser()
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    DateOfBirth = user.DateOfBirth,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    PathToProfilePic = user.PathToProfilePic
                };*/

                var tokenUser = new TokenUser()
                {
                    Email= inpUser.Email,
                    FirstName= inpUser.FirstName,
                    Role = "login"
                   /* LastName= inpUser.LastName,
                    UserId = user.UserId*/
                };

                string token = CreateToken(tokenUser);
                user.Token = token;
                await DbContext.Users.AddAsync(user);
                await DbContext.SaveChangesAsync();


                response.StatusCode = 200;
                response.Message = "User added Successfully";
                ResponseDataObj data = new ResponseDataObj()
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token  = token
                };
                response.Data = data;
                response.Success = true;
                chatAppHub.AddUserConnectionId(data.Email);
                //chatAppHub.refesh();
                return response;
            }
            else
            {
                response2.StatusCode = 409;
                response2.Message = "Email already registered please try another";
                response2.Success  = false;
                return response2;
            }
        }


        public Object Login(UserDTO request)
        {
            string regexPatternEmail = "^[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,4}$";
            if (!Regex.IsMatch(request.Email, regexPatternEmail))
            {
                response2.StatusCode = 400;
                response2.Message = "Please Enter Valid Email";
                response2.Success = false;
                return response2;
            }
            //int index = details.Teacher.FindIndex(t => t.Username == request.Username);
            var user = DbContext.Users.Where(u => u.Email == request.Email).FirstOrDefault();
            if (user == null)
            {
                response2.StatusCode = 404;
                response2.Message = "User not found";
                response2.Success = false;
                return response2;
            }
            else if(request.Password == null)
            {
                response2.StatusCode = 403;
                response2.Message = "Null/Wrong password.";
                response2.Success = false;
                return response2;
            }
            else if (!VerifyPasswordHash(request.Password, user.PasswordHash))
            {
                response2.StatusCode = 403;
                response2.Message = "Wrong password.";
                response2.Success = false;
                return response2;
            }
            tokenUser.Email = user.Email;
            tokenUser.FirstName = user.FirstName;
            tokenUser.Role = "login";
            string token = CreateToken(tokenUser);
            user.Token = token;
            DbContext.SaveChanges();
            response.StatusCode = 200;
            response.Message = "Login Successful";
            ResponseDataObj data = new ResponseDataObj()
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = user.Token,
            };
            response.Data = data;
            response.Success = true;
            chatAppHub.AddUserConnectionId(data.Email);
            //chatAppHub.refesh();
            return response;
        }

        public async Task<Object> ForgetPassword(string email)
        {
            try
            {
                //var user = await DbContext.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
                var user = await DbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
                bool exists = DbContext.Users.Where(u => u.Email == email).Any();
                Random random = new Random();
                int otp = random.Next(100000, 999999);
                user.VerificationOTP = otp;
                user.OtpUsableTill= DateTime.Now.AddHours(1);
                user.Token = string.Empty;

                if (!exists || user==null)
                {
                    response2.StatusCode = 404;
                    response2.Message = "User not found";
                    response2.Success = false;
                    return response2;
                }
           
                response2 = SendEmail(email, otp);
                await DbContext.SaveChangesAsync();
                var tokenUser = new TokenUser()
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    Role = "resetpassword"
                };
                string returntoken = CreateToken(tokenUser);
                if (response2.StatusCode == 200)
                {
                    response.StatusCode= 200;
                    response.Message= response2.Message;
                    response.Success = true;
                    ResponseDataObj data = new ResponseDataObj()
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Token = returntoken
                    };
                    response.Data = data;
                    return response;
                }
                return response2;
            }
            catch (Exception ex)
            {
                response2.StatusCode = 500;
                response2.Message = "Invalid Mail or "+ex.Message;
                response2.Success = false;
                return response2;
            }
        }

        public async Task<Object> Verify(ResetpassModel r,string email)
        {
            //var user = await DbContext.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                response2.StatusCode = 404;
                response2.Message = "User not found";
                response2.Success = false;
                return response2;
            }
            if (r.OTP != user.VerificationOTP)//(user == null)
            {
                response2.StatusCode = 400;
                response2.Message = "Invalid verification Value/Otp";
                response2.Success = false;
                return response2;
            }
            if(user.OtpUsableTill < DateTime.Now)
            {
                response2.StatusCode = 400;
                response2.Message = "Otp Expired";
                response2.Success = false;
                return response2;
            }
            user.VerifiedAt = DateTime.UtcNow;
            result = ResetPassword(r.Password, email).Result;
            user.OtpUsableTill = DateTime.Now;
            await DbContext.SaveChangesAsync();
            return result;
        }

        internal ResponseWithoutData SendEmail(string email,int value)
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
                client.Credentials = new NetworkCredential("chetan.gupta@chicmic.co.in", "Chicmic@2022");
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
            catch(Exception ex)
            {
                response2.StatusCode = 500;
                response2.Message = ex.Message;
                //response.Data = ex.Data;
                Console.WriteLine(ex);
                response2.Success = false;
                return response2;
            }
        }

        internal async Task<object> ResetPassword(string password,string email)
        {
            //var user = await DbContext.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            /*if (r.Password != r.ConfirmPassword)
            {
                response.StatusCode = 400;
                response.Message = "Password and confirm password do not match";
                response.Data = string.Empty;
                return response;
            }*/
            string regexPatternPassword = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$";
            if (!Regex.IsMatch(password, regexPatternPassword))
            {
                response2.StatusCode = 400;
                response2.Message = "Please Enter Valid Password. Must contain atleast one uppercase letter, one lowercase letter, one number and one special chararcter and must be atleast 8 characters long";
                response2.Success = false;
                return response2;
            }
            try
            {
                byte[] pass = CreatePasswordHash(password);
                user.PasswordHash = pass;

                tokenUser.Email = user.Email;
                tokenUser.FirstName = user.FirstName;
                tokenUser.Role = "login";
                string token = CreateToken(tokenUser);
                user.Token = token;
                await DbContext.SaveChangesAsync();
                var responsedata = new ResponseDataObj()
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Token = token
                };
                
                response.StatusCode = 200;
                response.Message = "Password reset successful";
                response.Data = responsedata;
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Data = ex.Data;
                response.Success = false;
                return response;
            }
        }

        public Object GoogleHelper(GoogleJsonWebSignature.Payload user)
        {
            if(user.EmailVerified != true)
            {
                response2.StatusCode = 400;
                response2.Message = "Email not verified";
                response2.Success = false;
                return response2;
            }
            string returntoken;
            var users = DbContext.Users;
            var userExists = users.Where( u => u.Email == user.Email).FirstOrDefault();
            if (userExists == null)
            {
                User newUser = new User()
                {
                    Email= user.Email,
                    FirstName = user.GivenName,
                    LastName = user.FamilyName,
                    UserId = new Guid(),
                    PasswordHash = CreatePasswordHash("gsahgfas@#$2343hjdshadAAHSHG676@@dJHJSd"),
                    IsDeleted= false,
                    CreatedAt = DateTime.Now,
                    DateOfBirth = DateTime.Now,
                    PathToProfilePic= user.Picture,
                    UpdatedAt= DateTime.Now,
                    Phone  = 9999999999
                };
                
                var tokenUser = new TokenUser()
                {
                    Email = newUser.Email,
                    FirstName = newUser.FirstName,
                    Role = "login"
                    /* LastName= inpUser.LastName,
                     UserId = user.UserId*/
                };

                returntoken = CreateToken(tokenUser);
                newUser.Token = returntoken;
                DbContext.Users.Add(newUser);
                DbContext.SaveChanges();
            }
            else
            {
                var tokenUser = new TokenUser()
                {
                    Email = user.Email,
                    FirstName = user.GivenName,
                    /* LastName= inpUser.LastName,
                     UserId = user.UserId*/
                };

                returntoken = CreateToken(tokenUser);
                userExists.Token= returntoken;
                DbContext.SaveChanges();
            }

            response.StatusCode=200;
            response.Message = "Login successful";
            ResponseDataObj data = new ResponseDataObj()
            {
                UserId = userExists.UserId,
                Email = userExists.Email,
                FirstName = userExists.FirstName,
                LastName = userExists.LastName,
                Token = returntoken
            };
            response.Data = data;
            response.Success= true;
            return response;
        }

        public async Task<object> ChangePassword(ChangePassModel r,string email,string token)
        {
            //var user = await DbContext.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            //var PasswordHash = CreatePasswordHash(r.oldPassword);
            if (token != user.Token)
            {
                response2.StatusCode = 401;
                response2.Message = "Invalid/expired token. Login First";
                response2.Success = false;
                return response2;
            }
            string regexPatternPassword = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$";
            if (!Regex.IsMatch(r.Password, regexPatternPassword))
            {
                response2.StatusCode = 400;
                response2.Message = "Enter Valid Password. Must contain atleast one uppercase letter, one lowercase letter, one number and one special chararcter and must be atleast 8 characters long";
                response2.Success = false;
                return response2;
            }
            if (!VerifyPasswordHash(r.oldPassword, user.PasswordHash))
            {
                response2.StatusCode = 400;
                response2.Message = "Invalid Old password";
                response2.Success = false;
                return response2;
            }


            if (user == null)
            {
                response2.StatusCode = 404;
                response2.Message = "User not found";
                response2.Success = false;
                return response2;
            }
            try
            {
                byte[] pass = CreatePasswordHash(r.Password);
                user.PasswordHash = pass;

                tokenUser.Email = user.Email;
                tokenUser.FirstName = user.FirstName;
                tokenUser.Role = "login";
                /*string token = CreateToken(tokenUser);
                user.Token = token;*/
                await DbContext.SaveChangesAsync();
                var responsedata = new ResponseDataObj()
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Token = user.Token
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
                response.Message = "Password change successful";
                response.Data = responsedata;
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response2.StatusCode = 500;
                response2.Message = ex.Message;
                //response.Data = ex.Data;
                response2.Success = false;
                return response2;
            }
        }

        public async Task<object> Logout(string email,string token)
        {
            var user = await DbContext.Users.Where(u=>u.Email==email).FirstOrDefaultAsync();

            if (user == null)
            {
                response2.StatusCode = 404;
                response2.Message = "User not found";
                response2.Success = false;
                return response2;
            }
            if(token!=user.Token)
            {
                response2.StatusCode = 401;
                response2.Message = "Invalid/expired token. Login First";
                response2.Success = false;
                return response2;
            }
            try
            {
                user.Token = string.Empty;
                await DbContext.SaveChangesAsync();
                var responsedata = new ResponseDataObj()
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Token = token
                };
                /*var tokenUser = new TokenUser()
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    *//* LastName= inpUser.LastName,
                     UserId = user.UserId*//*
                };*/

                //string returntoken = CreateToken(tokenUser);
                response2.StatusCode = 200;
                response2.Message = "User Logged out Successfully";
                chatAppHub.RemoveUserFromList(email);
                //chatAppHub.refesh();
                //response2.Data = responsedata;
                response2.Success = true;
                return response2;
            }
            catch (Exception ex)
            {
                response2.StatusCode = 500;
                response2.Message = ex.Message;
                //response.Data = ex.Data;
                response2.Success = false;
                return response2;
            }
        }

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

