using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcoMarket.Models
{
    public class Review
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

        [BsonElement("rating")]
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public required int Rating { get; set; } = 0;

        [BsonElement("comment")]
        [Required(ErrorMessage = "Comment is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 500 characters")]
        public required string Comment { get; set; } = string.Empty;

        [BsonElement("images")]
        public List<string> Images { get; set; } = new();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
