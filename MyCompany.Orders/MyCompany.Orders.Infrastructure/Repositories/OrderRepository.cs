using Microsoft.EntityFrameworkCore;
using MyCompany.Orders.Domain.Entities;
using MyCompany.Orders.Domain.Interfaces;
using MyCompany.Orders.Infrastructure.Data;

namespace MyCompany.Orders.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrdersDbContext _db;

        public OrderRepository(OrdersDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Order order)
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Order>> GetByUserAsync(Guid userId)
        {
            return await _db.Orders.Where(o => o.UserId == userId).ToListAsync();
        }
    }
}
