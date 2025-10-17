# GraphQL Resolvers Guide

This guide explains how GraphQL resolvers work in our Node.js Apollo Server implementation, including all parameters, resolver types, and execution flow.

---

## Table of Contents
1. [What are Resolvers?](#what-are-resolvers)
2. [Resolver Function Signature](#resolver-function-signature)
3. [Types of Resolvers](#types-of-resolvers)
4. [Resolver Parameters Explained](#resolver-parameters-explained)
5. [Query & Mutation Resolvers](#query--mutation-resolvers)
6. [Type Resolvers (Field Resolvers)](#type-resolvers-field-resolvers)
7. [Execution Flow](#execution-flow)
8. [Best Practices](#best-practices)

---

## What are Resolvers?

**Resolvers** are functions that tell GraphQL **how to fetch data** for each field in your schema. They connect your GraphQL schema to your data sources (databases, REST APIs, etc.).

```javascript
// Schema defines WHAT data is available
type Post {
  id: ID!
  content: String!
  author: User!
}

// Resolver defines HOW to get that data
Post: {
  author: async (post) => {
    return getUserById(post.authorId);
  }
}
```

---

## Resolver Function Signature

Every resolver function receives **four parameters**:

```javascript
fieldName: (parent, args, context, info) => {
  // Resolver logic
}
```

### Parameter Breakdown:

| Parameter | Description | When to Use |
|-----------|-------------|-------------|
| `parent` | The result from the parent resolver | Type resolvers (e.g., `Post.author`) |
| `args` | Arguments passed to the field | Queries/Mutations with parameters |
| `context` | Shared state across all resolvers | Authentication, data sources |
| `info` | Metadata about the query | Advanced use cases (rarely used) |

---

## Resolver Parameters Explained

### 1. **`parent` (or `root`)**

The **result of the parent resolver**. Contains data from the previous level in the query.

```javascript
// Example: Resolving Post.author
export const Post = {
  author: async (parent) => {
    // 'parent' is the Post object from the parent resolver
    console.log(parent);
    // Output: { id: "post123", authorId: "user456", content: "Hello" }
    
    return profileAPI.getUserById(parent.authorId);
  }
}
```

**When it's used:**
- ✅ Type resolvers (Post, Comment, Like, etc.)
- ❌ Root Query/Mutation resolvers (parent is undefined)

**Common naming conventions:**
- `parent` - Most common
- `root` - Also used
- Named after the type: `post`, `comment`, `user`

### 2. **`args` (Arguments)**

The **arguments passed to the GraphQL field**. Contains query variables.

```javascript
// Query with arguments:
query {
  post(id: "post123") {
    content
  }
}

// Resolver receives args:
export const Query = {
  post: async (_, args) => {
    console.log(args);
    // Output: { id: "post123" }
    
    return postAPI.getPostById(args.id);
  }
}

// Using destructuring (preferred):
export const Query = {
  post: async (_, { id }) => {
    return postAPI.getPostById(id);
  }
}
```

**When it's used:**
- ✅ Queries with parameters
- ✅ Mutations with input data
- ❌ Fields without arguments

### 3. **`context`**

**Shared state** available to all resolvers. Used for:
- Authentication data (user info, JWT tokens)
- Data sources
- Request information
- Logging utilities

```javascript
// Setting up context in server.js:
const server = new ApolloServer({
  typeDefs,
  resolvers,
  context: ({ req }) => ({
    user: req.user,           // Authenticated user
    dataSources: {            // Data sources
      userAPI: new UserAPI(),
      postAPI: new PostAPI()
    },
    token: req.headers.authorization
  })
});

// Using context in resolvers:
export const Post = {
  author: async (post, _, context) => {
    // Access data sources from context
    return context.dataSources.userAPI.getUserById(post.authorId);
  }
}

// With destructuring (preferred):
export const Post = {
  author: async (post, _, { dataSources }) => {
    return dataSources.userAPI.getUserById(post.authorId);
  }
}
```

**Current Project Status:**
- ⚠️ Our current implementation doesn't use `context` for data sources
- ✅ We import APIs directly at the top of resolvers.js
- 🔮 Future: Will use context for authentication

### 4. **`info`**

Contains **metadata about the query execution**. Rarely used in practice.

```javascript
export const Query = {
  posts: async (_, __, ___, info) => {
    console.log(info.fieldName);        // "posts"
    console.log(info.returnType);       // [Post!]!
    console.log(info.parentType);       // Query
    
    return postAPI.getAllPosts();
  }
}
```

**When it's used:**
- Advanced query optimization
- Custom directives
- Query complexity analysis
- 📝 **Most projects never need this**

---

## Types of Resolvers

### 1. **Query Resolvers** (Read Operations)

Entry points for fetching data. Always at the **root level**.

```javascript
export const Query = {
  // List all posts
  posts: async () => {
    return postAPI.getAllPosts();
  },
  
  // Get single post by ID
  post: async (_, { id }) => {
    return postAPI.getPostById(id);
  },
  
  // Get comments for a post
  comments: async (_, { postId }) => {
    return commentAPI.getCommentsByPost(postId);
  }
}
```

**GraphQL Usage:**
```graphql
query {
  posts {
    id
    content
  }
  
  post(id: "post123") {
    content
  }
}
```

### 2. **Mutation Resolvers** (Write Operations)

Entry points for modifying data. Always at the **root level**.

```javascript
export const Mutation = {
  // Create a post
  createPost: async (_, { authorId, content, mediaUrl }) => {
    return postAPI.createPost({ authorId, content, mediaUrl });
  },
  
  // Update a post
  updatePost: async (_, { id, content }) => {
    return postAPI.updatePost(id, { content });
  },
  
  // Delete a post
  deletePost: async (_, { id }) => {
    return postAPI.deletePost(id);
  }
}
```

**GraphQL Usage:**
```graphql
mutation {
  createPost(authorId: "user123", content: "Hello World!") {
    id
    content
    createdAt
  }
}
```

### 3. **Type Resolvers** (Field Resolvers)

Resolve **nested fields** on custom types. These run **after** the parent resolver.

```javascript
export const Post = {
  // Resolve the author field on a Post
  author: async (post) => {
    return profileAPI.getUserById(post.authorId);
  },
  
  // Resolve the comments field on a Post
  comments: async (post) => {
    return commentAPI.getCommentsByPost(post.id);
  },
  
  // Resolve the likes field on a Post
  likes: async (post) => {
    return likeAPI.getLikesByPost(post.id);
  }
}
```

**GraphQL Usage:**
```graphql
query {
  post(id: "post123") {
    id
    content
    author {        # Post.author resolver is called
      username
    }
    comments {      # Post.comments resolver is called
      content
    }
    likes {         # Post.likes resolver is called
      id
    }
  }
}
```

---

## Query & Mutation Resolvers

### Query Resolver Example

```javascript
export const Query = {
  // Parameter breakdown:
  likes: async (
    _,              // 1. parent (unused, convention: _)
    { postId },     // 2. args (destructured)
    context,        // 3. context (unused here)
    info            // 4. info (unused)
  ) => {
    try {
      return await likeAPI.getLikesByPost(postId);
    } catch (err) {
      console.error("Error fetching likes:", err);
      return [];
    }
  }
}
```

### Mutation Resolver Example

```javascript
export const Mutation = {
  // Parameter breakdown:
  createComment: async (
    _,                              // 1. parent (unused)
    { postId, authorId, content },  // 2. args (all input data)
    context                         // 3. context (for auth in future)
  ) => {
    // In production, get authorId from context.user instead
    return commentAPI.createComment({ postId, authorId, content });
  }
}
```

---

## Type Resolvers (Field Resolvers)

Type resolvers are the **magic** of GraphQL - they allow you to fetch related data automatically.

### How Type Resolvers Work

```javascript
// 1. Client makes a query
query {
  post(id: "post123") {  // Step 1: Query.post resolver runs
    id
    content
    author {              // Step 2: Post.author resolver runs
      username
    }
  }
}

// 2. Execution flow:

// STEP 1: Query.post resolver executes
export const Query = {
  post: async (_, { id }) => {
    // Returns: { id: "post123", authorId: "user456", content: "Hello" }
    return postAPI.getPostById(id);
  }
}

// STEP 2: Post.author resolver executes
export const Post = {
  author: async (post) => {  // 'post' is the result from Step 1
    // post = { id: "post123", authorId: "user456", content: "Hello" }
    // Now fetch the author using authorId
    return profileAPI.getUserById(post.authorId);
  }
}
```

### Complete Type Resolver Examples

#### Post Type Resolvers

```javascript
export const Post = {
  // Resolve author (User type)
  author: async (post, _, { dataSources }) => {
    // post.authorId exists in the database
    return dataSources.userAPI.getUserById(post.authorId);
  },
  
  // Resolve comments (array of Comments)
  comments: async (post) => {
    // Fetch all comments for this post
    return commentAPI.getCommentsByPost(post.id);
  },
  
  // Resolve likes (array of Likes)
  likes: async (post) => {
    // Fetch all likes for this post
    return likeAPI.getLikesByPost(post.id);
  }
}
```

#### Comment Type Resolvers

```javascript
export const Comment = {
  // Resolve author (User type)
  author: async (comment) => {
    return profileAPI.getUserById(comment.authorId);
  },
  
  // Resolve post (Post type)
  post: async (comment) => {
    return postAPI.getPostById(comment.postId);
  }
}
```

#### Like Type Resolvers

```javascript
export const Like = {
  // Resolve user who liked (User type)
  user: async (like) => {
    return profileAPI.getUserById(like.authorId);
  },
  
  // Resolve the post that was liked (Post type)
  post: async (like) => {
    return postAPI.getPostById(like.postId);
  }
}
```

#### User Type Resolvers

```javascript
export const User = {
  // Resolve list of users being followed
  following: async (user) => {
    return profileAPI.getFollowing(user.id);
  },
  
  // Resolve list of followers
  followers: async (user) => {
    return profileAPI.getFollowers(user.id);
  },
  
  // Resolve follower count
  followerCount: async (user) => {
    return profileAPI.getFollowerCount(user.id);
  },
  
  // Resolve following count
  followingCount: async (user) => {
    return profileAPI.getFollowingCount(user.id);
  }
}
```

#### Media Type Resolvers

```javascript
export const Media = {
  // Resolve the user who uploaded the media
  uploadedBy: async (media) => {
    return profileAPI.getUserById(media.authorId);
  }
}
```

---

## Execution Flow

### Example: Complex Nested Query

```graphql
query {
  post(id: "post123") {
    id
    content
    author {
      username
      followers {
        username
      }
    }
    comments {
      content
      author {
        username
      }
    }
    likes {
      user {
        username
      }
    }
  }
}
```

### Execution Order:

```
1. Query.post(_, { id: "post123" })
   ↓ Returns: Post object

2. Post.author(post)
   ↓ Returns: User object
   
3. User.followers(user)
   ↓ Returns: Array of User objects

4. Post.comments(post)
   ↓ Returns: Array of Comment objects

5. Comment.author(comment) [for each comment]
   ↓ Returns: User object

6. Post.likes(post)
   ↓ Returns: Array of Like objects

7. Like.user(like) [for each like]
   ↓ Returns: User object
```

### Visual Flow Diagram:

```
Query.post
    │
    ├─→ Post.author → User.followers
    │
    ├─→ Post.comments → Comment.author
    │
    └─→ Post.likes → Like.user
```

---

## Resolver Patterns & Conventions

### 1. **Unused Parameters Convention**

Use `_` for unused parameters:

```javascript
// Good: Clear which parameters are used
export const Query = {
  post: async (_, { id }) => {
    return postAPI.getPostById(id);
  }
}

// Also valid: Omit unused parameters
export const Query = {
  posts: async () => {
    return postAPI.getAllPosts();
  }
}
```

### 2. **Destructuring Arguments**

Always destructure args for clarity:

```javascript
// ✅ Good: Clear what arguments are expected
createPost: async (_, { authorId, content, mediaUrl }) => {
  return postAPI.createPost({ authorId, content, mediaUrl });
}

// ❌ Avoid: Not clear what's in args
createPost: async (_, args) => {
  return postAPI.createPost(args);
}
```

### 3. **Error Handling**

Always wrap in try-catch for robustness:

```javascript
export const Query = {
  likes: async (_, { postId }) => {
    try {
      return await likeAPI.getLikesByPost(postId);
    } catch (err) {
      console.error("Error fetching likes:", err);
      return [];  // Return empty array for lists
    }
  }
}

export const Mutation = {
  deletePost: async (_, { id }) => {
    try {
      return await postAPI.deletePost(id);
    } catch (err) {
      console.error("Error deleting post:", err);
      throw new Error(err.message || "Failed to delete post");
    }
  }
}
```

### 4. **Async/Await**

All resolvers that fetch data should be async:

```javascript
// ✅ Good: Using async/await
author: async (post) => {
  return await profileAPI.getUserById(post.authorId);
}

// ✅ Also good: Implicit return
author: async (post) => {
  return profileAPI.getUserById(post.authorId);
}

// ❌ Avoid: Not using async (unless returning immediately)
author: (post) => {
  return profileAPI.getUserById(post.authorId);
}
```

---

## Common Patterns

### 1. **Default Resolvers**

GraphQL automatically resolves fields that match property names:

```javascript
// Schema
type Post {
  id: ID!
  content: String!
  createdAt: String!
}

// You DON'T need to write:
export const Post = {
  id: (post) => post.id,
  content: (post) => post.content,
  createdAt: (post) => post.createdAt
}

// GraphQL does this automatically!
```

**You only need resolvers for:**
- Fields that require data fetching (e.g., `author`)
- Fields that need transformation
- Computed fields

### 2. **Computed Fields**

```javascript
export const Post = {
  // Computed field not in database
  likeCount: async (post) => {
    const likes = await likeAPI.getLikesByPost(post.id);
    return likes.length;
  }
}
```

### 3. **Conditional Resolution**

```javascript
export const Post = {
  author: async (post) => {
    // Only fetch if authorId exists
    if (!post.authorId) return null;
    return profileAPI.getUserById(post.authorId);
  }
}
```

---

## Best Practices

### ✅ Do's

1. **Use meaningful parameter names**
   ```javascript
   // Good
   author: async (post) => { ... }
   
   // Avoid
   author: async (parent) => { ... }
   ```

2. **Handle errors gracefully**
   ```javascript
   try {
     return await api.getData();
   } catch (err) {
     console.error("Error:", err);
     return null; // or []
   }
   ```

3. **Keep resolvers thin**
   ```javascript
   // Business logic in API layer
   createPost: async (_, args) => {
     return postAPI.createPost(args);
   }
   ```

4. **Use async/await consistently**
   ```javascript
   posts: async () => {
     return await postAPI.getAllPosts();
   }
   ```

### ❌ Don'ts

1. **Don't put business logic in resolvers**
   ```javascript
   // ❌ Bad
   createPost: async (_, { content }) => {
     const sanitized = sanitize(content);
     const validated = validate(sanitized);
     return db.create(validated);
   }
   
   // ✅ Good
   createPost: async (_, args) => {
     return postAPI.createPost(args);
   }
   ```

2. **Don't ignore errors**
   ```javascript
   // ❌ Bad
   posts: async () => {
     return postAPI.getAllPosts(); // What if this fails?
   }
   
   // ✅ Good
   posts: async () => {
     try {
       return await postAPI.getAllPosts();
     } catch (err) {
       console.error("Error:", err);
       return [];
     }
   }
   ```

3. **Don't make N+1 queries**
   ```javascript
   // ❌ Bad: Makes separate query for each post
   posts: async () => {
     const posts = await getAllPosts();
     for (let post of posts) {
       post.author = await getUser(post.authorId);
     }
     return posts;
   }
   
   // ✅ Good: Let GraphQL handle it
   // Query resolver returns posts
   // Type resolver handles authors when requested
   ```

---

## Summary

| Resolver Type | Purpose | Uses `parent` | Uses `args` | Uses `context` |
|---------------|---------|---------------|-------------|----------------|
| **Query** | Fetch data | ❌ No | ✅ Yes | ✅ Optional |
| **Mutation** | Modify data | ❌ No | ✅ Yes | ✅ Optional |
| **Type (Field)** | Resolve nested data | ✅ Yes | ✅ Optional | ✅ Optional |

### Key Takeaways:

1. **Query/Mutation resolvers** are entry points - they don't receive parent data
2. **Type resolvers** receive parent data and resolve nested fields
3. **GraphQL is lazy** - only requested fields are resolved
4. **Resolvers are independent** - each resolver is a separate function
5. **Context is shared** - use it for authentication and shared resources

---

## Additional Resources

- [Apollo Server Resolvers Documentation](https://www.apollographql.com/docs/apollo-server/data/resolvers/)
- [GraphQL Resolvers Explained](https://graphql.org/learn/execution/)
- [Understanding GraphQL Execution](https://www.apollographql.com/blog/graphql/basics/graphql-execution-explained/)
