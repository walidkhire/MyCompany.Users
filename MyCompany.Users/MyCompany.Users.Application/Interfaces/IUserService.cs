using MyCompany.Users.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompany.Users.Application.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task CreateAsync(string name, string email, string password);
        Task UpdateAsync(Guid id, string name, string email, string? password = null);
        Task DeleteAsync(Guid id);
    }
}