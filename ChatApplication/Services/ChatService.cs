using ChatApplication.Data;
using ChatApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Services
{
    /* public class ChatService : IChatService
     {
         Response response = new Response();
         ResponseWithoutData response2 = new ResponseWithoutData();
         //TokenUser tokenUser = new TokenUser();
         private readonly ChatAppDbContext DbContext;
         private readonly IConfiguration _configuration;
         IAuthService authService;

         public ChatService(IConfiguration configuration, ChatAppDbContext dbContext)
         {
             this._configuration = configuration;
             DbContext = dbContext;
             //authService = new AuthService(configuration, dbContext);
         }




         public object GetUsers(string email, string token, Guid? UserId, string? FirstName, string? LastName, string? Email, long Phone, String OrderBy, int SortOrder, int RecordsPerPage, int PageNumber)          // sort order   ===   e1 for ascending   -1 for descending
         {
             var userLoggedIn = DbContext.Users.Where(u => u.Email == email).FirstOrDefault();
             var users = DbContext.Users.ToList();
             users = users.Where(t => t.IsDeleted == false).ToList();
             if (token != userLoggedIn.Token)
             {
                 response2.StatusCode = 404;
                 response2.Message = "Invalid/expired token. Login First";
                 response2.Success = false;
                 return response2;
             }
             if (UserId != null) { users = users.Where(s => (s.UserId == UserId)).ToList(); }
             if (FirstName != null) { users = users.Where(s => (s.FirstName == FirstName)).ToList(); }
             if (FirstName != null) { users = users.Where(s => (s.FirstName == FirstName)).ToList(); }
             if (Email != null) { users = users.Where(s => (s.Email == Email)).ToList(); }
             if (Phone != -1) { users = users.Where(s => (s.Phone == Phone)).ToList(); }
             if (subjectId != null)
             {
                 List<SubjectTeacherMappingInput> subjectTeacherId = (List<SubjectTeacherMappingInput>)DbContext.SubjectTeachersMappings.Where(st => st.SubjectId == subjectId).Select(st => st);
                 //SubjectTeacherMappings temp = DbContext.SubjectTeachersMappings.Where(s => s.SubjectId.(subjectId));
                 student = (DbSet<Student>)student.Where(s => s.SubjectTeacherAllocated.));
             }


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
                 ResponseUser r = new ResponseUser()
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
                 response2.Success = true;
                 return response2;
             }

             response.StatusCode = 200;
             response.Message = "Users list fetched";
             response.Data = res;
             response.Success = true;
             return response;
         }

         public async Task<object> UpdateUser(string email, UpdateUser u, string tokenloggedin)
         {
             User? user = await DbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
             TimeSpan ageTimeSpan = DateTime.Now - u.DateOfBirth;
             int age = (int)(ageTimeSpan.Days / 365.25);

             if (tokenloggedin != user.Token)
             {
                 response2.StatusCode = 404;
                 response2.Message = "Invalid/expired token. Login First";
                 response2.Success = false;
                 return response2;
             }


             if (user != null && user.IsDeleted == false)
             {
                 if (u.FirstName != "string" && u.FirstName != null)
                 {
                     user.FirstName = u.FirstName;
                 }
                 if (u.LastName != "string" && u.LastName != null)
                 {
                     user.LastName = u.LastName;
                 }
                 if (u.Email != "string" && u.Email != null)
                 {
                     bool EmailAlreadyExists = DbContext.Users.Where(s => s.Email == u.Email).Any();
                     if (EmailAlreadyExists)
                     {
                         response2.Success = false;
                         response2.StatusCode = 400;
                         response2.Message = "Email you entered already registered. Please try another";
                         return response2;
                     }
                     user.Email = u.Email;
                 }
                 if (u.PathToProfilePic != "string" && u.PathToProfilePic != null)
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
                 user.UpdatedAt = DateTime.Now;
                 await DbContext.SaveChangesAsync();
                 ResponseUser resuser = new ResponseUser()
                 {
                     UserId = user.UserId,
                     Email = email,
                     FirstName = user.FirstName,
                     LastName = user.LastName,
                     CreatedAt = user.CreatedAt,
                     DateOfBirth = user.DateOfBirth,
                     PathToProfilePic = user.PathToProfilePic,
                     Phone = user.Phone,
                     UpdatedAt = user.UpdatedAt
                 };
                 var tokenUser = new TokenUser()
                 {
                     Email = user.Email,
                     FirstName = user.FirstName,
                     Role = "login"
                      LastName = inpUser.LastName,
                     UserId = user.UserId
                 };
                 string token = authService.CreateToken(tokenUser);
                 user.Token = token;
                 ResponseDataObj data = new ResponseDataObj()
                 {
                     UserId = user.UserId,
                     Email = user.Email,
                     FirstName = user.FirstName,
                     LastName = user.LastName,
                     Token = token
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

         public async Task<object> DeleteUser(string email, string token, string password)
         {
             User? user = await DbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
             if (token != user.Token)
             {
                 response2.StatusCode = 404;
                 response2.Message = "Invalid/expired token. Login First";
                 response2.Success = false;
                 return response2;
             }
             byte[] phash = user.PasswordHash;
             if (!authService.VerifyPasswordHash(password, phash))
             {
                 response2.Success = false;
                 response2.Message = "Invalid Password";
                 response2.StatusCode = 400;
                 return response2;
             }

             if (user != null && user.IsDeleted == false)
             {
                 user.IsDeleted = true;
                 user.Token = string.Empty;
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
     }*/
    public class ChatService : IChatService
    {
        // Key, Value eg: { {"abc@gmail.com", "unique_connection_id"}
        private static readonly Dictionary<string, string> Users = new Dictionary<string, string>();

        public bool AddUserToList(string userToAdd,string connectionId)
        {
            lock (Users)
            {
                foreach (var user in Users)
                {
                    if (user.Key.ToLower() == userToAdd.ToLower())
                    {
                        return false;
                    }
                }

                Users.Add(userToAdd, connectionId);
                return true;
            }
        }

        /*public void AddUserConnectinId(string user, string connectionId)
        {
            lock (Users)
            {
                if (Users.ContainsKey(user))
                {
                    Users[user] = connectionId;
                }
            }
        }*/

        public string GetUserByConnectionId(string connectionId)
        {
            lock (Users)
            {
                return Users.Where(x => x.Value == connectionId).Select(x => x.Key).FirstOrDefault();
            }
        }

        public string GetConnectionIdByUser(string user)
        {
            lock (Users)
            {
                return Users.Where(x => x.Key == user).Select(x => x.Value).FirstOrDefault();
            }
        }

        public void RemoveUserFromList(string user)
        {
            lock (Users)
            {
                if (Users.ContainsKey(user))
                {
                    Users.Remove(user);
                }
            }
        }

        public string[] GetOnlineUsers()
        {
            lock (Users)
            {
                return Users.OrderBy(x => x.Key).Select(x => x.Key).ToArray();
            }
        }
    }
}

