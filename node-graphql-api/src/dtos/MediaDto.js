/**
 * MediaDto - Data Transfer Object for Media files
 * Matches the .NET MediaDto structure in SocialMedia.Api
 * Note: This only handles metadata - actual file uploads are done via REST API
 */
class MediaDto {
  constructor({
    id = null,
    fileName = null,
    containerName = null,
    blobUrl = null,
    authorId = null,
    postId = null,
    fileCategory = 'OTHER',
    uploadedAt = new Date()
  } = {}) {
    this.id = id;
    this.fileName = fileName;
    this.containerName = containerName;
    this.blobUrl = blobUrl;
    this.authorId = authorId;
    this.postId = postId;
    this.fileCategory = fileCategory;
    this.uploadedAt = uploadedAt;
  }
}

export default MediaDto;
