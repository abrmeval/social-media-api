// postAPI.js - Data source for Post operations using Azure Cosmos DB
// Implements CRUD operations for posts, matching GraphQL schema and REST API logic

import { database } from "../cosmos-client.js";
import PostDto from "../dtos/PostDto.js";

/**
 * PostAPI - Data source for post operations
 * Handles CRUD operations for posts in Cosmos DB
 */
class PostAPI {
  constructor() {
    this.container = database.container("posts");
  }

  // Fetch all posts
  async getAllPosts() {
    console.log("Fetching all posts from Cosmos DB");
    const querySpec = {
      query:
        "SELECT * FROM c WHERE c.isActive = true ORDER BY c.createdAt DESC",
    };
    const iterator = this.container.items.query(querySpec);
    const { resources } = await iterator.fetchAll();
    return resources;
  }

  // Fetch a single post by ID
  async getPostById(id) {
    try {
      const { resource } = await this.container.item(id, id).read();
      return resource;
    } catch (err) {
      if (err.code === 404) return null;
      throw err;
    }
  }

  // Create a new post
  async createPost(postDto) {
    try {
      const post = new PostDto({ ...postDto });
      // Ensure required fields are set
      if (!post.authorId || !post.content) {
        throw new Error("authorId and content are required to create a post.");
      }

      post.id = require("crypto").randomUUID();
      post.mediaUrl = postDto.mediaUrl;
      post.createdAt = new Date().toISOString();
      post.isActive = true;
      post.likeCount = 0;
      post.commentCount = 0;

      const { resource } = await this.container.items.create(post);
      return resource;
    } catch (err) {
      throw err;
    }
  }

  // Update an existing post
  async updatePost(id, postDto) {
    try {
      const { resource: existing } = await this.container.item(id, id).read();

      if (!existing) return null;
      existing.content = postDto.content ?? existing.content;
      existing.mediaUrl = postDto.mediaUrl ?? existing.mediaUrl;
      existing.likeCount = postDto.likeCount ?? existing.likeCount;
      existing.commentCount = postDto.commentCount ?? existing.commentCount;
      existing.lastUpdatedAt = new Date().toISOString();

      const { resource } = await this.container.item(id, id).replace(existing);
      return resource;
    } catch (err) {
      if (err.code === 404) return null;
      throw err;
    }
  }

  // Delete a post
  async deletePost(id) {
    try {
      await this.container.item(id, id).delete();
      return true;
    } catch (err) {
      if (err.code === 404) return false;
      throw err;
    }
  }

  // Deactivate a post (soft delete)
  async deactivatePost(id) {
    const { resource: existing } = await this.container.item(id, id).read();
    if (!existing) return false;
    existing.isActive = false;
    existing.lastUpdatedAt = new Date().toISOString();
    await this.container.item(id, id).replace(existing);
    return true;
  }
}

export default new PostAPI();