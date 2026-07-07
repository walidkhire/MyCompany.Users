using MyCompany.Orders.Application.DTOs;

namespace MyCompany.Orders.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateAsync(Guid userId, decimal total, string jwtToken);
        // 🔹 À rajouter pour l'historique :
        Task<IEnumerable<OrderDto>> GetByUserIdAsync(Guid userId);
    }
}
