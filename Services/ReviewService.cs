using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using EcoMarket.Models;

namespace EcoMarket.Services
{
    public class ReviewService
    {
        private readonly MongoDBService _mongoDBService;

        public ReviewService(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        public async Task<List<Review>> GetAllReviews()
        {
            return await _mongoDBService.Reviews.Find(Builders<Review>.Filter.Empty).ToListAsync();
        }

        public async Task<Review> GetReviewById(string id)
        {
            return await _mongoDBService.Reviews.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task<PaginatedResponse<Review>> GetProductReviews(string productId, PaginationParams paginationParams)
        {
            var (validPageNumber, validPageSize) = paginationParams.GetValidatedValues();

            var filter = Builders<Review>.Filter.Eq(r => r.ProductId, productId);
            var totalItems = await _mongoDBService.Reviews.CountDocumentsAsync(filter);

            var reviews = await _mongoDBService.Reviews
                .Find(filter)
                .Skip((validPageNumber - 1) * validPageSize)
                .Limit(validPageSize)
                .ToListAsync();

            return new PaginatedResponse<Review>
            {
                Items = reviews,
                TotalItems = (int)totalItems,
                PageNumber = validPageNumber,
                PageSize = validPageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)validPageSize),
                HasNextPage = validPageNumber * validPageSize < totalItems,
                HasPreviousPage = validPageNumber > 1
            };
        }

        public async Task<List<Review>> GetUserReviews(string userId)
        {
            return await _mongoDBService.Reviews
                .Find(r => r.UserId == userId)
                .ToListAsync();
        }

        public async Task<(Review Review, string ErrorMessage)> CreateReview(Review review, string userId)
        {
            // Comprehensive input validation
            if (review == null)
                return (new Review 
                { 
                    UserId = userId ?? string.Empty, 
                    ProductId = string.Empty, 
                    Rating = 0, 
                    Comment = string.Empty 
                }, "Review cannot be null");

            // Validate UserId
            if (string.IsNullOrWhiteSpace(userId))
                return (new Review 
                { 
                    UserId = string.Empty, 
                    ProductId = review.ProductId, 
                    Rating = review.Rating, 
                    Comment = review.Comment 
                }, "User ID is required");

            // Validate ProductId
            if (string.IsNullOrWhiteSpace(review.ProductId))
                return (new Review 
                { 
                    UserId = userId, 
                    ProductId = string.Empty, 
                    Rating = review.Rating, 
                    Comment = review.Comment 
                }, "Product ID is required");

            // Validate Comment
            if (string.IsNullOrWhiteSpace(review.Comment))
                return (new Review 
                { 
                    UserId = userId, 
                    ProductId = review.ProductId, 
                    Rating = review.Rating, 
                    Comment = string.Empty 
                }, "Comment cannot be empty");

            // Console.WriteLine($"Creating Review - UserId: {userId}");
            // Console.WriteLine($"Review Details: ProductId: {review.ProductId}, Rating: {review.Rating}, Comment: {review.Comment}");
            // Console.WriteLine($"Review Images: {string.Join(", ", review.Images ?? new List<string>())}");

            // Verify user exists
            var user = await _mongoDBService.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                Console.WriteLine($"User not found: {userId}");
                return (new Review 
                { 
                    UserId = userId, 
                    ProductId = review.ProductId, 
                    Rating = review.Rating, 
                    Comment = review.Comment 
                }, "User not found");
            }

            // Validate product exists
            var product = await _mongoDBService.Products.Find(p => p.Id == review.ProductId).FirstOrDefaultAsync();
            if (product == null)
            {
                Console.WriteLine($"Product not found: {review.ProductId}");
                return (new Review 
                { 
                    UserId = userId, 
                    ProductId = review.ProductId, 
                    Rating = review.Rating, 
                    Comment = review.Comment 
                }, "Product not found");
            }

            // Validate rating is between 1 and 5
            if (review.Rating < 1 || review.Rating > 5)
            {
                Console.WriteLine($"Invalid rating: {review.Rating}");
                return (new Review 
                { 
                    UserId = userId, 
                    ProductId = review.ProductId, 
                    Rating = review.Rating, 
                    Comment = review.Comment 
                }, "Rating must be between 1 and 5");
            }

            // Check if user has already reviewed this product
            var existingReview = await _mongoDBService.Reviews
                .Find(r => r.UserId == userId && r.ProductId == review.ProductId)
                .FirstOrDefaultAsync();
            
            if (existingReview != null)
            {
                Console.WriteLine($"User {userId} has already reviewed product {review.ProductId}");
                return (new Review 
                { 
                    UserId = userId, 
                    ProductId = review.ProductId, 
                    Rating = review.Rating, 
                    Comment = review.Comment 
                }, "You have already reviewed this product");
            }

            // Set review metadata
            review.Id = null; // Ensure MongoDB generates a new ID
            review.UserId = userId;
            review.CreatedAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;

            try 
            {
                await _mongoDBService.Reviews.InsertOneAsync(review);
                Console.WriteLine($"Review created successfully: {review.Id}");

                // Update product rating
                await UpdateProductRating(review.ProductId);

                return (review, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Review creation failed: {ex.Message}");
                return (new Review 
                { 
                    UserId = userId, 
                    ProductId = review.ProductId, 
                    Rating = review.Rating, 
                    Comment = review.Comment 
                }, $"Review creation failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateReview(string id, Review updatedReview, string userId)
        {
            var existingReview = await GetReviewById(id);
            if (existingReview == null)
                return (false, "Review not found");

            if (existingReview.UserId != userId)
                return (false, "You can only update your own reviews");

            // Validate rating is between 1 and 5
            if (updatedReview.Rating < 1 || updatedReview.Rating > 5)
                return (false, "Rating must be between 1 and 5");

            updatedReview.Id = id;
            updatedReview.UserId = userId;
            updatedReview.ProductId = existingReview.ProductId; // Don't allow changing product
            updatedReview.CreatedAt = existingReview.CreatedAt;
            updatedReview.UpdatedAt = DateTime.UtcNow;

            var result = await _mongoDBService.Reviews.ReplaceOneAsync(r => r.Id == id, updatedReview);
            if (result.ModifiedCount == 0)
                return (false, "Failed to update review");

            // Update product rating
            await UpdateProductRating(updatedReview.ProductId);

            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteReview(string id, string userId)
        {
            var review = await GetReviewById(id);
            if (review == null)
                return (false, "Review not found");

            if (review.UserId != userId)
                return (false, "You can only delete your own reviews");

            var result = await _mongoDBService.Reviews.DeleteOneAsync(r => r.Id == id);
            if (result.DeletedCount == 0)
                return (false, "Failed to delete review");

            // Update product rating
            await UpdateProductRating(review.ProductId);

            return (true, string.Empty);
        }

        private async Task UpdateProductRating(string productId)
        {
            var reviews = await _mongoDBService.Reviews
                .Find(r => r.ProductId == productId)
                .ToListAsync();

            if (reviews.Count > 0)
            {
                var averageRating = reviews.Average(r => r.Rating);
                var update = Builders<Product>.Update
                    .Set(p => p.AverageRating, averageRating)
                    .Set(p => p.ReviewCount, reviews.Count);

                await _mongoDBService.Products.UpdateOneAsync(
                    p => p.Id == productId,
                    update
                );
            }
        }
    }
}
