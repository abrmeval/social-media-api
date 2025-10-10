# Node.js GraphQL API — Endpoint Reference

This document describes the queries and mutations available in the **Node.js GraphQL API** service for the Social Media Backend Project.  
Use these endpoints via GraphQL Playground at [`/graphql`](http://localhost:4000/graphql).

---

## 📚 Query Endpoints

### **User Queries**
```graphql
users: [User!]!
user(id: ID!): User
```
- **Description:** List all users, or fetch a user by ID.

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

### **User Mutations**
```graphql
createUser(username: String!, email: String!): User!
followUser(userId: ID!, followId: ID!): User!
unfollowUser(userId: ID!, unfollowId: ID!): User!
```
- **Description:** Create a user, follow/unfollow another user.

### **Post Mutations**
```graphql
createPost(authorId: ID!, content: String!, mediaUrl: String): Post!
```
- **Description:** Create a new post (optionally with media).

### **Comment Mutations**
```graphql
createComment(postId: ID!, authorId: ID!, content: String!): Comment!
```
- **Description:** Add a comment to a post.

### **Like Mutations**
```graphql
likePost(postId: ID!, userId: ID!): Like!
unlikePost(likeId: ID!): Boolean!
```
- **Description:** Like or unlike a post.

### **Media Mutations**
```graphql
uploadMedia(fileName: String!, blobUrl: String!, uploadedBy: ID!): Media!
```
- **Description:** Save media metadata (after file uploaded to Blob Storage).

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
  likePost(postId: "post456", userId: "user123") {
    id
    createdAt
  }
}
```

### Follow a User
```graphql
mutation {
  followUser(userId: "user123", followId: "user789") {
    id
    following
  }
}
```

---

## 🛡️ Notes

- Some mutations and queries may require authentication via JWT.
- For media, upload files directly to Blob Storage, then use `uploadMedia` mutation to register metadata.
- Extend or customize schema and resolvers as needed for your app logic.

---

## 📖 Resources

- [GraphQL Playground](https://github.com/graphql/graphql-playground)
- [Express GraphQL](https://graphql.org/graphql-js/)
- [Azure Cosmos DB Node.js SDK](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/sdk-node)
