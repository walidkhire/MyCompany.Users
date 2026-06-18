using FluentValidation;
using MyCompany.Orders.Application.DTOs; // Ajustez selon votre namespace

namespace MyCompany.Orders.Application.Validators
{
    public class CreateOrderValidator: AbstractValidator<Guid> // Ici on valide le userId passé au contrôleur par exemple
    {
        public CreateOrderValidator()
        {
            RuleFor(userid=>userid)
                .NotEmpty().WithMessage("L'identifiant de l'utilisateur est obligatoire.");
        }
    }
}
