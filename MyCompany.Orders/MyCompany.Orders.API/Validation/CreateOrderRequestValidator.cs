using FluentValidation;
using MyCompany.Orders.API.Models;

namespace MyCompany.Orders.API.Controllers // Ajuste le namespace selon l'endroit où tu la places
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.Total)
                .GreaterThan(0)
                .WithMessage("Le montant total de la commande doit être supérieur à 0 €.");
        }
    }
}