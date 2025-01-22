using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using EcoMarket.Models;

namespace EcoMarket.Services
{
    public class UserService
    {
        private readonly MongoDBService _mongoDBService;

        public UserService(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _mongoDBService.Users.Find(_ => true).ToListAsync();
        }

        public async Task<User> GetUserById(string id)
        {
            return await _mongoDBService.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User> CreateUser(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            await _mongoDBService.Users.InsertOneAsync(user);
            return user;
        }

        public async Task<bool> UpdateUser(string id, User updatedUser)
        {
            var existingUser = await GetUserById(id);
            if (existingUser == null)
                return false;

            updatedUser.Id = id;
            var result = await _mongoDBService.Users.ReplaceOneAsync(u => u.Id == id, updatedUser);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteUser(string id)
        {
            var result = await _mongoDBService.Users.DeleteOneAsync(u => u.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
