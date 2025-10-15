# REST API Authentication & Authorization

This document describes the authentication and authorization model for the .NET REST API.

---

## Overview

The REST API uses **JWT (JSON Web Token)** based authentication with **role-based authorization** to control access to endpoints.

---

## Authentication

### Login & Registration
- **Login:** `POST /api/auth/login` - Returns JWT token upon successful authentication
- **Register:** `POST /api/auth/register` - Creates new user account

### JWT Token
- Issued upon successful login
- Must be included in `Authorization` header for protected endpoints: `Bearer <token>`
- Contains user claims including `userId`, `username`, `email`, and `role`

---

## Authorization Roles

### Admin Role
**Purpose:** Full system access, including user management

**Permissions:**
- All user CRUD operations via `/api/users` endpoints
- System-wide data access
- User account management

**Endpoints Restricted to Admin:**
- `GET /api/users` - List all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

**Implementation:**
```csharp
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    // All user management endpoints require Admin role
}
```

### User Role (Default)
**Purpose:** Standard authenticated user access

**Permissions:**
- Create, read, update, delete own posts
- Create, read, update, delete own comments
- Like/unlike posts
- Follow/unfollow users
- Upload and manage own media
- View public content

---

## Endpoint Access Matrix

| Endpoint Category | GET (Read) | POST (Create) | PUT (Update) | DELETE |
|------------------|------------|---------------|--------------|--------|
| **Users** (`/api/users`) | Admin only | Admin only | Admin only | Admin only |
| **Auth** (`/api/auth`) | N/A | Public | N/A | N/A |
| **Posts** (`/api/posts`) | Authenticated | Authenticated | Owner/Authenticated | Owner/Authenticated |
| **Comments** (`/api/comments`) | Authenticated | Authenticated | Owner/Authenticated | Owner/Authenticated |
| **Likes** (`/api/posts/{postId}/likes`) | Authenticated | Authenticated | N/A | Owner/Authenticated |
| **Media** (`/api/media`) | Authenticated | Authenticated | N/A | Owner/Authenticated |
| **Profile** (`/api/profile`) | Authenticated | Authenticated | N/A | N/A |

### Profile Endpoints Detail
| Endpoint | Method | Auth Required | Description |
|----------|--------|---------------|-------------|
| `/api/profile/me` | GET | Authenticated | Get current user's profile |
| `/api/profile/{id}/follow` | POST | Authenticated | Follow another user |
| `/api/profile/{id}/unfollow` | POST | Authenticated | Unfollow another user |
| `/api/profile/feed` | GET | Authenticated | Get personalized feed (posts from followed users) |

---

## Authorization Patterns

### Controller-Level Authorization
Applied to entire controller (all endpoints):
```csharp
// Admin-only access to all user management endpoints
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    // All actions require Admin role
}

// Authenticated access to all profile endpoints
[Authorize]
public class ProfileController : ControllerBase
{
    // All actions require authentication (any role)
}
```

### Action-Level Authorization
Applied to specific endpoints:
```csharp
[Authorize]
public async Task<IActionResult> CreatePost([FromBody] PostDto post)
{
    // Requires authentication, any role
}
```

### Resource-Based Authorization
Checking ownership before allowing operations:
```csharp
// Verify the user owns the resource
if (existingPost.AuthorId != userId)
{
    return Forbid();
}
```

---

## Security Considerations

### Best Practices
1. **Always validate JWT tokens** on protected endpoints
2. **Check resource ownership** before allowing modifications
3. **Use HTTPS** in production to protect tokens in transit
4. **Set appropriate token expiration** times
5. **Implement refresh tokens** for long-lived sessions
6. **Log authorization failures** for security monitoring

### Token Management
- Tokens should expire after a reasonable time (e.g., 1-24 hours)
- Refresh tokens can be used to obtain new access tokens
- Implement token revocation for logout functionality
- Store sensitive configuration (JWT secret) in environment variables

### Rate Limiting
Consider implementing rate limiting for:
- Authentication endpoints to prevent brute force attacks
- Public endpoints to prevent abuse
- Resource-intensive operations

---

## Configuration

### JWT Settings
Configure in `appsettings.json`:
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "SocialMediaApi",
    "Audience": "SocialMediaClient",
    "ExpiryMinutes": 60
  }
}
```

### Environment Variables
For production, use environment variables:
- `JWT_SECRET` - Secret key for signing tokens
- `JWT_ISSUER` - Token issuer
- `JWT_AUDIENCE` - Token audience
- `JWT_EXPIRY_MINUTES` - Token expiration time

---

## Testing Authorization

### Using Swagger UI
1. Login via `/api/auth/login` endpoint
2. Copy the returned JWT token
3. Click "Authorize" button in Swagger UI
4. Enter: `Bearer <your-token>`
5. Test protected endpoints

### Using Postman/Thunder Client
1. Login to get JWT token
2. Add `Authorization` header to requests:
   ```
   Authorization: Bearer <your-token>
   ```
3. Test various endpoints with different roles

### Testing Admin Endpoints
1. Ensure test user has `Admin` role in database
2. Login with admin credentials
3. Access `/api/users` endpoints
4. Verify access is granted

### Testing Authorization Failures
1. Try accessing admin endpoints without token (401 Unauthorized)
2. Try accessing admin endpoints with regular user token (403 Forbidden)
3. Try modifying another user's resources (403 Forbidden)

---

## Common Error Responses

### 401 Unauthorized
**Cause:** No token provided or invalid/expired token
```json
{
  "error": "Unauthorized",
  "message": "Authentication required"
}
```

### 403 Forbidden
**Cause:** Valid token but insufficient permissions
```json
{
  "error": "Forbidden",
  "message": "You do not have permission to access this resource"
}
```

---

## Future Enhancements

### Planned Features
- **OAuth 2.0 Integration** - Support for third-party authentication (Google, Microsoft, etc.)
- **Azure AD B2C** - Enterprise authentication and user management
- **Refresh Tokens** - Long-lived sessions with automatic token renewal
- **Scoped Permissions** - More granular access control beyond roles
- **API Keys** - For service-to-service authentication
- **Rate Limiting** - Per-user or per-IP rate limits

### GraphQL API Authorization
The Node.js GraphQL API should implement similar authorization:
- JWT token validation in Apollo Server context
- Field-level authorization using directives
- Integration with same authentication system as REST API

---

## Related Documentation

- [REST API Endpoints](../rest-api-endpoints.md) - Complete endpoint reference
- [Node.js GraphQL Setup Guide](../node-api/node-graphql-setup-guide.md) - GraphQL API setup
- [GraphQL Endpoints](../node-api/graphql-endpoints.md) - GraphQL queries and mutations

---

**Last Updated:** October 11, 2025
