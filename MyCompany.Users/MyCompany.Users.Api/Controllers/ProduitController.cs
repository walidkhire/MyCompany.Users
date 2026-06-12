using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyCompany.Users.API.Controllers
{
    [Route("api/produits")]
    [ApiController]
    [Authorize]
    public class ProduitController : ControllerBase
    {

        [HttpGet("Produit")]
        public IActionResult Get()
        {
            return Ok(new { Message = "Hello from ProduitController!" });
        }
        [HttpPost]
        public IActionResult Post()
        {
            return Ok(new { Message = "Produit created successfully!" });
        }
    }
}
