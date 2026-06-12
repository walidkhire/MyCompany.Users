using MyCompany.Users.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCompany.Users.Application.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task CreateAsync(string name, string email, string password);
        // Aligné pour accepter le password optionnel ou obligatoire selon vos besoins
        Task UpdateAsync(Guid id, string name, string email, string password);
        Task DeleteAsync(Guid id);
        Task<User> GetById(Guid id);
        // Ajout de la méthode de validation pour qu'elle soit accessible depuis le AuthController
        Task<User?> ValidateUserAsync(string email, string password);
    }
}