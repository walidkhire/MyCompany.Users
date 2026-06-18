using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCompany.Orders.Application.Interfaces;
using System.ComponentModel.DataAnnotations;

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


      

        [HttpGet("GetOrder")]
        public IActionResult GetOrders()
        {
            return Ok(new[] { "Order1", "Order2" });
        }
        [HttpPost("AddOrder")] 
        public async Task<IActionResult> Create(Guid userId, decimal total, [FromServices]IValidator<Guid> validator)
        {
            // 🔹 On lance la validation explicitement
            var validationResult = await validator.ValidateAsync(userId);

            if (!validationResult.IsValid)
            {
                // Renvoie un 400 Bad Request avec le détail des erreurs
                return BadRequest(validationResult.ToDictionary());
            }


            // Récupérer le token JWT depuis l'Authorization header
            var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var order = await _service.CreateAsync(userId, total, jwtToken);
            return Ok(order);
        }
    }
}
