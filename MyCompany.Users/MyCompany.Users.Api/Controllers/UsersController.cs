using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCompany.Users.Application.DTOs;
using MyCompany.Users.Application.Interfaces;
using MyCompany.Users.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Users.API.Services;

namespace MyCompany.Users.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly RabbitMqPublisher _publisher; // ✅ Déclaré proprement en haut

        // ✅ Injectez TOUS vos services requis dans le constructeur unique
        public UsersController(IUserService userService, RabbitMqPublisher publisher)
        {
            _service = userService;
            _publisher = publisher;
        }

        // ----------------------
        // 1️⃣ GET : Récupérer tous les utilisateurs
        // ----------------------
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<User>> Get()
        {
            var users = await _service.GetAllAsync();
            return users;
        }


        // ----------------------
        // 1️⃣ GET : Récupérer tous les utilisateurs
        // ----------------------
        [HttpGet("{id}")]
        public async Task<User> GetById(Guid Id)
        {
            var users = await _service.GetById(Id);
            return users;
        }

        // ----------------------
        // 2️⃣ POST : Créer un nouvel utilisateur (Standard)
        // ----------------------
        [HttpPost]
        [AllowAnonymous] // 👈 AJOUTEZ CETTE LIGNE ICI temporairement pour créer vos tests
        public async Task<IActionResult> Post([FromBody] CreateUserDto dto)
        {
            await _service.CreateAsync(dto.Name, dto.Email, dto.Password);
            return StatusCode(201);
        }

        // ----------------------
        // 3️⃣ POST : Créer avec notification RabbitMQ
        // ----------------------
        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            await _service.CreateAsync(dto.Name, dto.Email, dto.Password);

            // ✅ Ajout du 'await' pour corriger le warning CS4014
            await _publisher.PublishUserCreated(dto.Email);

            return Ok(new { message = $"Utilisateur créé : {dto.Email}" });
        }

        // ----------------------
        // 4️⃣ PUT : Mettre à jour un utilisateur
        // ----------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            await _service.UpdateAsync(id, dto.Name, dto.Email, dto.Password);
            return NoContent(); // 204
        }
    }
}