using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace SocialMedia.Api.Models
{
    /// <summary>
    /// Data Transfer Object representing a comment on a social media post.
    /// </summary>
    public class CommentDto
    {
        /// <summary>
        /// Unique identifier for the comment.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// ID of the post this comment belongs to.
        /// </summary>
        [Required]
        [JsonProperty("postId")]
        public string? PostId { get; set; }

        /// <summary>
        /// ID of the user who made the comment.
        /// </summary>
        [Required]
        [JsonProperty("authorId")]
        public string? AuthorId { get; set; }

        /// <summary>
        /// The textual content of the comment.
        /// </summary>
        [Required]
        [JsonProperty("content")]
        [MaxLength(350, ErrorMessage = "Comment content cannot exceed 350 characters.")]
        public string? Content { get; set; }

        /// <summary>
        /// Timestamp when the comment was created (UTC).
        /// </summary>
        [SwaggerIgnore]
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [SwaggerIgnore]
        [JsonProperty("lastUpdatedAt")]
        public DateTime? LastUpdatedAt { get; set; }

        /// <summary>
        /// Indicates if the comment is active or has been deleted.
        /// </summary>
        [SwaggerIgnore]
        public bool IsActive { get; set; } = true;
    }
}