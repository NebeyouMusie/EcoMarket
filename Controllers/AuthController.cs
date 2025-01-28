using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcoMarket.Models;
using EcoMarket.Services;

namespace EcoMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var (registeredUser, token, message, success) = await _authService.Register(user);

            if (!success)
                return BadRequest(message);

            return Ok(new { 
                Message = message,
                Token = token, 
                User = new {
                    registeredUser.Id,
                    registeredUser.Name,
                    registeredUser.Email,
                    registeredUser.Role,
                    registeredUser.Phone,
                    registeredUser.Address,
                    registeredUser.CreatedAt,
                    registeredUser.LastLoginAt
                }
            });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (user, token, message, success) = await _authService.Login(request.Email, request.Password);

            if (!success)
                return Unauthorized(message);

            return Ok(new { 
                Message = message,
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

            var user = await _authService.GetUserById(userId);
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
