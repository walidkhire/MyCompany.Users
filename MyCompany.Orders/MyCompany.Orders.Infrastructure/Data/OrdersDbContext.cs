using Microsoft.EntityFrameworkCore; // <--- INDISPENSABLE
using MyCompany.Orders.Domain.Entities;
using MyCompany.Orders.Domain.Enums;

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

            modelBuilder.Entity<Order>().Property(o => o.Total).HasColumnType("decimal(18,2)");

            // 🔹 Force EF Core à stocker le statut sous forme de STRING (ex: "Pending") en BDD
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v));
            // 🔹 Force EF Core à stocker le moyen de paiement sous forme de STRING (ex: "CreditCard")
            modelBuilder.Entity<Order>()
                .Property(o => o.PaymentMethod)
                .HasConversion(
                    v => v.ToString(),
                    v => (PaymentMethod)Enum.Parse(typeof(PaymentMethod), v));
        }
    }
}