using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcoMarket.Models;
using EcoMarket.Services;
using System.Security.Claims;

namespace EcoMarket.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] PaginationParams paginationParams)
        {
            var response = await _productService.GetProducts(paginationParams);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string query, [FromQuery] PaginationParams paginationParams)
        {
            try
            {
                var response = await _productService.SearchProducts(query, paginationParams);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetProductsByCategory(string category, [FromQuery] PaginationParams paginationParams)
        {
            var response = await _productService.GetProductsByCategory(category, paginationParams);
            return Ok(response);
        }

        [HttpGet("eco-friendly")]
        public async Task<IActionResult> GetEcoFriendlyProducts([FromQuery] PaginationParams paginationParams)
        {
            var response = await _productService.GetEcoFriendlyProducts(paginationParams);
            return Ok(response);
        }

        [Authorize(Roles = "admin,seller")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            // Ensure the seller can only set their own sellerId
            if (User.IsInRole("seller") && product.SellerId != userId)
                return Forbid();

            var createdProduct = await _productService.CreateProduct(product);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        [Authorize(Roles = "admin,seller")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product product)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            // Check if the product exists and belongs to the seller
            var existingProduct = await _productService.GetProductById(id);
            if (existingProduct == null)
                return NotFound();

            // Debug logging
            Console.WriteLine($"Current User ID: {userId}");
            Console.WriteLine($"Current User Roles: {string.Join(", ", User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");
            Console.WriteLine($"Existing Product Seller ID: {existingProduct.SellerId}");

            // Ensure sellers can only update their own products
            if (User.IsInRole("seller") && existingProduct.SellerId != userId)
            {
                Console.WriteLine($"Update forbidden. Seller {userId} trying to update product owned by {existingProduct.SellerId}");
                return Forbid();
            }

            // Use the route ID for updating
            product.Id = id;

            var updatedProduct = await _productService.UpdateProduct(id, product);
            return Ok(updatedProduct);
        }

        [Authorize(Roles = "admin,seller")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            // Check if the product exists and belongs to the seller
            var existingProduct = await _productService.GetProductById(id);
            if (existingProduct == null)
                return NotFound();

            // Ensure sellers can only delete their own products
            if (User.IsInRole("seller") && existingProduct.SellerId != userId)
                return Forbid();

            await _productService.DeleteProduct(id);
            return NoContent();
        }
    }
}
