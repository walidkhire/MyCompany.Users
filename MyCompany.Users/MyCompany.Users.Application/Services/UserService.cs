using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MyCompany.Shared.Events;
using MyCompany.Users.Application.Interfaces;
using MyCompany.Users.Domain.Entities;
using MyCompany.Users.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCompany.Users.Application.Services
{
    // 🧩 3. Couche APPLICATION (Logique métier / applicative)
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

        // ---------------------------------------------------------------------
        // 1️⃣ Enregistrement et Notification de création
        // ---------------------------------------------------------------------
        public async Task CreateAsync(string name, string email, string password)
        {
            var user = new User(name, email, password);
            await _repository.AddAsync(user);

            // 🔥 Publication MassTransit (Plus de blocage !)
            await _publish.Publish(new UserCreatedEvent
            {
                UserId = user.Id,
                Email = user.Email
            });

            _logger.LogInformation("Utilisateur {Email} créé et notifié via MassTransit.", email);
        }

        // ---------------------------------------------------------------------
        // 2️⃣ Mise à jour d'un utilisateur
        // ---------------------------------------------------------------------
        public async Task UpdateAsync(Guid id, string name, string email, string password)
        {
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                throw new KeyNotFoundException("Utilisateur introuvable");

            // Si le mot de passe fourni est vide, on garde l'ancien mot de passe
            var passwordToUpdate = string.IsNullOrWhiteSpace(password) ? user.Password : password;

            user.Update(name, email, passwordToUpdate);
            await _repository.UpdateAsync(user);

            // On nettoie le cache après modification pour éviter des données obsolètes
            _cache.Remove(email);
        }

        // ---------------------------------------------------------------------
        // 3️⃣ Lecture globale (sans cache pour garantir la fraîcheur)
        // ---------------------------------------------------------------------
        public Task<IEnumerable<User>> GetAllAsync() => _repository.GetAllAsync();

        // ---------------------------------------------------------------------
        // 4️⃣ Suppression d'un utilisateur
        // ---------------------------------------------------------------------
        public async Task DeleteAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                throw new KeyNotFoundException("Utilisateur introuvable");

            await _repository.DeleteAsync(user);
            _cache.Remove(user.Email);
        }



        public async Task<User> GetById(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);


            if (user != null)
            {
                _cache.Get(user.Email);
                return user;
            }
            else
                return null;

        }


        // ---------------------------------------------------------------------
        // 5️⃣ Validation des identifiants (Login)
        // ---------------------------------------------------------------------
        public async Task<User?> ValidateUserAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Validation échouée pour {Email} : utilisateur non trouvé", email);
                return null;
            }

            // TODO : implémenter BCrypt ou PasswordHasher à l'avenir pour le mot de passe hashé
            if (user.Password != password)
            {
                _logger.LogWarning("Validation échouée pour {Email} : mot de passe incorrect", email);
                return null;
            }

            _logger.LogInformation("Utilisateur {Email} validé avec succès", email);
            return user;
        }

        // ---------------------------------------------------------------------
        // 6️⃣ Lecture unitaire (Optimisée par Cache RAM)
        // ---------------------------------------------------------------------
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            // 1. On cherche dans la RAM
            if (_cache.TryGetValue(email, out User? cachedUser))
                return cachedUser;

            // 2. Si absent, on va en Base de données
            var user = await _repository.GetByEmailAsync(email);

            if (user != null)
            {
                // 3. On configure les règles de sécurité de la RAM
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)) // Durée de vie max = 5 min
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2)); // Supprime si inactif pendant 2 min
                
                _cache.Set(email, user, cacheOptions);
            }

            return user;
        }
    }
}