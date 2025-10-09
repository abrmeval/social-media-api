using Azure.Core;
using Microsoft.Extensions.Configuration;
using SocialMedia.Api.Models;

namespace SocialMedia.Api.StartupExtensions
{

    /// <summary>
    /// Extension methods for registering AccessCore services in the DI container.
    /// </summary>
    public static class AccessCoreStartupExtensions
    {
        /// <summary>
        /// /
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection AddKeyVaultExtension(this IServiceCollection services, WebApplicationBuilder builder, TokenCredential credential)
        {

            // Bind JWT settings
            var jwtSettings = new JwtSettings();
            builder.Configuration.GetSection("Jwt").Bind(jwtSettings);
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            // Add Azure Key Vault as a configuration source
            // This loads all secrets from Key Vault and adds them to IConfiguration
            builder.Configuration.AddAzureKeyVault(new Uri(jwtSettings.KeyVaultUrl!), credential);
            return services;
        }
    }
}