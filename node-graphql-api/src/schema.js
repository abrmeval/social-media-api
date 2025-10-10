const { buildSchema } = require('graphql');

module.exports = buildSchema(`
  # User type
  type User {
    id: ID!
    username: String!
    email: String!
    registeredAt: String!
    following: [ID!]!
    posts: [Post!] # Optionally, resolve posts by user
  }

  # Post type
  type Post {
    id: ID!
    author: User!
    content: String!
    mediaUrl: String
    createdAt: String!
    likeCount: Int!
    commentCount: Int!
    comments: [Comment!] # Optionally, resolve comments for post
    likes: [Like!] # Optionally, resolve likes for post
  }

  # Comment type
  type Comment {
    id: ID!
    post: Post!
    author: User!
    content: String!
    createdAt: String!
  }

  # Like type
  type Like {
    id: ID!
    post: Post!
    user: User!
    createdAt: String!
  }

  # Media type
  type Media {
    id: ID!
    fileName: String!
    blobUrl: String!
    uploadedBy: User!
    uploadedAt: String!
  }

  # QUERY ROOT
  type Query {
    users: [User!]!
    user(id: ID!): User
    posts: [Post!]!
    post(id: ID!): Post
    comments(postId: ID!): [Comment!]!
    comment(id: ID!): Comment
    likes(postId: ID!): [Like!]!
    media(mediaId: ID!): Media
  }

  # MUTATION ROOT
  type Mutation {
    createUser(username: String!, email: String!): User!
    createPost(authorId: ID!, content: String!, mediaUrl: String): Post!
    createComment(postId: ID!, authorId: ID!, content: String!): Comment!
    likePost(postId: ID!, userId: ID!): Like!
    unlikePost(likeId: ID!): Boolean!
    followUser(userId: ID!, followId: ID!): User!
    unfollowUser(userId: ID!, unfollowId: ID!): User!
    uploadMedia(fileName: String!, blobUrl: String!, uploadedBy: ID!): Media!
  }
`);