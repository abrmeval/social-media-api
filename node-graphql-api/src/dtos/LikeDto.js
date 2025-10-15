// LikeDto.js - Data Transfer Object for a like on a social media post
// Mirrors the .NET LikeDto structure for consistency

class LikeDto {
  /**
   * @param {Object} data
   * @param {string|null} data.id
   * @param {string|null} data.postId
   * @param {string|null} data.authorId
   * @param {string} data.createdAt
   * @param {boolean} data.isActive
   */
  constructor({
    id = null,
    postId = null,
    authorId = null,
    createdAt,
    isActive = true
  }) {
    this.id = id;
    this.postId = postId;
    this.authorId = authorId;
    this.createdAt = createdAt;
    this.isActive = isActive;
  }
}

export default LikeDto;
