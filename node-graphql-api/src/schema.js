module.exports = `
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
    following: [ID!]!
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

    followUser(userId: ID!, followId: ID!): User!
    unfollowUser(userId: ID!, unfollowId: ID!): User!

    deleteMedia(mediaId: ID!): Boolean!
  }
`