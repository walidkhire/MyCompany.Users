using MyCompany.Orders.Application.DTOs;
using MyCompany.Orders.Application.Interfaces;
using MyCompany.Orders.Domain.Entities;
using MyCompany.Orders.Domain.Enums;
using MyCompany.Orders.Domain.Exceptions;
using MyCompany.Orders.Domain.Interfaces;
using MyCompany.Orders.Infrastructure.HttpClients; 

namespace MyCompany.Orders.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;
        private readonly IUsersClient _usersClient;

        public OrderService(IOrderRepository repo, IUsersClient usersClient)
        {
            _repo = repo;
            _usersClient = usersClient;
        }

        public async Task<OrderDto> CreateAsync(Guid userId, decimal total, string jwtToken)
        {
            // 🔐 Vérifier que l’utilisateur existe dans Users.API
            var userExists = await _usersClient.UserExistsAsync(userId, jwtToken);
            if (!userExists)
                throw new BadRequestException("Utilisateur invalide ou non autorisé");

            // 1. Définition des valeurs par défaut pour la nouvelle commande
            var currentStatus = OrderStatus.Pending;
            var paymentMethod = PaymentMethod.CreditCard; // Valeur par défaut simulée
            var shippingAddress = "Adresse par défaut de l'utilisateur"; // À récupérer plus tard via le profil utilisateur
            var now = DateTime.UtcNow;

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Total = total,
                CreatedAt = now,
                // 💡 Note : Si tu as déjà ajouté ces propriétés dans ton entité Order, décommente les lignes suivantes :
                Status = currentStatus,
                PaymentMethod = paymentMethod,
                ShippingAddress = shippingAddress,
                LastUpdatedAt = now
            };

            await _repo.AddAsync(order);

            // 🔹 CORRECTION : On passe TOUS les paramètres attendus par le record OrderDto
            return new OrderDto(
                order.Id,
                order.UserId,
                order.Total,
                now,             // LastUpdatedAt
                currentStatus.ToString(),   // Status
                paymentMethod.ToString(),   // PaymentMethod
                shippingAddress  // ShippingAddress
            );
        }


        public async Task<IEnumerable<OrderDto>> GetByUserIdAsync(Guid userId)
        {
            // 1. Récupération des entités du domaine depuis la base de données via le Repo
            var domainOrders = await _repo.GetByUserIdAsync(userId);

            // 2. Mapping propre des entités vers le Record de sortie OrderDto
            var dtos = domainOrders.Select(order => new OrderDto(
                order.Id,
                order.UserId,
                order.Total,
                order.LastUpdatedAt,  // Assure-toi que ces champs existent sur ton entité Order
                order.Status.ToString(), // 👈 Envoie "Pending", "Validated" ou "Rejected"
                order.PaymentMethod.ToString(),
                order.ShippingAddress
            ));

            return dtos;
        }
    }
}