using MyCompany.Users.Frontend.Models;

namespace MyCompany.Users.Frontend.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetUsersAsync();
    }
}
