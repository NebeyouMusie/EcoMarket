using Microsoft.AspNetCore.Mvc;
using EcoMarket.Models;
using EcoMarket.Services;
using Microsoft.AspNetCore.Authorization;

namespace EcoMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _orderService.GetAllOrders();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(string id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null)
                return NotFound();
            return Ok(order);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(string userId)
        {
            var orders = await _orderService.GetUserOrders(userId);
            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {

            var (createdOrder, errorMessage) = await _orderService.CreateOrder(order);
            if (!string.IsNullOrEmpty(errorMessage))
            {

                Console.WriteLine($"Order Creation Error: {errorMessage}");
                return BadRequest(new { message = errorMessage });
            }

            return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] string status)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            bool isAdmin = User.IsInRole("admin");

            var (success, errorMessage) = await _orderService.UpdateOrderStatus(id, status, userId, isAdmin);

            if (!success)
            {
                if (errorMessage == "Order not found")
                    return NotFound(new { message = errorMessage });

                if (errorMessage == "Not authorized to update this order's status")
                    return Forbid();

                return BadRequest(new { message = errorMessage });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            bool isAdmin = User.IsInRole("admin");

            var (success, errorMessage) = await _orderService.DeleteOrder(id, userId, isAdmin);

            if (!success)
            {
                if (errorMessage == "Order not found")
                    return NotFound(new { message = errorMessage });

                if (errorMessage == "Not authorized to delete this order" ||
                    errorMessage == "Only pending orders can be deleted")
                    return Forbid();

                return BadRequest(new { message = errorMessage });
            }

            return NoContent();
        }
    }
}