// commentAPI.js - Data source for Comment operations using Azure Cosmos DB
// Implements CRUD operations for comments, matching GraphQL schema and REST API logic

import { database } from "../cosmos-client.js";
import CommentDto from "../dtos/CommentDto.js";

// Helper to convert raw DB data to CommentDto
function toCommentDto(data) {
  return new CommentDto({
    id: data.id ?? null,
    postId: data.postId ?? null,
    authorId: data.authorId ?? null,
    content: data.content ?? null,
    createdAt: data.createdAt,
    lastUpdatedAt: data.lastUpdatedAt ?? null,
    isActive: data.isActive ?? true,
  });
}
class CommentAPI {
  constructor() {
    this.container = database.container("comments");
  }
  // Fetch all comments for a post
  async getCommentsByPost(postId) {
    const querySpec = {
      query:
        "SELECT * FROM c WHERE c.postId = @postId AND c.isActive = true ORDER BY c.createdAt ASC",
      parameters: [{ name: "@postId", value: postId }],
    };
    const iterator = this.container.items.query(querySpec);
    const { resources } = await iterator.fetchAll();
    return resources.map(toCommentDto);
  }
  // Fetch a single comment by ID
  async getCommentById(id) {
    try {
      const { resource } = await this.container.item(id, id).read();
      return resource ? toCommentDto(resource) : null;
    } catch (err) {
      if (err.code === 404) return null;
      throw err;
    }
  }
  // Create a new comment
  async createComment({ postId, authorId, content }) {
    const comment = new CommentDto({
      id: require("crypto").randomUUID(),
      postId,
      authorId,
      content,
      createdAt: new Date().toISOString(),
      isActive: true,
    });
    const { resource } = await this.container.items.create(comment);
    return toCommentDto(resource);
  }

  // Update an existing comment
  async updateComment(id, { content }) {
    try {
      const { resource: existing } = await this.container.item(id, id).read();

      if (!existing) return null;

      existing.content = content ?? existing.content;
      existing.lastUpdatedAt = new Date().toISOString();
      const { resource } = await this.container.item(id, id).replace(existing);
      return toCommentDto(resource);
    } catch (err) {
      if (err.code === 404) return null;
      throw err;
    }
  }

  // Delete a comment (hard delete)
  async deleteComment(id) {
    try {
      await this.container.item(id, id).delete();
      return true;
    } catch (err) {
      if (err.code === 404) return false;
      throw err;
    }
  }

  // Deactivate a comment (soft delete)
  async deactivateComment(id) {
    const { resource: existing } = await this.container.item(id, id).read();

    if (!existing) return false;

    existing.isActive = false;
    existing.lastUpdatedAt = new Date().toISOString();
    await this.container.item(id, id).replace(existing);
    return true;
  }
}

// Export singleton instance
export default new CommentAPI();
