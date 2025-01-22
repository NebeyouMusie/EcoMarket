using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace EcoMarket.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("email")]
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [BsonElement("password")]
        [Required]
        [MinLength(6)]
        public required string Password { get; set; }

        [BsonElement("name")]
        [Required]
        public required string Name { get; set; }

        [BsonElement("role")]
        public string? Role { get; set; }

        [BsonElement("phone")]
        [Phone]
        public string? Phone { get; set; }

        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("lastLoginAt")]
        public DateTime? LastLoginAt { get; set; }
    }
}
