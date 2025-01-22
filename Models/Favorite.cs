using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace EcoMarket.Models
{
    public class Favorite
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required(ErrorMessage = "User ID is required")]
        public required string UserId { get; set; } = string.Empty;

        [BsonElement("productId")]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required(ErrorMessage = "Product ID is required")]
        public required string ProductId { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
