using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using System.Text;
using SocialMedia.Api.Interfaces;
using SocialMedia.Api.Models;
using Azure.Identity;

namespace SocialMedia.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly KeyClient _keyClient;
        private readonly CryptographyClient _cryptoClient;
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            IOptions<JwtSettings> jwtSettings,
            KeyClient keyClient,
            CryptographyClient cryptoClient,
            ILogger<TokenService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _keyClient = keyClient;
            _cryptoClient = cryptoClient;
            _logger = logger;
        }

        public async Task<string> GenerateTokenAsync(
            string userId,
            string username,
            IEnumerable<string> roles)
        {
            // Claims are statements about the user that go inside the token
            // Think of them as facts about the user that the token certifies
            // Anyone with the token can read these claims, so never put
            // sensitive information like passwords or credit card numbers here
            var claims = new List<Claim>
            {
            // The subject (sub) claim identifies who the token is about
            // This is typically the user's ID in your system
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            
            // The JWT ID (jti) is a unique identifier for this specific token
            // This is useful for token revocation or tracking
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            
            // The name claim contains the user's display name
            // This is convenient for showing "Welcome, John" messages
            new Claim(ClaimTypes.Name, username),
            
            // The issued at (iat) claim records when the token was created
            // This is automatically set but can be useful for auditing
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Calculate expiration time
            var now = DateTime.UtcNow;
            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            // Get the RSA key from Key Vault
            // This retrieves metadata about the key, including its public key
            // but the private key never leaves Key Vault
            var keyVaultKey = await _keyClient.GetKeyAsync(_jwtSettings.SigningKeyName);

            // Manually construct the JWT header
            // The header tells anyone validating the token what algorithm was used
            // and optionally which key was used (the kid claim)
            var header = new Dictionary<string, object>
    {
        { "alg", "RS256" },  // RSA signature with SHA-256
        { "typ", "JWT" }      // Token type is JWT
    };

            // Manually construct the JWT payload
            // This contains all the claims plus the standard JWT timing claims
            var payload = new Dictionary<string, object>
            {
                { "sub", userId },
                { "jti", Guid.NewGuid().ToString() },
                { "name", username },
                { "iat", new DateTimeOffset(now).ToUnixTimeSeconds() },
                { "nbf", new DateTimeOffset(now).ToUnixTimeSeconds() },  // Not valid before
                {  "exp", new DateTimeOffset(expiration).ToUnixTimeSeconds() },  // Expiration
                { "iss", _jwtSettings.Issuer },
                { "aud", _jwtSettings.Audience }
    };

            // Add role claims to the payload
            // If there are multiple roles, this creates an array in the JSON
            if (roles.Any())
            {
                var roleArray = roles.ToArray();
                if (roleArray.Length == 1)
                {
                    payload["role"] = roleArray[0];
                }
                else
                {
                    payload["role"] = roleArray;
                }
            }

            // Serialize the header and payload to JSON
            var headerJson = System.Text.Json.JsonSerializer.Serialize(header);
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);

            // Encode them in base64url format (JWT standard encoding)
            // Base64url is like base64 but uses URL-safe characters
            var headerEncoded = Base64UrlEncoder.Encode(headerJson);
            var payloadEncoded = Base64UrlEncoder.Encode(payloadJson);

            // Combine header and payload with a period separator
            // This creates the unsigned token in the format: header.payload
            var unsignedToken = $"{headerEncoded}.{payloadEncoded}";

            // Convert the unsigned token to bytes for signing
            // Key Vault needs the data as a byte array to create the signature
            var dataToSign = Encoding.UTF8.GetBytes(unsignedToken);

            // Send the data to Key Vault for signing
            // This is the critical operation where the private key is used
            // The private key never leaves Key Vault's hardware security module
            var signResult = await _cryptoClient.SignDataAsync(
                SignatureAlgorithm.RS256,
                dataToSign);

            // Encode the signature in base64url format to match JWT standards
            var signature = Base64UrlEncoder.Encode(signResult.Signature);

            // Construct the final signed token by combining all three parts
            // JWT structure is: header.payload.signature
            var signedToken = $"{unsignedToken}.{signature}";

            _logger.LogInformation(
                "Generated RSA-signed JWT token for user {UserId} expiring at {Expiration}",
                userId,
                expiration);

            return signedToken;
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            // This method is useful if you need to manually validate tokens
            // Most of the time, ASP.NET Core handles validation automatically
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SigningKeyName);

                var principal = tokenHandler.ValidateToken(token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = _jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = _jwtSettings.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(5)
                    },
                    out _);

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return null;
            }
        }
    }
}