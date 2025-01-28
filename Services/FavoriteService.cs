using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using EcoMarket.Models;

namespace EcoMarket.Services
{
    public class FavoriteService
    {
        private readonly MongoDBService _mongoDBService;

        public FavoriteService(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        public async Task<List<Favorite>> GetAllFavorites()
        {
            return await _mongoDBService.Favorites.Find(Builders<Favorite>.Filter.Empty).ToListAsync();
        }

        public async Task<Favorite> GetFavoriteById(string id)
        {
            return await _mongoDBService.Favorites.Find(f => f.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Favorite>> GetUserFavorites(string userId)
        {
            return await _mongoDBService.Favorites.Find(f => f.UserId == userId).ToListAsync();
        }

        public async Task<(Favorite Favorite, string ErrorMessage)> CreateFavorite(Favorite favorite)
        {
            Console.WriteLine($"CreateFavorite method started with UserId: {favorite.UserId}, ProductId: {favorite.ProductId}");

            // Validate input
            if (favorite == null)
            {
                Console.WriteLine("Favorite is null");
                return (new Favorite
                {
                    UserId = string.Empty,
                    ProductId = string.Empty
                }, "Favorite cannot be null");
            }
            if (string.IsNullOrWhiteSpace(favorite.UserId))
            {
                Console.WriteLine("UserId is empty or whitespace");
                return (new Favorite
                {
                    UserId = string.Empty,
                    ProductId = favorite.ProductId
                }, "User ID is required");
            }

            if (string.IsNullOrWhiteSpace(favorite.ProductId))
            {
                Console.WriteLine("ProductId is empty or whitespace");
                return (new Favorite
                {
                    UserId = favorite.UserId,
                    ProductId = string.Empty
                }, "Product ID is required");
            }

            Console.WriteLine($"Validating product existence for ProductId: {favorite.ProductId}");
            var product = await _mongoDBService.Products.Find(p => p.Id == favorite.ProductId).FirstOrDefaultAsync();
            if (product == null)
            {
                Console.WriteLine($"Product not found: {favorite.ProductId}");
                return (new Favorite
                {
                    UserId = favorite.UserId,
                    ProductId = favorite.ProductId
                }, "Product not found");
            }

            Console.WriteLine($"Validating user existence for UserId: {favorite.UserId}");
            var user = await _mongoDBService.Users.Find(u => u.Id == favorite.UserId).FirstOrDefaultAsync();
            if (user == null)
            {
                Console.WriteLine($"User not found: {favorite.UserId}");
                return (new Favorite
                {
                    UserId = favorite.UserId,
                    ProductId = favorite.ProductId
                }, "User not found");
            }

            Console.WriteLine($"Checking for existing favorite for User {favorite.UserId} and Product {favorite.ProductId}");
            var existingFavorite = await _mongoDBService.Favorites.Find(
                f => f.UserId == favorite.UserId && f.ProductId == favorite.ProductId
            ).FirstOrDefaultAsync();

            if (existingFavorite != null)
            {
                Console.WriteLine($"Favorite already exists for User {favorite.UserId} and Product {favorite.ProductId}");
                return (new Favorite
                {
                    UserId = favorite.UserId,
                    ProductId = favorite.ProductId
                }, "Product is already in favorites");
            }

            try
            {
                favorite.CreatedAt = DateTime.UtcNow;

                Console.WriteLine($"Attempting to insert favorite for User {favorite.UserId} and Product {favorite.ProductId}");
                await _mongoDBService.Favorites.InsertOneAsync(favorite);

                Console.WriteLine($"Favorite created successfully: {favorite.Id}");
                return (favorite, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Favorite creation failed: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return (new Favorite
                {
                    UserId = favorite.UserId,
                    ProductId = favorite.ProductId
                }, $"Favorite creation failed: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFavorite(string id)
        {
            var result = await _mongoDBService.Favorites.DeleteOneAsync(f => f.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteFavoriteByUserAndProduct(string userId, string productId)
        {
            var result = await _mongoDBService.Favorites.DeleteOneAsync(
                f => f.UserId == userId && f.ProductId == productId);
            return result.DeletedCount > 0;
        }
    }
}