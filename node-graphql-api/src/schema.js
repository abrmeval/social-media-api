export default `
  # User type
  type User {
    id: ID!
    username: String!
    email: String!
    registeredAt: String!
    lastUpdatedAt: String
    lastLoginAt: String
    isActive: Boolean!
    role: String!
    following: [User!]! # List of users this user follows
    followers: [User!]! # List of users that follow this user
    followerCount: Int! # Number of followers
    followingCount: Int! # Number of users being followed
    posts: [Post!] # Optionally, resolve posts by user
  }

  # Post type
  type Post {
    id: ID!
    authorId: ID!
    author: User!
    content: String!
    mediaUrl: String
    createdAt: String!
    lastUpdatedAt: String
    isActive: Boolean!
    likeCount: Int!
    commentCount: Int!
    comments: [Comment!] # Optionally, resolve comments for post
    likes: [Like!] # Optionally, resolve likes for post
  }

  # Comment type
  type Comment {
    id: ID!
    postId: ID!
    post: Post!
    authorId: ID!
    author: User!
    content: String!
    createdAt: String!
    lastUpdatedAt: String
    isActive: Boolean!
  }

  # Like type
  type Like {
    id: ID!
    postId: ID!
    post: Post!
    authorId: ID!
    user: User!
    createdAt: String!
    isActive: Boolean!
  }

  # Follow/Unfollow response type
  type FollowResponse {
    success: Boolean!
    message: String!
    following: [ID!]!
  }

  # Media type
  type Media {
    id: ID!
    fileName: String!
    containerName: String
    blobUrl: String!
    authorId: ID!
    uploadedBy: User!
    postId: ID
    fileCategory: String
    uploadedAt: String!
  }

  # QUERY ROOT
  type Query {
    posts: [Post!]!
    post(id: ID!): Post
    comments(postId: ID!): [Comment!]!
    comment(id: ID!): Comment
    likes(postId: ID!): [Like!]!
    media(mediaId: ID!): Media
  }

  # MUTATION ROOT
  type Mutation {
    createPost(authorId: ID!, content: String!, mediaUrl: String): Post!
    updatePost(id: ID!, content: String, mediaUrl: String): Post!
    deletePost(id: ID!): Boolean!

    createComment(postId: ID!, authorId: ID!, content: String!): Comment!
    updateComment(id: ID!, content: String): Comment!
    deleteComment(id: ID!): Boolean!

    likePost(postId: ID!, authorId: ID!): Like!
    unlikePost(likeId: ID!): Boolean!

    followUser(currentUserId: ID!, targetUserId: ID!): FollowResponse!
    unfollowUser(currentUserId: ID!, targetUserId: ID!): FollowResponse!

    deleteMedia(mediaId: ID!): Boolean!
  }
`