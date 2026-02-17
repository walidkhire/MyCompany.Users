using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompany.Orders.Infrastructure.HttpClients
{
    public interface IUsersClient
    {
        Task<bool> UserExistsAsync(Guid userId, string jwtToken);
    }
}
