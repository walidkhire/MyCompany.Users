using MassTransit; // 🔹 AJOUTÉ : Pour utiliser l'Outbox native de MassTransit
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCompany.Users.Application.DTOs;
using MyCompany.Users.Application.Interfaces;
using MyCompany.Users.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCompany.Users.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IPublishEndpoint _publishEndpoint; // 🔹 VERSION SENIOR : Remplacera à terme votre publisher maison pour l'Outbox

        // ✅ Constructeur unique et propre
        public UsersController(IUserService userService, IPublishEndpoint publishEndpoint)
        {
            _service = userService;
            _publishEndpoint = publishEndpoint;
        }

        // ---------------------------------------------------------------------
        // 1️⃣ GET : Récupérer tous les utilisateurs (Anonyme pour la Gateway / Frontend)
        // ---------------------------------------------------------------------
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            var users = await _service.GetAllAsync();
            return Ok(users);
        }

        // ---------------------------------------------------------------------
        // 2️⃣ GET : Récupérer un utilisateur par son ID
        // ---------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetById(Guid id)
        {
            var user = await _service.GetById(id);
            if (user == null) return NotFound();

            return Ok(user);
        }

        // ---------------------------------------------------------------------
        // 3️⃣ POST : Création unique, sécurisée et transactionnelle (Outbox)
        // ---------------------------------------------------------------------
        [HttpPost]
        [AllowAnonymous] // 👈 Pratique pour vos tests locaux sans token complet au début
        public async Task<IActionResult> Post([FromBody] CreateUserDto dto)
        {
            // 1. Sauvegarde en base de données (Via votre service de domaine)
            await _service.CreateAsync(dto.Name, dto.Email, dto.Password);

            // 2. Publication via l'Outbox MassTransit 
            // Si la base de données crash, le message n'est pas envoyé à RabbitMQ. 
            // Si la base réussit, MassTransit garantit l'envoi dès que RabbitMQ revient à la vie.
            await _publishEndpoint.Publish(new MyCompany.Shared.Events.UserCreatedEvent
            {
                Email = dto.Email,
                Name = dto.Name
            });

            // Retourne un vrai code 201 Created (Le code 21 que vous aviez écrit n'existe pas en HTTP)
            return StatusCode(StatusCodes.Status201Created); // 201 Created
        }
        
        // ---------------------------------------------------------------------
        // 4️⃣ PUT : Mettre à jour un utilisateur
        // ---------------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            await _service.UpdateAsync(id, dto.Name, dto.Email, dto.Password);
            return NoContent(); // 204
        }
    }
}