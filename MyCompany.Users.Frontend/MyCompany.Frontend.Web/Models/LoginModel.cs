using System.ComponentModel.DataAnnotations;

namespace MyCompany.Frontend.Web.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "L'adresse email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Le format de l'email est invalide.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        public string Password { get; set; } = string.Empty;
    }
}