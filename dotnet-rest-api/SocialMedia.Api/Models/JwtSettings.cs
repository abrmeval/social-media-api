namespace SocialMedia.Api.Models
{
    public class JwtSettings
    {
        // The URL of your Key Vault
        // Something like "https://your-keyvault.vault.azure.net/"
        public string? KeyVaultUrl { get; set; }

        // The name of the RSA key you created in Key Vault
        // This is just an identifier, not a secret
        public string? SigningKeyName { get; set; }

        // The issuer is typically your API's URL or a name that identifies your system
        // This goes inside every token you create and helps prevent tokens from
        // one system being used in another. Think of it like a stamp that says
        // "This token was issued by the Contoso API"
        public string? Issuer { get; set; }

        // The audience is who the token is intended for, typically your API's URL
        // This prevents a token meant for your API from being used elsewhere
        // It's like addressing an envelope - even if someone intercepts it,
        // they can see it's not meant for them
        public string? Audience { get; set; }

        // How long tokens remain valid before expiring, in minutes
        // Shorter is more secure (limits damage if a token is stolen) but
        // requires users to re-authenticate more often. For most APIs,
        // fifteen to sixty minutes is a good balance
        public int ExpirationMinutes { get; set; } = 60;
    }
}