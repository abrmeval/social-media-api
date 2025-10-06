namespace SocialMedia.Api.Models
{
    public class LoginResponse
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
        public int ExpiresIn { get; set; }
    }
}