// CommentDto.js - Data Transfer Object for a comment on a social media post
// Mirrors the .NET CommentDto structure for consistency

class CommentDto {
  /**
   * @param {Object} data
   * @param {string|null} data.id
   * @param {string|null} data.postId
   * @param {string|null} data.authorId
   * @param {string|null} data.content
   * @param {string} data.createdAt
   * @param {string|null} data.lastUpdatedAt
   * @param {boolean} data.isActive
   */
  constructor({
    id = null,
    postId = null,
    authorId = null,
    content = null,
    createdAt,
    lastUpdatedAt = null,
    isActive = true
  }) {
    this.id = id;
    this.postId = postId;
    this.authorId = authorId;
    this.content = content;
    this.createdAt = createdAt;
    this.lastUpdatedAt = lastUpdatedAt;
    this.isActive = isActive;
  }
}

module.exports = CommentDto;
