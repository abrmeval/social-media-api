using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace SocialMedia.Api.Models
{
    /// <summary>
    /// Data Transfer Object representing a social media post.
    /// </summary>
    public class PostDto
    {
        /// <summary>
        /// Unique identifier for the post (Cosmos DB document id).
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Identifier for the user who created the post.
        /// </summary>
        [Required]
        [JsonProperty("authorId")]
        public string? AuthorId { get; set; }

        /// <summary>
        /// The textual content of the post.
        /// </summary>
        [Required]
        [JsonProperty("content")]
        [MaxLength(350, ErrorMessage = "Post content cannot exceed 350 characters.")]
        public string? Content { get; set; }

        /// <summary>
        /// URL pointing to the media (image, video, etc.) associated with the post, stored in Azure Blob Storage.
        /// </summary>
        [JsonProperty("mediaUrl")]
        [MaxLength(2048, ErrorMessage = "Media URL cannot exceed 2048 characters.")]
        [RegularExpression(@"^(http|https)://", ErrorMessage = "Media URL must start with http:// or https://")]
        public string? MediaUrl { get; set; }

        /// <summary>
        /// Timestamp indicating when the post was created (in UTC).
        /// </summary>
        [SwaggerIgnore]
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp indicating when the post was last updated (in UTC).
        /// </summary>
        [SwaggerIgnore]
        [JsonProperty("lastUpdatedAt")]
        public DateTime? LastUpdatedAt { get; set; }

        /// <summary>
        /// Indicates if the post is active or has been deactivated.
        /// </summary>
        [SwaggerIgnore]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Number of likes associated with the post.
        /// </summary>
        [JsonProperty("likeCount")]
        public int LikeCount { get; set; }

        /// <summary>
        /// Number of comments associated with the post.
        /// </summary>
        [JsonProperty("commentCount")]
        public int CommentCount { get; set; }
    }
}