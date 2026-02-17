using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCompany.Orders.Application.Interfaces;

namespace MyCompany.Orders.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }


        [AllowAnonymous] 

        [HttpGet]
        public IActionResult GetOrders()
        {
            return Ok(new[] { "Order1", "Order2" });
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create(Guid userId, decimal total)
        {
            // Récupérer le token JWT depuis l'Authorization header
            var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var order = await _service.CreateAsync(userId, total, jwtToken);
            return Ok(order);
        }
    }
}
