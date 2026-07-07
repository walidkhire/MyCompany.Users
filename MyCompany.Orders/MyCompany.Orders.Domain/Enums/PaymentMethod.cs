namespace MyCompany.Orders.Domain.Enums
{
    public enum PaymentMethod
    {
        Unknown = 0,       // Non spécifié / En attente de choix
        CreditCard = 1,    // Carte Bancaire (Visa, Mastercard, etc.)
        PayPal = 2,        // PayPal
        BankTransfer = 3,  // Virement bancaire
        ApplePay = 4       // Apple Pay / Google Pay
    }
}