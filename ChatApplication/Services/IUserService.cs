using ChatApplication.Models;

namespace ChatApplication.Services
{
    public interface IUserService
    {
        public object GetUsers(Guid? UserId, string? FirstName, string? LastName, string? Email, long Phone, String OrderBy, int SortOrder, int RecordsPerPage, int PageNumber);
    }
}
