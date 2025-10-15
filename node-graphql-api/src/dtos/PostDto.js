// PostDto.js - Data Transfer Object for a social media post
// Mirrors the .NET PostDto structure for consistency

class PostDto {
  /**
   * @param {Object} data
   * @param {string|null} data.id
   * @param {string|null} data.authorId
   * @param {string|null} data.content
   * @param {string|null} data.mediaUrl
   * @param {string} data.createdAt
   * @param {string|null} data.lastUpdatedAt
   * @param {boolean} data.isActive
   * @param {number} data.likeCount
   * @param {number} data.commentCount
   */
  constructor({
    id = null,
    authorId = null,
    content = null,
    mediaUrl = null,
    createdAt,
    lastUpdatedAt = null,
    isActive = true,
    likeCount = 0,
    commentCount = 0
  }) {
    this.id = id;
    this.authorId = authorId;
    this.content = content;
    this.mediaUrl = mediaUrl;
    this.createdAt = createdAt;
    this.lastUpdatedAt = lastUpdatedAt;
    this.isActive = isActive;
    this.likeCount = likeCount;
    this.commentCount = commentCount;
  }
}

export default PostDto;
