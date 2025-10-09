using Newtonsoft.Json;
using SocialMedia.Api.Common;

namespace SocialMedia.Api.Models
{
    /// <summary>
    /// Data Transfer Object for media files.
    /// </summary>
    public class MediaDto
    {
        /// <summary>Unique identifier for the media file.</summary>
        [JsonProperty("id")]
        public string? Id { get; set; }

        /// <summary>Original file name uploaded by the user.</summary>
        [JsonProperty("fileName")]
        public string? FileName { get; set; }

        /// <summary>Name of the Blob Storage container where the file is stored.</summary>
        [JsonProperty("containerName")]
        public string? ContainerName { get; set; }

        /// <summary>URL to access the media file in Azure Blob Storage.</summary>
        [JsonProperty("blobUrl")]
        public string? BlobUrl { get; set; }

        /// <summary>ID of the user who uploaded the file.</summary>
        [JsonProperty("authorId")]
        public string? AuthorId { get; set; }

        /// <summary>ID of the post this media is associated with (if any).</summary>
        [JsonProperty("postId")]
        public string? PostId { get; set; }

        /// <summary>
        /// Category of the file (e.g., PROFILEIMAGE, POSTIMAGE, OTHER).
        /// </summary>
        [JsonProperty("fileCategory")]
        public string? FileCategory { get; set; }

        /// <summary>Timestamp when the file was uploaded (UTC).</summary>
        [JsonProperty("uploadedAt")]
        public DateTime UploadedAt { get; set; }
    }
}