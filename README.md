# EcoMarket API

EcoMarket is a RESTful API for an eco-friendly e-commerce platform that connects environmentally conscious sellers with buyers. The platform facilitates the trading of sustainable, eco-friendly products while promoting environmental awareness.

## Features

- 🔐 **Authentication & Authorization**
  - JWT-based authentication
  - Role-based access control (Seller/User)
  - Secure password hashing
  
- 📦 **Product Management**
  - CRUD operations for eco-friendly products
  - Product categorization
  - Eco-friendly features and certifications
  - Advanced search and filtering
  
- 📄 **Pagination**
  - Efficient handling of large datasets
  - Customizable page size (1-50 items)
  - Pagination metadata included in responses

- 🔍 **Search Capabilities**
  - Search by product name and description
  - Filter by category
  - Filter eco-friendly products

- 🛍️ **Order Management**
  - Create, read, update, and delete orders
  - Order status management (e.g., pending, shipped, delivered)

- 📝 **Review System**
  - Create, read, update, and delete reviews
  - Review rating and comment system

## Prerequisites

- [.NET 9.0](https://dotnet.microsoft.com/download) or later
- [MongoDB](https://www.mongodb.com/try/download/community)
- A text editor or IDE (e.g., Visual Studio Code, Visual Studio)

## Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd EcoMarket
   ```

2. **Environment Variables**
   Create a `.env` file in the root directory with the following content:
   ```
   MONGO_URL=your_mongodb_connection_string
   JWT_SECRET=your_jwt_secret_key
   ```
   Replace the values with your actual MongoDB connection string and a secure JWT secret.

3. **Install Dependencies**
   Run the following commands to install required packages:
   ```powershell
   dotnet add package MongoDB.Driver
   dotnet add package MongoDB.Bson
   dotnet add package MongoDB.Driver.Core
   dotnet add package BCrypt.Net-Next
   dotnet add package System.IdentityModel.Tokens.Jwt
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
   dotnet add package DotNetEnv
   dotnet add package Swashbuckle.AspNetCore
   dotnet restore
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```
   The API will be available at `http://localhost:5039`

## API Documentation

### Authentication Endpoints

#### 1. Register a New User
```http
POST /api/auth/register
Content-Type: application/json

{
    "name": "John Doe",
    "email": "john@example.com",
    "password": "SecurePass123!",
    "phone": "+1234567890",
    "address": "123 Green Street",
    "role": "seller"
}
```
Response: Returns user details and JWT token

#### 2. Login
```http
POST /api/auth/login
Content-Type: application/json

{
    "email": "john@example.com",
    "password": "SecurePass123!"
}
```
Response: Returns user details and JWT token

#### 3. Get Current User (Protected)
```http
GET /api/auth/me
Authorization: Bearer your_jwt_token
```
Response: Returns current user's details

### User Endpoints

#### 1. Get User Details (Protected)
```http
GET /api/users/{id}
Authorization: Bearer your_jwt_token
```
Response: Returns user details

#### 2. Update User Profile (Protected)
```http
PUT /api/users/{id}
Authorization: Bearer your_jwt_token
Content-Type: application/json

{
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "+1234567890",
    "address": "123 Green Street"
}
```
Response: Returns updated user details

#### 3. Delete User Account (Protected)
```http
DELETE /api/users/{id}
Authorization: Bearer your_jwt_token
```
Response: Returns success message

#### 4. Get Products by User ID
```http
GET /api/products/user/{userId}
```
Response: Returns products created by the specified user

### Product Endpoints

#### 1. List All Products (Paginated)
```http
GET /api/products?pageNumber=1&pageSize=10
```
Response: Returns paginated list of all products

#### 2. Get Single Product
```http
GET /api/products/{id}
```
Response: Returns details of a specific product

#### 3. Search Products (Paginated)
```http
GET /api/products/search?query=bottle&pageNumber=1&pageSize=10
```
Response: Returns paginated list of products matching the search query

#### 4. Get Products by Category (Paginated)
```http
GET /api/products/category/{category}?pageNumber=1&pageSize=10
```
Response: Returns paginated list of products in the specified category

#### 5. Get Eco-Friendly Products (Paginated)
```http
GET /api/products/eco-friendly?pageNumber=1&pageSize=10
```
Response: Returns paginated list of eco-friendly products

#### 6. Create Product (Protected - Seller Only)
```http
POST /api/products
Authorization: Bearer your_jwt_token
Content-Type: application/json

{
    "name": "Reusable Bottle 3",
    "description": "Reusable bottle",
    "price": 10,
    "category": "Household",
    "sellerId": "6798b3149421fcd7b4b427a7",
    "CreatedById" : "6798b3149421fcd7b4b427a7",
    "imageUrl": "https://example.com/reusable.jpg",
    "stockQuantity": 100,
    "isEcoFriendly": true,
    "ecoFeatures": ["Reusable", "BPA-free", "Recyclable"],
    "ecoCertifications": []
}
```
Response: Returns created product details

#### 7. Update Product (Protected - Seller Only)
```http
PUT /api/products/{id}
Authorization: Bearer your_jwt_token
Content-Type: application/json

{
    "name": "Updated Eco Water Bottle",
    "description": "Updated description",
    "price": 24.99,
    "stockQuantity": 150
}
```
Response: Returns updated product details

#### 8. Delete Product (Protected - Seller Only)
```http
DELETE /api/products/{id}
Authorization: Bearer your_jwt_token
```
Response: Returns success message

### Order Endpoints

#### 1. Create Order (Protected)
```http
POST /api/orders
Authorization: Bearer your_jwt_token
Content-Type: application/json

{
    "productId": "product_id",
    "quantity": 2,
    "shippingAddress": "123 Green Street, Eco City",
    "paymentMethod": "Credit Card"
}
```
Response: Returns created order details

#### 2. Get User Orders (Protected)
```http
GET /api/orders/user
Authorization: Bearer your_jwt_token
```
Response: Returns list of user's orders

#### 3. Get Order Details (Protected)
```http
GET /api/orders/{id}
Authorization: Bearer your_jwt_token
```
Response: Returns specific order details

#### 4. Update Order Status (Protected - Seller Only)
```http
PUT /api/orders/{id}/status
Authorization: Bearer your_jwt_token
Content-Type: application/json

{
    "status": "Shipped"
}
```
Response: Returns updated order details

#### 5. Cancel Order (Protected)
```http
DELETE /api/orders/{id}
Authorization: Bearer your_jwt_token
```
Response: Returns success message

### Review Endpoints

#### 1. Get Product Reviews (Paginated)
```http
GET /api/reviews/product/{productId}?pageNumber=1&pageSize=10
```
Response: Returns paginated list of reviews for a product

#### 2. Get User Reviews (Protected)
```http
GET /api/reviews/user
Authorization: Bearer your_jwt_token
```
Response: Returns list of authenticated user's reviews

#### 3. Create Review (Protected)
```http
POST /api/reviews
Authorization: Bearer your_jwt_token
Content-Type: application/json

{   
    "userId": "user_id",
    "productId": "product_id",
    "rating": 5,
    "comment": "Great eco-friendly product!",
    "images": ["https://example.com/review1.jpg"]
}
```
Response: Returns created review details

#### 4. Update Review (Protected)
```http
PUT /api/reviews/{id}
Authorization: Bearer your_jwt_token
Content-Type: application/json

{
    "rating": 4,
    "comment": "Updated review comment",
    "images": ["https://example.com/updated-review.jpg"]
}
```
Response: 204 No Content

#### 5. Delete Review (Protected)
```http
DELETE /api/reviews/{id}
Authorization: Bearer your_jwt_token
```
Response: 204 No Content

### Favorites Endpoints

#### 1. **GET /api/favorites/user/{userId} (Protected)**
   - Retrieve user's favorite products
   - Requires user authentication
   - User can only retrieve their own favorites

#### 2. **POST /api/favorites (Protected)**
   - Add a product to user's favorites
   - Requires user authentication
   - User can only add favorites for their own account

#### 3. **DELETE /api/favorites/{id} (Protected)**
   - Remove a specific favorite item
   - Requires user authentication
   - User can only delete their own favorites

#### 4. **DELETE /api/favorites/user/{userId}/product/{productId} (Protected)**
   - Remove a specific product from user's favorites
   - Requires user authentication
   - User can only remove favorites from their own account

## Response Examples

#### Success Response (200 OK)
```json
{
    "items": [
        {
            "id": "product_id",
            "name": "Eco Water Bottle",
            "description": "Reusable water bottle made from recycled materials",
            "category": "Household",
            "price": 19.99,
            "sellerId": "seller_id",
            "imageUrl": "https://example.com/bottle.jpg",
            "stockQuantity": 100,
            "isEcoFriendly": true,
            "ecoFeatures": ["Reusable", "BPA-free", "Recyclable"],
            "ecoCertifications": ["GreenSeal"],
            "createdAt": "2025-01-18T15:33:58Z"
        }
    ],
    "totalItems": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10,
    "hasNextPage": true,
    "hasPreviousPage": false
}
```

#### Authentication Success Response
```json
{
    "message": "Login successful. Use this token for authenticated requests.",
    "token": "your.jwt.token",
    "user": {
        "id": "user_id",
        "name": "John Doe",
        "email": "john@example.com",
        "role": "seller",
        "phone": "+1234567890",
        "address": "123 Green Street",
        "createdAt": "2025-01-18T15:33:58Z",
        "lastLoginAt": "2025-01-18T15:33:58Z"
    }
}
```

#### Error Response (400, 401, 404)
```json
{
    "error": "Detailed error message"
}
```

#### Order Response Example
```json
{
    "id": "order_id",
    "userId": "user_id",
    "productId": "product_id",
    "quantity": 2,
    "totalAmount": 39.98,
    "status": "Pending",
    "shippingAddress": "123 Green Street, Eco City",
    "paymentMethod": "Credit Card",
    "createdAt": "2025-01-18T15:33:58Z",
    "updatedAt": "2025-01-18T15:33:58Z"
}
```

#### Review Response Example
```json
{
    "id": "review_id",
    "userId": "user_id",
    "productId": "product_id",
    "rating": 5,
    "comment": "Great eco-friendly product!",
    "images": ["https://example.com/review1.jpg"],
    "createdAt": "2025-01-18T15:33:58Z",
    "updatedAt": "2025-01-18T15:33:58Z"
}
```

## Protected Endpoints
The following endpoints require authentication (JWT token in Authorization header):
- `GET /api/auth/me`
- `POST /api/products`
- `PUT /api/products/{id}`
- `DELETE /api/products/{id}`
- `POST /api/orders`
- `GET /api/orders/user`
- `GET /api/orders/{id}`
- `PUT /api/orders/{id}/status` (Seller only)
- `DELETE /api/orders/{id}`
- `POST /api/reviews`
- `GET /api/reviews/user`
- `PUT /api/reviews/{id}`
- `DELETE /api/reviews/{id}`
- `GET /api/favorites/user/{userId}`
- `POST /api/favorites`
- `DELETE /api/favorites/{id}`
- `DELETE /api/favorites/user/{userId}/product/{productId}`

## Pagination Parameters
All paginated endpoints accept these query parameters:
- `pageNumber`: Page number (≥ 1, defaults to 1 if invalid)
- `pageSize`: Items per page (1-50, defaults to 10 if invalid)

Example:
```http
GET /api/products?pageNumber=2&pageSize=20
```

## Error Handling

The API uses standard HTTP status codes:
- 200: Success
- 400: Bad Request (invalid input)
- 401: Unauthorized (invalid/missing token)
- 404: Not Found
- 500: Server Error

Error responses include a message explaining the error:
```json
{
    "error": "Detailed error message"
}
```

## Security

- Passwords are hashed using BCrypt
- JWT tokens are required for protected endpoints
- Environment variables are used for sensitive data
- CORS is configured for secure cross-origin requests

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
