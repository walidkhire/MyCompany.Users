using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompany.Shared.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
