using ChatApplication.Data;
using ChatApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Services
{
    public class UserService: IUserService
    {
        Response response = new Response();
        //TokenUser tokenUser = new TokenUser();
        private readonly ChatAppDbContext DbContext;
        private readonly IConfiguration _configuration;


        public UserService(IConfiguration configuration, ChatAppDbContext dbContext)
        {
            this._configuration = configuration;
            DbContext = dbContext;
        }
        public Response GetUsers(Guid? UserId, string? FirstName, string? LastName, string? Email, long Phone, String OrderBy, int SortOrder, int RecordsPerPage, int PageNumber)          // sort order   ===   e1 for ascending   -1 for descending
        {
            var users = DbContext.Users.ToList();
            users = users.Where(t => t.IsDeleted == false).ToList();
            if (UserId != null) { users = users.Where(s => (s.UserId == UserId)).ToList(); }
            if (FirstName != null) { users = users.Where(s => (s.FirstName == FirstName)).ToList(); }
            if (FirstName != null) { users = users.Where(s => (s.FirstName == FirstName)).ToList(); }
            if (Email != null) { users = users.Where(s => (s.Email == Email)).ToList(); }
            if (Phone != -1) { users = users.Where(s => (s.Phone == Phone)).ToList(); }
            /*if (subjectId != null)
            {
                List<SubjectTeacherMappingInput> subjectTeacherId = (List<SubjectTeacherMappingInput>)DbContext.SubjectTeachersMappings.Where(st => st.SubjectId == subjectId).Select(st => st);
                //SubjectTeacherMappings temp = DbContext.SubjectTeachersMappings.Where(s => s.SubjectId.(subjectId));
                student = (DbSet<Student>)student.Where(s => s.SubjectTeacherAllocated.));
            }*/


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
                };
                res.Add(r);
            }

            response.StatusCode = 200;
            response.Message = "Users list fetched";
            response.Data = res;
            return response;
        }
    }
}
