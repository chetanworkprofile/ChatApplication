using ChatApplication.Models;

namespace ChatApplication.Services
{
    public interface IUserService
    {
        public object GetUsers(string email, string token, Guid? UserId, string? searchString, string? Email, long Phone, String OrderBy, int SortOrder, int RecordsPerPage, int PageNumber);
        public Task<object> UpdateUser(string email, UpdateUser u,string token);
        public Task<object> DeleteUser(string email,string token,string password);
    }
}
