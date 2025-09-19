using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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

        /// <summary>Date the user registered.</summary>
        [SwaggerIgnore]
        [JsonProperty("registeredAt")]
        public DateTime RegisteredAt { get; set; }

        /// <summary>URL to user's profile image.</summary>
        [JsonProperty("profileImageUrl")]
        public string? ProfileImageUrl { get; set; }
    }
}