// GraphQL resolvers for social-media-api
// Each resolver connects a schema field to your data source (DB, REST API, etc.)
const PostDto = require("./dtos/PostDto");
const postAPI = require("./dataSources/postAPI");
const commentAPI = require("./dataSources/commentAPI");

module.exports = {
  Query: {
    // Returns all posts
    /**
     * Use parent for nested field resolvers.
     * Use args for query/mutation arguments.
     * Use context for shared resources like data sources.
     * @param {*} _ 
     * @param {*} __ 
     * @param {*} param2 
     * @returns 
     */
    posts: async () => {
            console.log("Fetching all posts from Cosmos DB");

      // Fetch posts from postAPI
      return postAPI.getAllPosts();
    },
    // Returns a single post by ID
    post: async (_, { id }) => {
      return postAPI.getPostById(id);
    },
    // Returns all comments for a post
    comments: async (_, { postId }) => {
      return commentAPI.getCommentsByPost(postId);
    },
    // Returns a single comment by ID
    comment: async (_, { id }) => {
      return commentAPI.getCommentById(id);
    },
    // Returns all likes for a post
    likes: async (_, { postId }, { dataSources }) => {
      return dataSources.likeAPI.getLikesByPost(postId);
    },
    // Returns a media file by ID
    media: async (_, { mediaId }, { dataSources }) => {
      return dataSources.mediaAPI.getMediaById(mediaId);
    },
  },
  Mutation: {
    // Creates a new post
    createPost: async (_, { authorId, content, mediaUrl }) => {
      const postDto = new PostDto({ authorId, content, mediaUrl });
      return postAPI.createPost(postDto);
    },
    // Updates an existing post
    updatePost: async (_, { id, content, mediaUrl, likeCount, commentCount }) => {
      const postDto = new PostDto({ content, mediaUrl, likeCount, commentCount });
      return postAPI.updatePost(id, postDto);
    },
    // Deletes a post
    deletePost: async (_, { id }) => {
      return postAPI.deletePost(id);
    },
    // Creates a new comment
    createComment: async (_, { postId, authorId, content }) => {
      return commentAPI.createComment({ postId, authorId, content });
    },
    // Updates a comment
    updateComment: async (_, { id, content }) => {
      return commentAPI.updateComment(id, { content });
    },
    // Deletes a comment
    deleteComment: async (_, { id }) => {
      return commentAPI.deleteComment(id);
    },
    // Likes a post
    likePost: async (_, { postId, authorId }, { dataSources }) => {
      return dataSources.likeAPI.likePost(postId, authorId);
    },
    // Unlikes a post
    unlikePost: async (_, { likeId }, { dataSources }) => {
      return dataSources.likeAPI.unlikePost(likeId);
    },
    // Follows a user
    followUser: async (_, { userId, followId }, { dataSources }) => {
      return dataSources.profileAPI.followUser(userId, followId);
    },
    // Unfollows a user
    unfollowUser: async (_, { userId, unfollowId }, { dataSources }) => {
      return dataSources.profileAPI.unfollowUser(userId, unfollowId);
    },
    // Deletes a media file
    deleteMedia: async (_, { mediaId }, { dataSources }) => {
      return dataSources.mediaAPI.deleteMedia(mediaId);
    },
  },
  // Field resolvers (example for nested fields)
  Post: {
    // Resolves the author field for a post
    author: async (post, _, { dataSources }) => {
      return dataSources.userAPI.getUserById(post.authorId);
    },
    // Resolves comments for a post
    comments: async (post, _, { dataSources }) => {
      return dataSources.commentAPI.getCommentsByPost(post.id);
    },
    // Resolves likes for a post
    likes: async (post, _, { dataSources }) => {
      return dataSources.likeAPI.getLikesByPost(post.id);
    },
  },
  Comment: {
    // Resolves the author field for a comment
    author: async (comment, _, { dataSources }) => {
      return dataSources.userAPI.getUserById(comment.authorId);
    },
    // Resolves the post field for a comment
    post: async (comment, _, { dataSources }) => {
      return postAPI.getPostById(comment.postId);
    },
  },
  Like: {
    // Resolves the user field for a like
    user: async (like, _, { dataSources }) => {
      return dataSources.userAPI.getUserById(like.authorId);
    },
    // Resolves the post field for a like
    post: async (like, _, { dataSources }) => {
      return dataSources.postAPI.getPostById(like.postId);
    },
  },
  Media: {
    // Resolves the uploadedBy field for a media file
    uploadedBy: async (media, _, { dataSources }) => {
      return dataSources.userAPI.getUserById(media.authorId);
    },
  },
};
