using MyCompany.Users.Frontend.Models;

namespace MyCompany.Users.Frontend.Services
{
    public interface IAuthService
    {
        Task<bool> AuthenticateAsync(string email, string password);

        Task LogoutAsync();
        Task<User?> GetCurrentUserAsync();
    }
}
