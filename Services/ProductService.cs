using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using EcoMarket.Models;
using System.Text.RegularExpressions;

namespace EcoMarket.Services
{
    public class ProductService
    {
        private readonly MongoDBService _mongoDBService;

        public ProductService(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        public async Task<PaginatedResponse<Product>> GetProducts(PaginationParams paginationParams)
        {
            var (validPageNumber, validPageSize) = paginationParams.GetValidatedValues();

            var filter = Builders<Product>.Filter.Empty;
            var totalItems = await _mongoDBService.Products.CountDocumentsAsync(filter);

            var products = await _mongoDBService.Products
                .Find(filter)
                .Skip((validPageNumber - 1) * validPageSize)
                .Limit(validPageSize)
                .ToListAsync();

            return CreatePaginatedResponse(products, totalItems, validPageNumber, validPageSize);
        }

        public async Task<Product> GetProductById(string id)
        {
            return await _mongoDBService.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<PaginatedResponse<Product>> SearchProducts(string query, PaginationParams paginationParams)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query cannot be empty");

            var (validPageNumber, validPageSize) = paginationParams.GetValidatedValues();

            var searchRegex = new Regex(query, RegexOptions.IgnoreCase);
            var filter = Builders<Product>.Filter.Or(
                Builders<Product>.Filter.Regex(p => p.Name, searchRegex),
                Builders<Product>.Filter.Regex(p => p.Description, searchRegex),
                Builders<Product>.Filter.AnyIn(p => p.EcoFeatures, new[] { query })
            );

            var totalItems = await _mongoDBService.Products.CountDocumentsAsync(filter);

            var products = await _mongoDBService.Products
                .Find(filter)
                .Skip((validPageNumber - 1) * validPageSize)
                .Limit(validPageSize)
                .ToListAsync();

            return CreatePaginatedResponse(products, totalItems, validPageNumber, validPageSize);
        }

        public async Task<PaginatedResponse<Product>> GetProductsByCategory(string category, PaginationParams paginationParams)
        {
            var (validPageNumber, validPageSize) = paginationParams.GetValidatedValues();

            var filter = Builders<Product>.Filter.Eq(p => p.Category, category);
            var totalItems = await _mongoDBService.Products.CountDocumentsAsync(filter);

            var products = await _mongoDBService.Products
                .Find(filter)
                .Skip((validPageNumber - 1) * validPageSize)
                .Limit(validPageSize)
                .ToListAsync();

            return CreatePaginatedResponse(products, totalItems, validPageNumber, validPageSize);
        }

        public async Task<Product> CreateProduct(Product product)
        {
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            await _mongoDBService.Products.InsertOneAsync(product);
            return product;
        }

        public async Task<Product> UpdateProduct(string id, Product updatedProduct)
        {
            updatedProduct.UpdatedAt = DateTime.UtcNow;
            await _mongoDBService.Products.ReplaceOneAsync(p => p.Id == id, updatedProduct);
            return updatedProduct;
        }

        public async Task<bool> DeleteProduct(string id)
        {
            var result = await _mongoDBService.Products.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<PaginatedResponse<Product>> GetEcoFriendlyProducts(PaginationParams paginationParams)
        {
            var (validPageNumber, validPageSize) = paginationParams.GetValidatedValues();

            var filter = Builders<Product>.Filter.Eq(p => p.IsEcoFriendly, true);
            var totalItems = await _mongoDBService.Products.CountDocumentsAsync(filter);

            var products = await _mongoDBService.Products
                .Find(filter)
                .Skip((validPageNumber - 1) * validPageSize)
                .Limit(validPageSize)
                .ToListAsync();

            return CreatePaginatedResponse(products, totalItems, validPageNumber, validPageSize);
        }

        public async Task<List<Product>> GetProductsByUserIdAsync(string userId)
        {
            return await _mongoDBService.Products
                .Find(p => p.CreatedById == userId)
                .ToListAsync();
        }

        private PaginatedResponse<Product> CreatePaginatedResponse(List<Product> items, long totalItems, int pageNumber, int pageSize)
        {
            return new PaginatedResponse<Product>
            {
                Items = items,
                TotalItems = (int)totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                HasNextPage = pageNumber * pageSize < totalItems,
                HasPreviousPage = pageNumber > 1
            };
        }
    }
}
