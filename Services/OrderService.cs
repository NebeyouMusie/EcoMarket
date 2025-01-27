using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using EcoMarket.Models;

namespace EcoMarket.Services
{
    public class OrderService
    {
        private readonly MongoDBService _mongoDBService;

        public OrderService(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        public async Task<List<Order>> GetAllOrders()
        {
            return await _mongoDBService.Orders.Find(Builders<Order>.Filter.Empty).ToListAsync();
        }

        public async Task<Order> GetOrderById(string id)
        {
            return await _mongoDBService.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Order>> GetUserOrders(string userId)
        {
            return await _mongoDBService.Orders.Find(o => o.UserId == userId).ToListAsync();
        }

        public async Task<(Order Order, string? ErrorMessage)> CreateOrder(Order? order)
        {
            if (order == null)
                return (new Order 
                { 
                    UserId = string.Empty, 
                    Items = new List<OrderItem>(), 
                    ShippingAddress = new ShippingAddress 
                    { 
                        Street = string.Empty, 
                        City = string.Empty, 
                        State = string.Empty, 
                        ZipCode = string.Empty, 
                        Country = string.Empty 
                    } 
                }, "Order cannot be null");

            if (string.IsNullOrWhiteSpace(order.UserId))
                return (order, "User ID is required");

            var user = await _mongoDBService.Users.Find(u => u.Id == order.UserId).FirstOrDefaultAsync();
            if (user == null)
            {
                Console.WriteLine($"User not found: {order.UserId}");
                return (order, $"User with ID {order.UserId} not found");
            }

            if (order.ShippingAddress == null)
                return (order, "Shipping address is required");

            if (order.Items == null || order.Items.Count == 0)
                return (order, "Order must contain at least one item");

            decimal totalAmount = 0;
            foreach (var item in order.Items)
            {
                if (string.IsNullOrWhiteSpace(item.ProductId))
                    return (order, "Product ID is required for each order item");

                if (item.Quantity <= 0)
                    return (order, $"Invalid quantity for product {item.ProductId}. Quantity must be greater than 0");

                var product = await _mongoDBService.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
                if (product == null)
                {
                    Console.WriteLine($"Product not found: {item.ProductId}");
                    return (order, $"Product with ID {item.ProductId} not found");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    Console.WriteLine($"Insufficient stock. Product: {item.ProductId}, Available: {product.StockQuantity}, Requested: {item.Quantity}");
                    return (order, $"Insufficient stock for product {item.ProductId}. Available: {product.StockQuantity}, Requested: {item.Quantity}");
                }

                totalAmount += product.Price * item.Quantity;
            }

            order.TotalAmount = totalAmount;
            order.OrderDate = DateTime.UtcNow;
            order.Status = "Pending";

            try 
            {
                await _mongoDBService.Orders.InsertOneAsync(order);

                foreach (var item in order.Items)
                {
                    var updateStock = Builders<Product>.Update.Inc(p => p.StockQuantity, -item.Quantity);
                    var updateResult = await _mongoDBService.Products.UpdateOneAsync(p => p.Id == item.ProductId, updateStock);
                    
                    if (updateResult.ModifiedCount == 0)
                    {
                        Console.WriteLine($"Failed to update stock for product: {item.ProductId}");
                    }
                }

                return (order, null);
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Order creation failed: {ex.Message}");
                return (order, $"Order creation failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateOrderStatus(string orderId, string newStatus, string userId, bool isAdmin = false)
        {
            try 
            {
                if (!Order.StatusValidation.IsValidStatus(newStatus))
                    return (false, $"Invalid status. Allowed statuses are: {string.Join(", ", Order.StatusValidation.ValidStatuses)}");

                var order = await _mongoDBService.Orders.Find(o => o.Id == orderId).FirstOrDefaultAsync();
                if (order == null)
                    return (false, "Order not found");

                if (!isAdmin && order.UserId != userId)
                    return (false, "Not authorized to update this order's status");

                if (!Order.StatusValidation.IsValidStatusTransition(order.Status, newStatus))
                    return (false, $"Cannot transition from {order.Status} to {newStatus}");

                if (newStatus == "Cancelled" && (order.Status == "Pending" || order.Status == "Processing"))
                {
                    foreach (var item in order.Items)
                    {
                        var updateStock = Builders<Product>.Update.Inc(p => p.StockQuantity, item.Quantity);
                        await _mongoDBService.Products.UpdateOneAsync(p => p.Id == item.ProductId, updateStock);
                    }
                }

                var update = Builders<Order>.Update
                    .Set(o => o.Status, newStatus)
                    .Set(o => o.UpdatedAt, DateTime.UtcNow);

                var updateResult = await _mongoDBService.Orders.UpdateOneAsync(o => o.Id == orderId, update);
                
                return (updateResult.ModifiedCount > 0, null);
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Order status update failed: {ex.Message}");
                return (false, $"Order status update failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteOrder(string orderId, string userId, bool isAdmin = false)
        {
            try 
            {
                var order = await _mongoDBService.Orders.Find(o => o.Id == orderId).FirstOrDefaultAsync();
                if (order == null)
                    return (false, "Order not found");

                if (!isAdmin && order.UserId != userId)
                    return (false, "Not authorized to delete this order");

                if (order.Status != "Pending")
                    return (false, "Only pending orders can be deleted");

                foreach (var item in order.Items)
                {
                    var updateStock = Builders<Product>.Update.Inc(p => p.StockQuantity, item.Quantity);
                    var updateResult = await _mongoDBService.Products.UpdateOneAsync(p => p.Id == item.ProductId, updateStock);
                    
                    if (updateResult.ModifiedCount == 0)
                    {
                        Console.WriteLine($"Failed to restore stock for product: {item.ProductId}");
                    }
                }

                var deleteResult = await _mongoDBService.Orders.DeleteOneAsync(o => o.Id == orderId);
                
                return (deleteResult.DeletedCount > 0, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Order deletion failed: {ex.Message}");
                return (false, $"Order deletion failed: {ex.Message}");
            }
        }

        public async Task<bool> DeleteOrder(string id)
        {
            var result = await _mongoDBService.Orders.DeleteOneAsync(o => o.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
