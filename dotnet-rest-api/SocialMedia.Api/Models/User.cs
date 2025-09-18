using System;
namespace SocialMedia.Api.Models
{
    /// <summary>
    /// Represents a user in the social media app.
    /// </summary>
    public class User
    {
        /// <summary>User's unique identifier.</summary>
        public Guid Id { get; set; }

        /// <summary>User's display name.</summary>
        public string? Username { get; set; }

        /// <summary>User's email address.</summary>
        public string? Email { get; set; }

        /// <summary>Date the user registered.</summary>
        public DateTime RegisteredAt { get; set; }

        /// <summary>URL to user's profile image.</summary>
        public string? ProfileImageUrl { get; set; }
    }
}