using MassTransit; // 🔹 AJOUTÉ : Pour reconnaître les extensions d'Outbox MassTransit
using Microsoft.EntityFrameworkCore;
using MyCompany.Users.Domain.Entities;

namespace MyCompany.Users.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();

        // 🔹 CORRIGÉ : ModelBuilder au lieu de ModelCreatingBuilder
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des tables de l'Outbox MassTransit dans votre base SQL Server
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}