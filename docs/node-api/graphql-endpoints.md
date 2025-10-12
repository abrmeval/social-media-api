# Node.js GraphQL API — Endpoint Reference

This document describes the queries and mutations available in the **Node.js GraphQL API** service for the Social Media Backend Project.  
Use these endpoints via Apollo GraphQL Playground at [`http://localhost:4000/`](http://localhost:4000/).

**Technology Stack:**
- **Apollo Server** - Modern GraphQL server implementation
- **Azure Cosmos DB** - NoSQL database for data persistence
- **Node.js** - JavaScript runtime

For setup and migration details, see [Node.js GraphQL Setup Guide](node-graphql-setup-guide.md).

---

## 📚 Query Endpoints

### **Scope Note**
The GraphQL API focuses on posts, comments, and likes. The following are handled exclusively by the REST API:
- **User management** - Admin-only endpoints at `/api/users`
- **User profiles and following** - Profile endpoints at `/api/profile`
- **Personalized feeds** - Feed endpoint at `/api/profile/feed`
- **Authentication** - Login/register at `/api/auth`

### **Post Queries**
```graphql
posts: [Post!]!
post(id: ID!): Post
```
- **Description:** List all posts, or fetch a post by ID.

### **Comment Queries**
```graphql
comments(postId: ID!): [Comment!]!
comment(id: ID!): Comment
```
- **Description:** List comments for a post, or fetch a comment by ID.

### **Like Queries**
```graphql
likes(postId: ID!): [Like!]!
```
- **Description:** List all likes for a post.

### **Media Queries**
```graphql
media(mediaId: ID!): Media
```
- **Description:** Fetch media metadata by ID.

---

## ✏️ Mutation Endpoints

### **Post Mutations**
```graphql
createPost(authorId: ID!, content: String!, mediaUrl: String): Post!
updatePost(id: ID!, content: String, mediaUrl: String): Post!
deletePost(id: ID!): Boolean!
```
- **Description:** Create, update, or delete a post (optionally with media).

### **Comment Mutations**
```graphql
createComment(postId: ID!, authorId: ID!, content: String!): Comment!
updateComment(id: ID!, content: String): Comment!
deleteComment(id: ID!): Boolean!
```
- **Description:** Add, update, or delete a comment on a post.

### **Like Mutations**
```graphql
likePost(postId: ID!, authorId: ID!): Like!
unlikePost(likeId: ID!): Boolean!
```
- **Description:** Like or unlike a post.

### **Profile Mutations**
```graphql
followUser(userId: ID!, followId: ID!): User!
unfollowUser(userId: ID!, unfollowId: ID!): User!
```
- **Description:** Follow or unfollow another user.

### **Media Mutations**
```graphql
deleteMedia(mediaId: ID!): Boolean!
```
- **Description:** Delete media file metadata.

---

## 📝 Sample Queries & Mutations

### Get All Posts
```graphql
query {
  posts {
    id
    content
    author {
      username
    }
    createdAt
  }
}
```

### Create a New Post
```graphql
mutation {
  createPost(authorId: "user123", content: "Hello World!", mediaUrl: "https://...") {
    id
    content
    createdAt
  }
}
```

### Like a Post
```graphql
mutation {
  likePost(postId: "post456", authorId: "user123") {
    id
    createdAt
  }
}
```

### Update a Post
```graphql
mutation {
  updatePost(id: "post456", content: "Updated content") {
    id
    content
    lastUpdatedAt
  }
}
```

### Delete a Comment
```graphql
mutation {
  deleteComment(id: "comment789")
}
```

---

## 🛡️ Authentication & Authorization

### Current Implementation
- GraphQL API does not currently enforce authentication
- Authentication and authorization should be implemented in production
- User management is restricted to REST API with admin-only access

### Recommended Implementation
- Add JWT token validation in Apollo Server context
- Implement field-level authorization for mutations
- Use `@auth` directives or custom middleware for protected operations

---

## 🔄 API Integration

### Working with REST API
- Use REST API (`/api/auth`) for user registration and login
- Use REST API (`/api/users`) for user management (admin only)
- GraphQL API handles posts, comments, and likes with flexible querying

### Media Uploads
- Upload files directly to Azure Blob Storage via REST API
- Media metadata is stored in Cosmos DB
- Use `deleteMedia` mutation to remove media references

---

## 📖 Resources

- [Apollo Server Documentation](https://www.apollographql.com/docs/apollo-server/)
- [GraphQL Documentation](https://graphql.org/learn/)
- [Azure Cosmos DB Node.js SDK](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/sdk-nodejs)
- [Node.js GraphQL Setup Guide](node-graphql-setup-guide.md)
