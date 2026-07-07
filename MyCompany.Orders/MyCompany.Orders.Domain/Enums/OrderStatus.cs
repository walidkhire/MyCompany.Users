namespace MyCompany.Orders.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,    // En attente (statut initial à la création)
        Validated = 1,  // Validée (paiement OK, stock OK)
        Rejected = 2    // Rejetée / Annulée (échec de validation, manque de stock, etc.)
    }
}