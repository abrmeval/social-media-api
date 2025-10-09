namespace SocialMedia.Api.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(string userId, string username, IEnumerable<string> roles);
    }
}