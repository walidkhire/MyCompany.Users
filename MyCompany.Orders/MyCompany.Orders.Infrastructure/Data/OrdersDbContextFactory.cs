using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyCompany.Orders.Infrastructure.Data
{
    public class OrdersDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
    {
        public OrdersDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrdersDbContext>();

            // Chaîne de connexion locale dédiée uniquement au design-time (génération des migrations)
            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=OrdersDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            optionsBuilder.UseSqlServer(connectionString);

            return new OrdersDbContext(optionsBuilder.Options);
        }
    }
}