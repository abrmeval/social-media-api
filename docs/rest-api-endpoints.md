| Category     | Method | Endpoint                                  | Description                      | Auth Required |
|--------------|--------|-------------------------------------------|----------------------------------|---------------|
| Users        | GET    | /api/users                                | List all users                   | Admin only    |
|              | GET    | /api/users/{id}                           | Get user by ID                   | Admin only    |
|              | POST   | /api/users                                | Create user                      | Admin only    |
|              | PUT    | /api/users/{id}                           | Update user                      | Admin only    |
|              | DELETE | /api/users/{id}                           | Delete user                      | Admin only    |
| Auth         | POST   | /api/auth/login                           | Login                            | None          |
|              | POST   | /api/auth/register                        | Register                         | None          |
| Posts        | GET    | /api/posts                                | List all posts                   | Authenticated |
|              | GET    | /api/posts/{id}                           | Get post by ID                   | Authenticated |
|              | POST   | /api/posts                                | Create post                      | Authenticated |
|              | PUT    | /api/posts/{id}                           | Edit post                        | Authenticated |
|              | DELETE | /api/posts/{id}                           | Delete post                      | Authenticated |
| Comments     | GET    | /api/posts/{postId}/comments              | List comments for post           | Authenticated |
|              | GET    | /api/comments/{id}                        | Get comment by ID                | Authenticated |
|              | POST   | /api/posts/{postId}/comments              | Add comment to post              | Authenticated |
|              | PUT    | /api/comments/{id}                        | Edit comment                     | Authenticated |
|              | DELETE | /api/comments/{id}                        | Delete comment                   | Authenticated |
| Likes        | POST   | /api/posts/{postId}/likes                 | Like post                        | Authenticated |
|              | DELETE | /api/posts/{postId}/likes/{likeId}        | Unlike post                      | Authenticated |
|              | GET    | /api/posts/{postId}/likes                 | Get all likes for post           | Authenticated |
| Media        | POST   | /api/media/upload                         | Upload media file                | Authenticated |
|              | GET    | /api/media/{mediaId}                      | Download media file              | Authenticated |
|              | DELETE | /api/media/{mediaId}                      | Delete media file                | Authenticated |
| Profile      | GET    | /api/profile/me                           | Current user profile             | Authenticated |
|              | POST   | /api/profile/{id}/follow                  | Follow another user              | Authenticated |
|              | POST   | /api/profile/{id}/unfollow                | Unfollow another user            | Authenticated |
|              | GET    | /api/profile/feed                         | Get user's personalized feed     | Authenticated |

---

## 🔐 Authentication & Authorization

### Admin-Only Endpoints
The following endpoints require **Admin role** authorization:
- All `/api/users` endpoints (GET, POST, PUT, DELETE)
- User management is restricted to administrators only

### Authenticated Endpoints
The following operations require authentication via JWT token:
- All write operations (POST, PUT, DELETE) for posts, comments, likes, and media
- All Profile endpoints (`/api/profile/*`)
  - Get current user profile (`/api/profile/me`)
  - Follow/unfollow users (`/api/profile/{id}/follow`, `/api/profile/{id}/unfollow`)
  - Get personalized feed (`/api/profile/feed`)

### Public Endpoints
Read operations are publicly accessible:
- `GET` operations for posts, comments, likes, and media
- Authentication endpoints (`/api/auth/login`, `/api/auth/register`)

---

## 📋 Endpoint Details

### Profile Endpoints (`/api/profile`)
All profile endpoints require authentication and use the `[Authorize]` attribute.

#### Get Current User Profile
- **Endpoint:** `GET /api/profile/me`
- **Description:** Retrieves the authenticated user's profile information, including followed users
- **Auth:** Required (any authenticated user)
- **Returns:** User profile with following list

#### Follow User
- **Endpoint:** `POST /api/profile/{id}/follow`
- **Description:** Adds a user to the current user's following list
- **Auth:** Required (any authenticated user)
- **Parameters:** `id` - User ID to follow
- **Returns:** Success message

#### Unfollow User
- **Endpoint:** `POST /api/profile/{id}/unfollow`
- **Description:** Removes a user from the current user's following list
- **Auth:** Required (any authenticated user)
- **Parameters:** `id` - User ID to unfollow
- **Returns:** Success message

#### Get Personalized Feed
- **Endpoint:** `GET /api/profile/feed`
- **Description:** Retrieves posts from the current user and all users they follow, ordered by creation date (newest first)
- **Auth:** Required (any authenticated user)
- **Returns:** List of posts from followed users

---

## 🚫 Removed/Non-Existent Endpoints

The following endpoints were previously documented but do not exist in the current implementation:
- `GET /api/search?query=...` - Search functionality not implemented
- `GET /api/feed` - Replaced by `/api/profile/feed`