using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using EcoMarket.Models;
using BC = BCrypt.Net.BCrypt;

namespace EcoMarket.Services
{
    public class AuthService
    {
        private readonly MongoDBService _mongoDBService;
        private readonly JwtService _jwtService;

        public AuthService(MongoDBService mongoDBService, JwtService jwtService)
        {
            _mongoDBService = mongoDBService;
            _jwtService = jwtService;
        }

        public async Task<(User User, string Token, string Message, bool Success)> Register(User user)
        {
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
                return (user, string.Empty, "Email and password are required", false);

            var existingUser = await _mongoDBService.Users
                .Find(u => u.Email == user.Email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
                return (user, string.Empty, "User with this email already exists", false);

            user.Password = BC.HashPassword(user.Password);
            user.Role ??= "user";
            user.CreatedAt = DateTime.UtcNow;
            user.LastLoginAt = DateTime.UtcNow;

            await _mongoDBService.Users.InsertOneAsync(user);
            var token = _jwtService.GenerateToken(user);

            return (user, token, "Registration successful", true);
        }

        public async Task<(User User, string Token, string Message, bool Success)> Login(string email, string password)
        {
            var emptyUser = new User 
            { 
                Email = email,
                Name = "Unknown",
                Password = "********"
            };
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return (emptyUser, string.Empty, "Email and password are required", false);

            var user = await _mongoDBService.Users
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();

            if (user == null)
                return (emptyUser, string.Empty, "Invalid email or password", false);

            if (!BC.Verify(password, user.Password))
                return (emptyUser, string.Empty, "Invalid email or password", false);

            var update = Builders<User>.Update.Set(u => u.LastLoginAt, DateTime.UtcNow);
            await _mongoDBService.Users.UpdateOneAsync(u => u.Id == user.Id, update);
            user.LastLoginAt = DateTime.UtcNow;

            var token = _jwtService.GenerateToken(user);

            return (user, token, "Login successful", true);
        }

        public async Task<User> GetUserById(string userId)
        {
            return await _mongoDBService.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        }
    }
}
