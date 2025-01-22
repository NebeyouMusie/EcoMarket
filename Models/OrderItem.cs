using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EcoMarket.Models
{
    public class OrderItem
    {
        [BsonElement("productId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ProductId { get; set; }

        [BsonElement("quantity")]
        public required int Quantity { get; set; }
    }
}
