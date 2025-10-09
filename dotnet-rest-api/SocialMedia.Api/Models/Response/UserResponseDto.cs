using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
namespace SocialMedia.Api.Models
{
    /// <summary>
    /// Represents a user in the social media app.
    /// </summary>
    public class UserResponseDto
    {
        /// <summary>User's unique identifier.</summary>
        [JsonProperty("id")]
        public string? Id { get; set; }

        /// <summary>User's display name.</summary>
        [JsonProperty("username")]
        public string? Username { get; set; }

        /// <summary>User's email address.</summary>
        [JsonProperty("email")]
        public string? Email { get; set; }

        /// <summary>Indicates if the password is temporary and needs to be changed.</summary>
        [SwaggerIgnore]
        public bool IsTemporaryPassword { get; set; }

        /// <summary>Date the user registered.</summary>
        [SwaggerIgnore]
        [JsonProperty("registeredAt")]
        public DateTime RegisteredAt { get; set; }

        /// <summary>Date of the user's last update.</summary>
        [SwaggerIgnore]
        [JsonProperty("lastUpdatedAt")]
        public DateTime? LastUpdatedAt { get; set; }

        /// <summary>Date of the user's last login.</summary>
        /// <remarks>Null if the user has never logged in.</remarks>
        [SwaggerIgnore]
        [JsonProperty("lastLoginAt")]
        public DateTime? LastLoginAt { get; set; }

        /// <summary>Indicates if the user account is active.</summary>
        [SwaggerIgnore]
        [JsonProperty("isActive")]
        public bool IsActive { get; set; } = true;

        /// <summary>URL to user's profile image.</summary>
        [JsonProperty("profileImageUrl")]
        public string? ProfileImageUrl { get; set; }
    }
}