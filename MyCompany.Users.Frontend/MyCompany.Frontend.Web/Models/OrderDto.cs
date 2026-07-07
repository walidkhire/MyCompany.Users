using System;

namespace MyCompany.Frontend.Web.Models
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }

        // 🔹 Nouveaux champs ajoutés :
        public DateTime LastUpdatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
    }
}