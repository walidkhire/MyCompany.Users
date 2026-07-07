using MyCompany.Orders.Domain.Enums;
using System;

namespace MyCompany.Orders.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }

        // 🔹 NOUVELLES COLONNES À AJOUTER :
        public DateTime LastUpdatedAt { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending; // Valeur par défaut
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Unknown;
        public string ShippingAddress { get; set; } = "123 Rue de la République, Paris";
    }
}