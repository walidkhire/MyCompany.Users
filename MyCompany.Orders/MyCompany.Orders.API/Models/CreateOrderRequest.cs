namespace MyCompany.Orders.API.Models // Utilise le même namespace que ton contrôleur
{
    public record CreateOrderRequest(decimal Total);
}