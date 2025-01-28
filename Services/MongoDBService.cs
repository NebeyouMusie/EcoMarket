using EcoMarket.Models;
using MongoDB.Driver;

namespace EcoMarket.Services
{
    public class MongoDBService
    {
        private readonly IMongoDatabase _database;
        private readonly DatabaseSettings _settings;

        public MongoDBService(DatabaseSettings settings)
        {
            _settings = settings;

            try
            {
                var client = new MongoClient(_settings.ConnectionString);
                _database = client.GetDatabase(_settings.DatabaseName);

                _database.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}").Wait();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to connect to MongoDB: {ex.Message}. Please check your connection string and ensure MongoDB is running.");
            }
        }

        public IMongoCollection<User> Users =>
            _database.GetCollection<User>(_settings.UserCollectionName);

        public IMongoCollection<Product> Products =>
            _database.GetCollection<Product>(_settings.ProductCollectionName);

        public IMongoCollection<Order> Orders =>
            _database.GetCollection<Order>(_settings.OrderCollectionName);

        public IMongoCollection<Review> Reviews =>
            _database.GetCollection<Review>(_settings.ReviewCollectionName);

        public IMongoCollection<Favorite> Favorites =>
            _database.GetCollection<Favorite>(_settings.FavoriteCollectionName);
    }
}