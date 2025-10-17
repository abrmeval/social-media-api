/**
 * MediaAPI - Data source for media metadata operations
 * Note: Actual file uploads are handled by REST API (/api/media)
 * This API only manages metadata stored in Cosmos DB
 */

import { database } from "../cosmos-client.js";
import MediaDto from "../dtos/MediaDto.js";

/**
 * Creates a MediaDto from a Cosmos DB document
 * @param {Object} document - Raw document from Cosmos DB
 * @returns {MediaDto} - Formatted MediaDto instance
 */
function toMediaDto(document) {
  if (!document) return null;

  return new MediaDto({
    id: document.id,
    fileName: document.fileName,
    containerName: document.containerName,
    blobUrl: document.blobUrl,
    authorId: document.authorId,
    postId: document.postId,
    fileCategory: document.fileCategory || "OTHER",
    uploadedAt: document.uploadedAt
      ? new Date(document.uploadedAt)
      : new Date(),
  });
}

class MediaAPI {
  constructor() {
    this.container = database.container("media");
  }
  /**
   * Gets a media file by ID
   * @param {string} mediaId - The media ID
   * @returns {Promise<MediaDto|null>} - The media metadata or null if not found
   */
  async getMediaById(mediaId) {
    try {
      const { resource } = await this.container.item(mediaId, mediaId).read();
      return resource ? toMediaDto(resource) : null;
    } catch (error) {
      if (error.code === 404) {
        return null;
      }
      console.error("Error fetching media by ID:", error);
      throw new Error("Failed to fetch media");
    }
  }

  /**
   * Deletes a media metadata entry
   * Note: This only deletes the metadata. The actual file in Blob Storage
   * should be deleted via REST API
   * @param {string} mediaId - The media ID
   * @returns {Promise<boolean>} - True if deleted successfully
   */
  async deleteMedia(mediaId) {
    try {
      await this.container.item(mediaId, mediaId).delete();
      return true;
    } catch (error) {
      if (error.code === 404) {
        throw new Error("Media not found");
      }
      console.error("Error deleting media:", error);
      throw new Error("Failed to delete media");
    }
  }

  /**
   * Gets the user who uploaded the media
   * @param {string} authorId - The author/uploader ID
   * @returns {Promise<Object|null>} - The user object
   */
  async getUploadedBy(authorId) {
    // This will be resolved by the User resolver in the parent context
    // We just return the authorId for the resolver to handle
    return { id: authorId };
  }
}
export default new MediaAPI();