| Category     | Method | Endpoint                                  | Description                      |
|--------------|--------|-------------------------------------------|----------------------------------|
| Users        | GET    | /api/users                                | List all users                   |
|              | GET    | /api/users/{id}                           | Get user by ID                   |
|              | POST   | /api/users                                | Create user                      |
|              | PUT    | /api/users/{id}                           | Update user                      |
|              | DELETE | /api/users/{id}                           | Delete user                      |
| Auth         | POST   | /api/auth/login                           | Login                            |
|              | POST   | /api/auth/register                        | Register                         |
| Posts        | GET    | /api/posts                                | List all posts                   |
|              | GET    | /api/posts/{id}                           | Get post by ID                   |
|              | POST   | /api/posts                                | Create post                      |
|              | PUT    | /api/posts/{id}                           | Edit post                        |
|              | DELETE | /api/posts/{id}                           | Delete post                      |
| Comments     | GET    | /api/posts/{postId}/comments              | List comments for post           |
|              | GET    | /api/comments/{id}                        | Get comment by ID                |
|              | POST   | /api/posts/{postId}/comments              | Add comment to post              |
|              | PUT    | /api/comments/{id}                        | Edit comment                     |
|              | DELETE | /api/comments/{id}                        | Delete comment                   |
| Likes        | POST   | /api/posts/{postId}/likes                 | Like post                        |
|              | DELETE | /api/posts/{postId}/likes/{likeId}        | Unlike post                      |
|              | GET    | /api/posts/{postId}/likes                 | Get all likes for post           |
| Media        | POST   | /api/media/upload                         | Upload media file                |
|              | GET    | /api/media/{mediaId}                      | Download media file              |
|              | DELETE | /api/media/{mediaId}                      | Delete media file                |
| Feed/Profile | GET    | /api/feed                                 | User feed                        |
|              | GET    | /api/profile/me                           | Current user profile             |
| Search       | GET    | /api/search?query=...                     | Search                           |