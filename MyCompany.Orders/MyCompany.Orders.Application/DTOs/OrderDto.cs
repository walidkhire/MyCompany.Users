namespace MyCompany.Orders.Application.DTOs
{
    public record OrderDto(Guid Id, Guid UserId, decimal Total, DateTime LastUpdatedAt, string Status, string PaymentMethod, string ShippingAddress);
}

