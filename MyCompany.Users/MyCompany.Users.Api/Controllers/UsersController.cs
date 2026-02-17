using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCompany.Users.Application.DTOs;
using MyCompany.Users.Application.Interfaces;
using MyCompany.Users.Application.Services;
using MyCompany.Users.Domain.Entities;
using Users.API.Services;

namespace MyCompany.Users.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize] // Tous les endpoints nécessitent JWT sauf si on met [AllowAnonymous]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        // ✅ On injecte l'interface IUserService, pas la classe concrète
        public UsersController(IUserService userService)
        {
            _service = userService;
        }


        // ----------------------
        // 1️⃣ GET : Récupérer tous les utilisateurs
        // ----------------------
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<User>> Get()
        {
            var users = await _service.GetAllAsync();
            return (users);
        }

        // ----------------------
        // 3️⃣ POST : Créer un nouvel utilisateur
        // ----------------------
        [HttpPost]

        public async Task<IActionResult> Post([FromBody] CreateUserDto dto)
        {
            await _service.CreateAsync(dto.Name, dto.Email, dto.Password);
            return StatusCode(201);
        }



        private readonly RabbitMqPublisher _publisher = new RabbitMqPublisher();

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            // Ici on simule la création utilisateur
            await _service.CreateAsync(dto.Name, dto.Email, dto.Password);
            _publisher.PublishUserCreated(dto.Email);

            return Ok(new { message = $"Utilisateur créé : {dto.Email}" });
        }



        //🧩 9️⃣ CRUD COMPLET (Create / Read / Update / Delete)
        // ----------------------
        // 2️⃣ GET /{id} : Récupérer un utilisateur par Id
        // ----------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            await _service.UpdateAsync(id, dto.Name, dto.Email, dto.Password);
            return NoContent(); // 204
        }
    }
}
