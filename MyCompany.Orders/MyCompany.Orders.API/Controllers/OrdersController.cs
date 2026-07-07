using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCompany.Orders.Application.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MyCompany.Orders.API.Models;

namespace MyCompany.Orders.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/orders")] // 🔹 RECORRIGÉ ICI : Pour matcher le PathPattern de YARP !
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;
        private readonly IValidator<CreateOrderRequest> _validator;

        public OrdersController(IOrderService service, IValidator<CreateOrderRequest> validator)
        {
            _service = service;
            _validator = validator;
        }
        [HttpGet] // GET https://localhost:7259/api/orders
        public async Task<IActionResult> GetOrders()
        {
            // 1. Extraction sécurisée du UserId depuis les Claims du Token JWT passé par YARP
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Jeton utilisateur invalide ou manquant.");
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest("L'identifiant utilisateur est corrompu.");
            }

            // 2. 🔥 APPEL RÉEL : On va chercher les vrais éléments typés en BDD pour cet utilisateur
            var orders = await _service.GetByUserIdAsync(userId);

            // 3. Renvoi des données au Frontend avec un statut HTTP 200 OK
            return Ok(orders);
        }

        [HttpPost] // Répondra à : POST https://localhost:7259/api/orders
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            Guid userId = Guid.Parse(userIdClaim);
            string jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var order = await _service.CreateAsync(userId, request.Total, jwtToken);

            return Ok(order);
        }
    }
}