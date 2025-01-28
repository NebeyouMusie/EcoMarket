using Microsoft.AspNetCore.Mvc;
using EcoMarket.Models;
using EcoMarket.Services;
using Microsoft.AspNetCore.Authorization;

namespace EcoMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly FavoriteService _favoriteService;

        public FavoritesController(FavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var favorites = await _favoriteService.GetAllFavorites();
            return Ok(favorites);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFavorite(string id)
        {
            var favorite = await _favoriteService.GetFavoriteById(id);
            if (favorite == null)
                return NotFound();
            return Ok(favorite);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserFavorites(string userId)
        {
            var favorites = await _favoriteService.GetUserFavorites(userId);
            return Ok(favorites);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFavorite([FromBody] Favorite favorite)
        {
            Console.WriteLine($"Received Favorite Request: UserId = {favorite.UserId}, ProductId = {favorite.ProductId}");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Validation failed", errors });
            }


            var authenticatedUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"Authenticated User ID: {authenticatedUserId}");

            if (authenticatedUserId == null)
            {
                return Unauthorized();
            }
            if (favorite.UserId != authenticatedUserId)
            {
                return Unauthorized("You can only add favorites for your own user ID");
            }

            try
            {
                var (createdFavorite, errorMessage) = await _favoriteService.CreateFavorite(favorite);

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.WriteLine($"Favorite Creation Error: {errorMessage}");
                    return BadRequest(new { message = errorMessage });
                }

                return CreatedAtAction(nameof(GetFavorite), new { id = createdFavorite.Id }, createdFavorite);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavorite(string id)
        {
            var success = await _favoriteService.DeleteFavorite(id);
            if (!success)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("user/{userId}/product/{productId}")]
        public async Task<IActionResult> DeleteFavoriteByUserAndProduct(string userId, string productId)
        {
            var success = await _favoriteService.DeleteFavoriteByUserAndProduct(userId, productId);
            if (!success)
                return NotFound();
            return NoContent();
        }
    }
}