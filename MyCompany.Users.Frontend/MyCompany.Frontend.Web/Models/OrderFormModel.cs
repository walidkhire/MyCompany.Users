using System.ComponentModel.DataAnnotations;

namespace MyCompany.Frontend.Web.Models
{
    public class OrderFormModel
    {
        [Required(ErrorMessage = "Le montant de la commande est obligatoire.")]
        [Range(0.01, 100000.00, ErrorMessage = "Le montant doit être supérieur à 0 € et ne peut pas dépasser 100 000 €.")]
        public decimal Total { get; set; } = 120.50m; // Valeur par défaut
    }
}