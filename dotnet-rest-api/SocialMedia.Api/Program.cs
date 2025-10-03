using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SocialMedia.Api.Interfaces;
using SocialMedia.Api.Services;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using SocialMedia.Api.Models;
using Azure.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
// .ConfigureApiBehaviorOptions(options =>
// {
//     // Allows file uploads without ASP.NET Core enforcing the MIME type on the endpoint
//     options.SuppressConsumesConstraintForFormFileParameters = true;

//    // When you suppress this inference by setting it to true, you're saying "Don't make these assumptions. I'll explicitly tell you where each parameter comes from using attributes like [FromBody], [FromRoute], [FromQuery], etc.   
//     options.SuppressInferBindingSourcesForParameters = true;

//     // When you have an API controller, ASP.NET Core automatically validates incoming data against your model's validation attributes (like [Required], [MaxLength], etc.). If validation fails, it automatically returns a 400 Bad Request response with details about what went wrong, and your controller action never even executes.
//     options.SuppressModelStateInvalidFilter = true;

//     // Provides a standardized format for API error responses following RFC 7807. When enabled, certain client errors (like 404s) are automatically converted into ProblemDetails responses with consistent structure.
//     // Setting this to true disables that automatic mapping. You're essentially saying "I want to handle error responses my own way" rather than using the framework's standardized format.
//     options.SuppressMapClientErrors = true;

//     // Customize the links for specific status codes in the generated ProblemDetails responses.
//     options.ClientErrorMapping[StatusCodes.Status404NotFound].Link =
//         "https://httpstatuses.com/404";
// });

// Create a credential that works both locally and in Azure
// This checks if we're running locally (development) or in Azure (production)
TokenCredential credential;

if (builder.Environment.IsDevelopment())
{
    // For local development, use Service Principal credentials
    // These values would come from your configuration (User Secrets, appsettings.Development.json, etc.)
    var clientId = builder.Configuration["AzureAd:ClientId"];
    var clientSecret = builder.Configuration["AzureAd:ClientSecret"];
    var tenantId = builder.Configuration["AzureAd:TenantId"];

    // Create a ClientSecretCredential with your Service Principal details
    // This explicitly tells Azure Identity to use these specific credentials
    credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

    Console.WriteLine("Using Service Principal authentication for local development");
}
else
{
    // For production in Azure, use DefaultAzureCredential
    // This will automatically find and use the Managed Identity
    credential = new DefaultAzureCredential();

    Console.WriteLine("Using DefaultAzureCredential (Managed Identity) for production");
}

// Load configuration
var keyVaultUrl = builder.Configuration["Jwt:KeyVaultUrl"];
builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), credential);

// Bind JWT settings
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("Jwt").Bind(jwtSettings);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Create a KeyClient to retrieve the public key from Key Vault
// This happens once during startup, not on every request
var keyClient = new KeyClient(new Uri(jwtSettings.KeyVaultUrl), credential);

// Retrieve the RSA key from Key Vault
// This gives us access to both the key metadata and the public key material
var keyVaultKey = await keyClient.GetKeyAsync(jwtSettings.SigningKeyName);

// Extract the public key in a format that ASP.NET Core can use for validation
// The public key is not secret, so it's safe to hold it in memory
// This is what we'll use to verify token signatures
var rsaPublicKey = keyVaultKey.Value.Key.ToRSA();
var rsaSecurityKey = new RsaSecurityKey(rsaPublicKey);


// Add JWT config
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Use the RSA public key for validation
        // Notice we're using the public key here, not calling Key Vault
        // This means token validation is fast and doesn't depend on Key Vault availability
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = rsaSecurityKey,

        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5)
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();
            logger.LogWarning(
                "Authentication failed: {Error}",
                context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();
            logger.LogInformation(
                "Token validated for user: {User}",
                context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        }
    };
});

// Register the KeyClient as a singleton for use in the TokenService
// This client will be used to sign tokens using the private key in Key Vault
builder.Services.AddSingleton(keyClient);
builder.Services.AddSingleton(new CryptographyClient(keyVaultKey.Value.Id, credential));

// Register CosmosDbService
builder.Services.AddScoped<ICosmosDbService, CosmosDbService>();
// Register TokenService
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
