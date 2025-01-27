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
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly ReviewService _reviewService;

        public ReviewsController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> GetReviews()
        {
            var reviews = await _reviewService.GetAllReviews();
            return Ok(reviews);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReview(string id)
        {
            var review = await _reviewService.GetReviewById(id);
            if (review == null)
                return NotFound();
            return Ok(review);
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(string productId, [FromQuery] PaginationParams paginationParams)
        {
            var response = await _reviewService.GetProductReviews(productId, paginationParams);
            return Ok(response);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserReviews()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var reviews = await _reviewService.GetUserReviews(userId);
            return Ok(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] Review review)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Validation failed", errors });
            }

            // Log the incoming review details
            // Console.WriteLine($"Received Review Request: {System.Text.Json.JsonSerializer.Serialize(review)}");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var (createdReview, errorMessage) = await _reviewService.CreateReview(review, userId);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                // Console.WriteLine($"Review Creation Error: {errorMessage}");
                return BadRequest(new { message = errorMessage });
            }

            return CreatedAtAction(nameof(GetReview), new { id = createdReview.Id }, createdReview);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(string id, [FromBody] Review review)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var (success, errorMessage) = await _reviewService.UpdateReview(id, review, userId);
            if (!success)
                return errorMessage == "Review not found" ? NotFound(errorMessage) : BadRequest(errorMessage);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var (success, errorMessage) = await _reviewService.DeleteReview(id, userId);
            if (!success)
                return errorMessage == "Review not found" ? NotFound(errorMessage) : BadRequest(errorMessage);

            return NoContent();
        }
    }
}
