using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
namespace SocialMedia.Api.Models
{
    /// <summary>
    /// Represents a user in the social media app.
    /// </summary>
    public class UserDto
    {
        /// <summary>User's unique identifier.</summary>
        [SwaggerIgnore]
        [JsonProperty("id")]
        public string? Id { get; set; }

        /// <summary>User's display name.</summary>
        [Required]
        [JsonProperty("username")]
        public string? Username { get; set; }

        /// <summary>User's email address.</summary>
        [Required]
        [EmailAddress]
        [JsonProperty("email")]
        public string? Email { get; set; }

        /// <summary>User's hashed password.</summary>
        [SwaggerIgnore]
        public string? PasswordHash { get; set; }

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
        public bool IsActive { get; set; } = true;

        /// <summary>URL to user's profile image.</summary>
        [JsonProperty("profileImageUrl")]
        public string? ProfileImageUrl { get; set; }
    }
}