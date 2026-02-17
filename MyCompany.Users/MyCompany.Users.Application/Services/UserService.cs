using MassTransit;
using MassTransit.Transports;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MyCompany.Shared.Events;
using MyCompany.Users.Application.Interfaces;
using MyCompany.Users.Domain.Entities;
using MyCompany.Users.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MyCompany.Users.Application.Services
{
    //🧩 3. Couche APPLICATION (logique applicative) (logique métier)

    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        private readonly ILogger<UserService> _logger;

        private readonly IMemoryCache _cache;

        private readonly IPublishEndpoint _publish;


        public UserService(IUserRepository repository, ILogger<UserService> logger, IMemoryCache cache, IPublishEndpoint publish)
        {
            _repository = repository;
            _logger = logger;
            _cache = cache;
            _publish = publish;
        }


        public async Task CreateAsync(string name, string email, string password)
        {
            var user = new User(name, email, password);
            await _repository.AddAsync(user);
            // 🔥 EVENT
            await _publish.Publish(new UserCreatedEvent
            {
                UserId = user.Id,
                Email = user.Email
            });
        }
        public async Task CreateUserAsync(Guid userId, string email)
        {
            await _publish.Publish(new UserCreatedEvent
            {
                UserId = userId,
                Email = email
            });
        }


        public async Task UpdateAsync(Guid id, string name, string email, string password)
        {
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                throw new Exception("Utilisateur introuvable");

            user.Update(name, email, password);

            await _repository.UpdateAsync(user);
        }

        public Task<IEnumerable<User>> GetAllAsync()
        => _repository.GetAllAsync();


        public async Task DeleteAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                throw new Exception("Utilisateur introuvable");

            await _repository.DeleteAsync(user);
        }


        // ========================
        // Méthode de validation pour login
        // ========================
        public async Task<User?> ValidateUserAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Validation échouée pour {Email} : utilisateur non trouvé", email);
                return null;
            }

            // TODO: vérifier le mot de passe hashé
            if (user.Password != password)
            {
                _logger.LogWarning("Validation échouée pour {Email} : mot de passe incorrect", email);
                return null;
            }

            _logger.LogInformation("Utilisateur {Email} validé avec succès", email);
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            if (_cache.TryGetValue(email, out User cachedUser))
                return cachedUser;

            var user = await _repository.GetByEmailAsync(email);

            if (user != null)
                _cache.Set(email, user, TimeSpan.FromMinutes(5));

            return user;
        }


    }
}
