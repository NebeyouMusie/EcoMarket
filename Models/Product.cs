using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace EcoMarket.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public required string Name { get; set; }

        [BsonElement("description")]
        public required string Description { get; set; }

        [BsonElement("category")]
        public required string Category { get; set; }

        [BsonElement("price")]
        public required decimal Price { get; set; }

        [BsonElement("sellerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string SellerId { get; set; }

        [BsonElement("imageUrl")]
        public string? ImageUrl { get; set; }

        [BsonElement("stockQuantity")]
        public required int StockQuantity { get; set; }

        [BsonElement("isEcoFriendly")]
        public bool IsEcoFriendly { get; set; } = true;

        [BsonElement("ecoFeatures")]
        public List<string> EcoFeatures { get; set; } = new();

        [BsonElement("ecoCertifications")]
        public List<string> EcoCertifications { get; set; } = new();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("averageRating")]
        public double AverageRating { get; set; } = 0;

        [BsonElement("reviewCount")]
        public int ReviewCount { get; set; } = 0;
    }
}
