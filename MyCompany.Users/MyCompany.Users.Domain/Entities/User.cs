using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompany.Users.Domain.Entities
{

    //🧠 2. Couche DOMAIN (cœur métier)
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }

        // 🔑 Propriété mot de passe (hashé en production)
        public string Password { get; set; } = null!;

        private User() { }


        public User(string name, string email,string password)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            Password = BCrypt.Net.BCrypt.HashPassword(password); // hash du mot de passe
        }

        public void Update(string name, string email,string password)
        {
            Name = name;
            Email = email;
            if (!string.IsNullOrEmpty(password))
                Password = BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
