using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using EcoMarket.Models;
using EcoMarket.Services;
using BC = BCrypt.Net.BCrypt;

namespace EcoMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;
        private readonly JwtService _jwtService;

        public AuthController(MongoDBService mongoDBService, JwtService jwtService)
        {
            _mongoDBService = mongoDBService;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
                return BadRequest("Email and password are required");

            var existingUser = await _mongoDBService.Users
                .Find(u => u.Email == user.Email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
                return BadRequest("User with this email already exists");

            user.Password = BC.HashPassword(user.Password);
            user.Role ??= "user"; // Default role if not specified
            user.CreatedAt = DateTime.UtcNow;
            user.LastLoginAt = DateTime.UtcNow;

            await _mongoDBService.Users.InsertOneAsync(user);

            var token = _jwtService.GenerateToken(user);

            return Ok(new { 
                Message = "Registration successful. Use this token for authenticated requests.",
                Token = token, 
                User = new {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role,
                    user.Phone,
                    user.Address,
                    user.CreatedAt,
                    user.LastLoginAt
                }
            });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Email and password are required");

            var user = await _mongoDBService.Users
                .Find(u => u.Email == request.Email)
                .FirstOrDefaultAsync();

            if (user == null)
                return Unauthorized("Invalid email or password");

            if (!BC.Verify(request.Password, user.Password))
                return Unauthorized("Invalid email or password");

            var update = Builders<User>.Update.Set(u => u.LastLoginAt, DateTime.UtcNow);
            await _mongoDBService.Users.UpdateOneAsync(u => u.Id == user.Id, update);
            user.LastLoginAt = DateTime.UtcNow;

            var token = _jwtService.GenerateToken(user);

            return Ok(new { 
                Message = "Login successful. Use this token for authenticated requests.",
                Token = token, 
                User = new {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role,
                    user.Phone,
                    user.Address,
                    user.CreatedAt,
                    user.LastLoginAt
                }
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await _mongoDBService.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                return NotFound();

            return Ok(new {
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.Phone,
                user.Address,
                user.CreatedAt,
                user.LastLoginAt
            });
        }
    }

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
