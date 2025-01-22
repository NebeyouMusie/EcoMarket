using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EcoMarket.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string UserId { get; set; }

        [BsonElement("items")]
        public required List<OrderItem> Items { get; set; }

        [BsonElement("shippingAddress")]
        public required ShippingAddress ShippingAddress { get; set; }

        [BsonElement("totalAmount")]
        public decimal TotalAmount { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Pending";

        [BsonElement("orderDate")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Static method to validate order statuses
        public static class StatusValidation
        {
            public static readonly ReadOnlyCollection<string> ValidStatuses = new ReadOnlyCollection<string>(new List<string>
            {
                "Pending",     // Initial order state
                "Processing",  // Order is being prepared
                "Shipped",     // Order has been shipped
                "Delivered",   // Order has been received by customer
                "Cancelled",   // Order has been cancelled
                "Refunded"     // Order has been refunded
            });

            public static bool IsValidStatus(string status)
            {
                return ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
            }

            // Define valid status transitions
            public static bool IsValidStatusTransition(string currentStatus, string newStatus)
            {
                return newStatus switch
                {
                    "Pending" => currentStatus == "Pending",
                    "Processing" => currentStatus == "Pending",
                    "Shipped" => currentStatus == "Processing",
                    "Delivered" => currentStatus == "Shipped",
                    "Cancelled" => currentStatus is "Pending" or "Processing",
                    "Refunded" => currentStatus is "Delivered" or "Shipped",
                    _ => false
                };
            }
        }
    }
}
