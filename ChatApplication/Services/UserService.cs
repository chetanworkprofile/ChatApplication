﻿using ChatApplication.Controllers;
using ChatApplication.Data;
using ChatApplication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace ChatApplication.Services
{
    public class UserService: IUserService
    {
        Response response = new Response();
        ResponseWithoutData response2 = new ResponseWithoutData();
        //TokenUser tokenUser = new TokenUser();
        private readonly ChatAppDbContext DbContext;
        private readonly IConfiguration _configuration;
        IAuthService authService;
        // secondary service file to make code clean
        SecondaryAuthService _secondaryAuthService;

        public UserService(IConfiguration configuration, ChatAppDbContext dbContext, ILogger<AuthController> logger)
        {
            this._configuration = configuration;
            DbContext = dbContext;
            authService = new AuthService(configuration, dbContext, logger);
            _secondaryAuthService = new SecondaryAuthService(configuration, dbContext, logger);
        }

        public object GetYourself(string email, string token)
        {
            var userLoggedIn = DbContext.Users.Where(u => u.Email == email).FirstOrDefault();
            
            if(userLoggedIn == null || userLoggedIn.IsDeleted==true)
            {
                response2.StatusCode = 404;
                response2.Message = "Can't get details user error";
                response2.Success = false;
                return response2;
            }

            if (token != userLoggedIn.Token)
            {
                response2.StatusCode = 401;
                response2.Message = "Invalid/expired token. Login First";
                response2.Success = false;
                return response2;
            }

            ResponseUser r = new ResponseUser()
            {
                UserId = userLoggedIn.UserId,
                FirstName = userLoggedIn.FirstName,
                LastName = userLoggedIn.LastName,
                Email = userLoggedIn.Email,
                Phone = userLoggedIn.Phone,
                DateOfBirth = userLoggedIn.DateOfBirth,
                CreatedAt = userLoggedIn.CreatedAt,
                UpdatedAt = userLoggedIn.UpdatedAt,
                PathToProfilePic = userLoggedIn.PathToProfilePic,
            };


            response.StatusCode = 200;
            response.Message = "Users list fetched";
            response.Data = r;
            response.Success = true;
            return response;
        }
        public object GetUsers(string email,string token,Guid? UserId, string? searchString, string? Email, long Phone, String OrderBy, int SortOrder, int RecordsPerPage, int PageNumber)          // sort order   ===   e1 for ascending   -1 for descending
        {
            //get logged in user from database
            var userLoggedIn = DbContext.Users.Where(u => u.Email == email).FirstOrDefault();
            var userss = DbContext.Users.AsQueryable();
            userss = userss.Where(u => u.Email != email);       //remove logged in user from list

            //userss = userss.Where(x => (x.UserId == UserId || UserId == null) && (x.IsDeleted == false) && (EF.Functions.Like(x.FirstName, "%" + searchString + "%") || EF.Functions.Like(x.LastName, "%" + searchString + "%") || searchString == null) &&
            //(x.Email == Email || Email == null)).Select(x => x);


            userss = userss.Where(t => t.IsDeleted == false);     //remove deleted users from list
            
            if (token != userLoggedIn.Token)
            {
                response2.StatusCode = 401;
                response2.Message = "Invalid/expired token. Login First";
                response2.Success = false;
                return response2;
            }

            //--------------------------filtering based on userId,searchString, Email, or Phone---------------------------------//
            
            if (UserId != null) { userss = userss.Where(s => (s.UserId == UserId)); }
            if (searchString != null) { userss = userss.Where(s => EF.Functions.Like(s.FirstName, "%" + searchString + "%") || EF.Functions.Like(s.LastName, "%" + searchString + "%") || EF.Functions.Like(s.FirstName+" "+s.LastName, "%" + searchString + "%")); }
            //if (FirstName != null) { users = users.Where(s => (s.FirstName == FirstName)).ToList(); }
            if (Email != null) { userss = userss.Where(s => (s.Email == Email)); }
            if (Phone != -1) { userss = userss.Where(s => (s.Phone == Phone)); }

            var users = userss.ToList();

            // delegate used to create orderby depending on user input
            Func<User, Object> orderBy = s => s.UserId;
            if (OrderBy == "UserId" || OrderBy == "ID" || OrderBy == "Id")
            {
                orderBy = x => x.UserId;
            }
            else if (OrderBy == "FirstName" || OrderBy == "Name" || OrderBy == "firstname")
            {
                orderBy = x => x.FirstName;
            }
            else if (OrderBy == "Email" || OrderBy == "email")
            {
                orderBy = x => x.Email;
            }

            // sort according to input based on orderby
            if (SortOrder == 1)
            {
                users = users.OrderBy(orderBy).Select(c => (c)).ToList();
            }
            else
            {
                users = users.OrderByDescending(orderBy).Select(c => (c)).ToList();
            }

            //pagination
            users = users.Skip((PageNumber - 1) * RecordsPerPage)
                                  .Take(RecordsPerPage).ToList();

            List<ResponseUser> res = new List<ResponseUser>();

            foreach (var user in users)
            {
                ResponseUser r = new ResponseUser()                 //need to use automapper or constructor here
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    DateOfBirth = user.DateOfBirth,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    PathToProfilePic = user.PathToProfilePic,
                };
                res.Add(r);
            }

            if (!res.Any())
            {
                response2.StatusCode = 200;
                response2.Message = "No user found";
                response2.Success= true;
                return response2;
            }

            response.StatusCode = 200;
            response.Message = "Users list fetched";
            response.Data = res;
            response.Success = true;
            return response;
        }

        public async Task<object> UpdateUser(string email, UpdateUser u,string tokenloggedin)
        {
            User? user = await DbContext.Users.Where(u=>u.Email == email).FirstOrDefaultAsync();
            TimeSpan ageTimeSpan = DateTime.Now - u.DateOfBirth;
            int age = (int)(ageTimeSpan.Days / 365.25);

            if (tokenloggedin != user.Token)
            {
                response2.StatusCode = 401;
                response2.Message = "Invalid/expired token. Login First";
                response2.Success = false;
                return response2;
            }


            if (user != null && user.IsDeleted == false)
            {
                if (u.FirstName != "string" && u.FirstName != string.Empty)
                {
                    user.FirstName = u.FirstName;
                }
                if (u.LastName != "string" && u.LastName != string.Empty)
                {
                    user.LastName = u.LastName;
                }
               /* if (u.Email != "string" && u.Email != string.Empty)
                {
                    var EmailAlreadyExists = DbContext.Users.Where(s => s.Email == u.Email).First();
                    if(EmailAlreadyExists!=null)
                    {
                        response2.Success = false;
                        response2.StatusCode = 400;
                        response2.Message = "Email you entered already registered. Please try another";
                        return response2;
                    }
                    user.Email = u.Email;
                }*/
                if (u.PathToProfilePic != "string" && u.PathToProfilePic != string.Empty)
                {
                    user.PathToProfilePic = u.PathToProfilePic;
                }
                if (u.Phone != -1 && u.Phone != 0)
                {
                    user.Phone = u.Phone;
                }
                if (u.DateOfBirth != DateTime.MinValue && u.DateOfBirth != DateTime.Now)
                {
                    // Perform your DOB validation here based on your specific requirements
                    if (age < 12)
                    {
                        // The user is not enough
                        response2.StatusCode = 200;
                        response2.Message = "Not allowed. User is underage. Must be atleast 12 years old";
                        response2.Success = false;
                        return response2;
                    }
                    user.DateOfBirth = u.DateOfBirth;
                }
                user.UpdatedAt= DateTime.Now;
                await DbContext.SaveChangesAsync();
                
                /*var tokenUser = new TokenUser()
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    Role = "login"
                    *//* LastName= inpUser.LastName,
                     UserId = user.UserId*//*
                };
                string token = _secondaryAuthService.CreateToken(tokenUser);
                user.Token = token;*/
                ResponseDataObj2 data = new ResponseDataObj2()
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    //Token = token
                };
                await DbContext.SaveChangesAsync();
                response.StatusCode = 200;
                response.Message = "User updated successfully";
                response.Success = true;
                response.Data = data;
                return response;
            }
            else
            {
                response2.StatusCode = 404;
                response2.Message = "User not found";
                response2.Success = false;
                return response2;
            }
        }

        public async Task<object> DeleteUser(string email, string token,string password)
        {
            User? user = await DbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
            if (token != user.Token)
            {
                response2.StatusCode = 401;
                response2.Message = "Invalid/expired token. Login First";
                response2.Success = false;
                return response2;
            }
            byte[] phash = user.PasswordHash;
            if (!_secondaryAuthService.VerifyPasswordHash(password, phash)){
                response2.Success = false;
                response2.Message = "Invalid Password";
                response2.StatusCode = 400;
                return response2;
            }

            if (user != null && user.IsDeleted == false)
            {
                user.IsDeleted = true;
                user.Token= string.Empty;
                await DbContext.SaveChangesAsync();

                response2.StatusCode = 200;
                response2.Message = "User deleted successfully";
                response2.Success = true;
                return response2;
            }
            else
            {
                response2.StatusCode = 404;
                response2.Message = "User Not found";
                response2.Success = false;
                return response2;
            }

        }
    }
}
