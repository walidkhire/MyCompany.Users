using Microsoft.EntityFrameworkCore; // <--- INDISPENSABLE
using MyCompany.Orders.Domain.Entities;

namespace MyCompany.Orders.Infrastructure.Data
{
    public class OrdersDbContext : DbContext
    {
        public DbSet<Order> Orders => Set<Order>();

        public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Maintenant, HasColumnType sera reconnu
            modelBuilder.Entity<Order>()
                .Property(o => o.Total)
                .HasColumnType("decimal(18,2)");
        }
    }
}