namespace MyCompany.Orders.Application.DTOs
{
    public record OrderDto(Guid Id, Guid UserId, decimal Total);
}
