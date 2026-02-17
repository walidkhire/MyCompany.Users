namespace MyCompany.Users.Frontend.Models
{
    public class User
    {
        public string? Email { get; set; }
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
