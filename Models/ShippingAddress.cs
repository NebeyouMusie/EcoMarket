using MongoDB.Bson.Serialization.Attributes;

namespace EcoMarket.Models
{
    public class ShippingAddress
    {
        [BsonElement("street")]
        public required string Street { get; set; }

        [BsonElement("city")]
        public required string City { get; set; }

        [BsonElement("state")]
        public required string State { get; set; }

        [BsonElement("zipCode")]
        public required string ZipCode { get; set; }

        [BsonElement("country")]
        public required string Country { get; set; }
    }
}
