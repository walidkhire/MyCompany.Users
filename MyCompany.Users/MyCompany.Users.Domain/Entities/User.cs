using System;

namespace MyCompany.Users.Domain.Entities
{
    // 🧠 Couche DOMAIN (cœur métier - Pure et sans dépendance externe BCrypt)
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; } // 🔹 Repassé en private set pour encapsulation

        // Constructeur pour Entity Framework / Dapper
        private User() { }

        // Constructeur Métier
        public User(string name, string email, string hashedPassword)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            Password = hashedPassword; // 🔹 Simple affectation ! On ne re-hache pas ici.
        }

        public void Update(string name, string email, string hashedPassword)
        {
            Name = name;
            Email = email;
            if (!string.IsNullOrEmpty(hashedPassword))
                Password = hashedPassword; // 🔹 Simple affectation !
        }
    }
}