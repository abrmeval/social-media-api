// GraphQL resolvers for social-media-api
// Each resolver connects a schema field to your data source (DB, REST API, etc.)
import { postAPI } from "./dataSources/postAPI.js";
import commentAPI from "./dataSources/commentAPI.js";
import likeAPI from "./dataSources/likeAPI.js";
import profileAPI from "./dataSources/profileAPI.js";
import mediaAPI from "./dataSources/mediaAPI.js";

export const Query = {
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
  likes: async (_, { postId }) => {
    try {
      return await likeAPI.getLikesByPost(postId);
    } catch (err) {
      console.error("Error fetching likes:", err);
      return [];
    }
  },
  // Returns a media file by ID
  media: async (_, { mediaId }) => {
    try {
      return await mediaAPI.getMediaById(mediaId);
    } catch (err) {
      console.error("Error fetching media:", err);
      return null;
    }
  },
};
export const Mutation = {
  // Creates a new post
  createPost: async (_, { authorId, content, mediaUrl }) => {
    return postAPI.createPost({ authorId, content, mediaUrl });
  },
  // Updates an existing post
  updatePost: async (_, { id, content, mediaUrl, likeCount, commentCount }) => {
    return postAPI.updatePost(id, {
      content,
      mediaUrl,
      likeCount,
      commentCount,
    });
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
  likePost: async (_, { postId, authorId }) => {
    try {
      return await likeAPI.likePost({ postId, authorId });
    } catch (err) {
      console.error("Error liking post:", err);
      throw new Error(err.message || "Failed to like post");
    }
  },
  // Unlikes a post
  unlikePost: async (_, { likeId }) => {
    try {
      return await likeAPI.unlikePost(likeId);
    } catch (err) {
      console.error("Error unliking post:", err);
      return false;
    }
  },
  // Follows a user
  followUser: async (_, { currentUserId, targetUserId }) => {
    try {
      return await profileAPI.followUser(currentUserId, targetUserId);
    } catch (err) {
      console.error("Error following user:", err);
      throw new Error(err.message || "Failed to follow user");
    }
  },
  // Unfollows a user
  unfollowUser: async (_, { currentUserId, targetUserId }) => {
    try {
      return await profileAPI.unfollowUser(currentUserId, targetUserId);
    } catch (err) {
      console.error("Error unfollowing user:", err);
      throw new Error(err.message || "Failed to unfollow user");
    }
  },
  // Deletes a media file metadata
  deleteMedia: async (_, { mediaId }) => {
    try {
      return await mediaAPI.deleteMedia(mediaId);
    } catch (err) {
      console.error("Error deleting media:", err);
      throw new Error(err.message || "Failed to delete media");
    }
  },
};
export const Post = {
  // Resolves the author field for a post
  author: async (post, _, { dataSources }) => {
    return dataSources.userAPI.getUserById(post.authorId);
  },
  // Resolves comments for a post
  comments: async (post) => {
    try {
      return await commentAPI.getCommentsByPost(post.id);
    } catch (err) {
      console.error("Error fetching comments for post:", err);
      return [];
    }
  },
  // Resolves likes for a post
  likes: async (post) => {
    try {
      return await likeAPI.getLikesByPost(post.id);
    } catch (err) {
      console.error("Error fetching likes for post:", err);
      return [];
    }
  },
};
export const Comment = {
  // Resolves the author field for a comment
  author: async (comment, _, {}) => {
    return profileAPI.getUserById(comment.authorId);
    //return dataSources.userAPIgetUserById(comment.authorId);
  },
  // Resolves the post field for a comment
  post: async (comment, _, { dataSources }) => {
    return postAPI.getPostById(comment.postId);
  },
};
export const Like = {
  // Resolves the user field for a like
  user: async (like, _, {}) => {
    return profileAPI.getUserById(like.authorId);
  },
  // Resolves the post field for a like
  post: async (like) => {
    try {
      return await postAPI.getPostById(like.postId);
    } catch (err) {
      console.error("Error fetching post for like:", err);
      return null;
    }
  },
};
export const User = {
  // Resolves the following field for a user (list of users they follow)
  following: async (user) => {
    try {
      return await profileAPI.getFollowing(user.id);
    } catch (err) {
      console.error("Error fetching following for user:", err);
      return [];
    }
  },
  // Resolves the followers field for a user (list of users that follow them)
  followers: async (user) => {
    try {
      return await profileAPI.getFollowers(user.id);
    } catch (err) {
      console.error("Error fetching followers for user:", err);
      return [];
    }
  },
  // Resolves follower count
  followerCount: async (user) => {
    try {
      return await profileAPI.getFollowerCount(user.id);
    } catch (err) {
      console.error("Error fetching follower count:", err);
      return 0;
    }
  },
  // Resolves following count
  followingCount: async (user) => {
    try {
      return await profileAPI.getFollowingCount(user.id);
    } catch (err) {
      console.error("Error fetching following count:", err);
      return 0;
    }
  },
};
export const Media = {
  // Resolves the uploadedBy field for a media file
  uploadedBy: async (media) => {
    try {
      return await profileAPI.getUserById(media.authorId);
    } catch (err) {
      console.error("Error fetching media uploader:", err);
      return null;
    }
  },
};
