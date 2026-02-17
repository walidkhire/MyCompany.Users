using MyCompany.Orders.Application.DTOs;
using MyCompany.Orders.Application.Interfaces;
using MyCompany.Orders.Domain.Entities;
using MyCompany.Orders.Domain.Exceptions; 
using MyCompany.Orders.Domain.Interfaces;
using MyCompany.Orders.Infrastructure.HttpClients;
using MyCompany.Orders.Infrastructure.Repositories;

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

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Total = total,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(order);

            return new OrderDto(order.Id, order.UserId, order.Total);
        }

        
    }
}
