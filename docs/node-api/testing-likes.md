# Testing the Like API

## GraphQL Playground Examples

Once your Apollo Server is running (`npm start`), navigate to `http://localhost:4000` to access GraphQL Playground.

### Query Examples

#### 1. Get all likes for a post
```graphql
query GetLikesForPost {
  likes(postId: "your-post-id-here") {
    id
    postId
    authorId
    createdAt
    isActive
  }
}
```

#### 2. Get likes with post details
```graphql
query GetLikesWithPost {
  likes(postId: "your-post-id-here") {
    id
    authorId
    createdAt
    post {
      id
      content
      likeCount
    }
  }
}
```

#### 3. Get post with all its likes
```graphql
query GetPostWithLikes {
  post(id: "your-post-id-here") {
    id
    content
    likeCount
    likes {
      id
      authorId
      createdAt
    }
  }
}
```

### Mutation Examples

#### 1. Like a post
```graphql
mutation LikePost {
  likePost(postId: "your-post-id-here", authorId: "your-user-id-here") {
    id
    postId
    authorId
    createdAt
    isActive
  }
}
```

#### 2. Unlike a post
```graphql
mutation UnlikePost {
  unlikePost(likeId: "your-like-id-here")
}
```

#### 3. Like a post and return with post details
```graphql
mutation LikePostWithDetails {
  likePost(postId: "your-post-id-here", authorId: "your-user-id-here") {
    id
    authorId
    createdAt
    post {
      id
      content
      likeCount
    }
  }
}
```

## Testing Workflow

### 1. Create a test post first
```graphql
mutation CreateTestPost {
  createPost(authorId: "user123", content: "Test post for likes") {
    id
    content
    likeCount
  }
}
```

### 2. Like the post
```graphql
mutation LikeTestPost {
  likePost(postId: "POST_ID_FROM_STEP_1", authorId: "user456") {
    id
    postId
    authorId
    createdAt
  }
}
```

### 3. Verify the like was created
```graphql
query VerifyLikes {
  likes(postId: "POST_ID_FROM_STEP_1") {
    id
    authorId
    createdAt
  }
}
```

### 4. Check post's like count
```graphql
query CheckPostLikes {
  post(id: "POST_ID_FROM_STEP_1") {
    id
    content
    likeCount
    likes {
      id
      authorId
    }
  }
}
```

### 5. Unlike the post
```graphql
mutation UnlikeTestPost {
  unlikePost(likeId: "LIKE_ID_FROM_STEP_2")
}
```

## Error Scenarios

### 1. Try to like the same post twice (should fail)
```graphql
mutation DuplicateLike {
  likePost(postId: "same-post-id", authorId: "same-user-id") {
    id
    postId
  }
}
```
**Expected**: Error message "You already liked this post"

### 2. Unlike a non-existent like
```graphql
mutation UnlikeNonExistent {
  unlikePost(likeId: "non-existent-id")
}
```
**Expected**: Returns `false`

### 3. Get likes for non-existent post
```graphql
query LikesForNonExistentPost {
  likes(postId: "non-existent-post-id") {
    id
  }
}
```
**Expected**: Returns empty array `[]`

## Direct API Testing with curl (if needed)

### Get likes for a post
```bash
curl -X POST http://localhost:4000/graphql \
  -H "Content-Type: application/json" \
  -d '{
    "query": "query { likes(postId: \"your-post-id\") { id authorId createdAt } }"
  }'
```

### Like a post
```bash
curl -X POST http://localhost:4000/graphql \
  -H "Content-Type: application/json" \
  -d '{
    "query": "mutation { likePost(postId: \"your-post-id\", authorId: \"your-user-id\") { id postId authorId createdAt } }"
  }'
```

### Unlike a post
```bash
curl -X POST http://localhost:4000/graphql \
  -H "Content-Type: application/json" \
  -d '{
    "query": "mutation { unlikePost(likeId: \"your-like-id\") }"
  }'
```

## Notes

- **Duplicate Likes Prevention**: The API automatically prevents users from liking the same post twice
- **Soft Delete**: Likes have an `isActive` flag for soft deletes (admin feature, not exposed in GraphQL)
- **Error Handling**: All errors are caught and logged, returning appropriate responses
- **Like Count**: The `likeCount` field on Post is maintained separately and should be updated when likes change (to be implemented)

## Next Steps

1. Implement authentication to automatically get `authorId` from JWT token
2. Add real-time subscriptions for like updates
3. Implement like count synchronization with Post model
4. Add pagination for likes on popular posts
5. Implement user validation (check if user and post exist before creating like)
