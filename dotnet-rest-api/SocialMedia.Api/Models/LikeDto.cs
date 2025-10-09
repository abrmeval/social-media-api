using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace SocialMedia.Api.Models
{
    /// <summary>
    /// Data Transfer Object representing a like on a social media post.
    /// </summary>
    public class LikeDto
    {
        /// <summary>Unique identifier for the like.</summary>
        [JsonProperty("id")]
        public string? Id { get; set; }

        /// <summary>ID of the post being liked.</summary>
        [JsonProperty("postId")]
        public string? PostId { get; set; }

        /// <summary>ID of the user who liked the post.</summary>
        [JsonProperty("authorId")]
        public string? AuthorId { get; set; }

        /// <summary>Timestamp when the like was created (UTC).</summary>
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Indicates if the like is active or has been deleted.
        /// </summary>
        [SwaggerIgnore]
        public bool IsActive { get; set; } = true;
    }
}