// likeAPI.js - Data source for Like operations using Azure Cosmos DB
// Implements CRUD operations for likes, matching GraphQL schema and REST API logic

import { database } from "../cosmos-client.js";
import LikeDto from "../dtos/LikeDto.js";

/**
 * Converts raw Cosmos DB data to LikeDto
 * @param {Object} data - Raw data from Cosmos DB
 * @returns {LikeDto}
 */
function toLikeDto(data) {
  return new LikeDto({
    id: data.id ?? null,
    postId: data.postId ?? null,
    authorId: data.authorId ?? null,
    createdAt: data.createdAt,
    isActive: data.isActive ?? true
  });
}

class LikeAPI {
  constructor() {
    this.container = database.container("likes");
  }
  /**
   * Fetch all likes for a specific post
   * @param {string} postId - The post ID
   * @returns {Promise<LikeDto[]>}
   */
  async getLikesByPost(postId) {
    try {
      const querySpec = {
        query: "SELECT * FROM c WHERE c.postId = @postId AND c.isActive = true ORDER BY c.createdAt DESC",
        parameters: [{ name: "@postId", value: postId }]
      };
      const iterator = this.container.items.query(querySpec);
      const { resources } = await iterator.fetchAll();
      return resources.map(toLikeDto);
    } catch (err) {
      console.error(`Error fetching likes for post ${postId}:`, err);
      throw err;
    }
  }

  /**
   * Fetch a single like by ID
   * @param {string} id - The like ID
   * @returns {Promise<LikeDto|null>}
   */
  async getLikeById(id) {
    try {
      const { resource } = await this.container.item(id, id).read();
      return resource ? toLikeDto(resource) : null;
    } catch (err) {
      if (err.code === 404) return null;
      console.error(`Error fetching like ${id}:`, err);
      throw err;
    }
  }

  /**
   * Check if a user already liked a post
   * @param {string} postId - The post ID
   * @param {string} authorId - The user ID
   * @returns {Promise<LikeDto|null>}
   */
  async findExistingLike(postId, authorId) {
    try {
      const querySpec = {
        query: "SELECT * FROM c WHERE c.postId = @postId AND c.authorId = @authorId AND c.isActive = true",
        parameters: [
          { name: "@postId", value: postId },
          { name: "@authorId", value: authorId }
        ]
      };
      const iterator = this.container.items.query(querySpec);
      const { resources } = await iterator.fetchAll();
      return resources.length > 0 ? toLikeDto(resources[0]) : null;
    } catch (err) {
      console.error(`Error checking existing like:`, err);
      throw err;
    }
  }

  /**
   * Create a new like (like a post)
   * @param {Object} params
   * @param {string} params.postId - The post ID
   * @param {string} params.authorId - The user ID
   * @returns {Promise<LikeDto>}
   */
  async likePost({ postId, authorId }) {
    try {
      // Check if user already liked this post
      const existing = await this.findExistingLike(postId, authorId);
      if (existing) {
        throw new Error("You already liked this post");
      }

      const { randomUUID } = await import("crypto");
      const like = new LikeDto({
        id: randomUUID(),
        postId,
        authorId,
        createdAt: new Date().toISOString(),
        isActive: true
      });

      const { resource } = await this.container.items.create(like);
      return toLikeDto(resource);
    } catch (err) {
      console.error(`Error liking post ${postId}:`, err);
      throw err;
    }
  }

  /**
   * Delete a like (unlike a post) - hard delete
   * @param {string} id - The like ID
   * @returns {Promise<boolean>}
   */
  async unlikePost(id) {
    try {
      await this.container.item(id, id).delete();
      return true;
    } catch (err) {
      if (err.code === 404) return false;
      console.error(`Error unliking post (deleting like ${id}):`, err);
      throw err;
    }
  }

  /**
   * Deactivate a like (soft delete)
   * @param {string} id - The like ID
   * @returns {Promise<boolean>}
   */
  async deactivateLike(id) {
    try {
      const { resource: existing } = await this.container.item(id, id).read();

      if (!existing) return false;

      existing.isActive = false;
      await this.container.item(id, id).replace(existing);
      return true;
    } catch (err) {
      if (err.code === 404) return false;
      console.error(`Error deactivating like ${id}:`, err);
      throw err;
    }
  }

  /**
   * Get like count for a post
   * @param {string} postId - The post ID
   * @returns {Promise<number>}
   */
  async getLikeCount(postId) {
    try {
      const querySpec = {
        query: "SELECT VALUE COUNT(1) FROM c WHERE c.postId = @postId AND c.isActive = true",
        parameters: [{ name: "@postId", value: postId }]
      };
      const iterator = this.container.items.query(querySpec);
      const { resources } = await iterator.fetchAll();
      return resources[0] || 0;
    } catch (err) {
      console.error(`Error getting like count for post ${postId}:`, err);
      return 0;
    }
  }
}

// Export singleton instance
export default new LikeAPI();