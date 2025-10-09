using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using SocialMedia.Api.Interfaces;
using SocialMedia.Api.Models;
using Microsoft.AspNetCore.Authorization;
using SocialMedia.Api.Common;

namespace SocialMedia.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly PasswordHasher<UserDto> _passwordHasher;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly ITokenService _tokenService;

        public AuthController(ILogger<AuthController> logger,
        ICosmosDbService cosmosDbService,
        ITokenService tokenService)
        {
            _logger = logger;
            _passwordHasher = new PasswordHasher<UserDto>();
            _cosmosDbService = cosmosDbService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="request">User registration details.</param>
        /// <returns>Result of registration.</returns>
        [HttpPost("register")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Username = request.Username,
                Email = request.Email,
                //Ensure every new user gets the default "User" role
                Role = AppConstants.DefaultRole,
                RegisteredAt = DateTime.UtcNow
            };

            // Hash the password before storing
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password!);
            await _cosmosDbService.GetContainer("users").CreateItemAsync(user, new PartitionKey(user.Id));
            return StatusCode(201); // Created
        }

        /// <summary>
        /// Logs in a user and returns a JWT token.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                // Find user by email (pseudo-code)
                var user = _cosmosDbService.GetContainer("users")
                    .GetItemLinqQueryable<UserDto>()
                    .Where(u => u.Email == request.Email)
                    .FirstOrDefault();

                if (user == null)
                    return Unauthorized();

                // Verify password
                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, request.Password!);

                if (result == PasswordVerificationResult.Failed)
                    return Unauthorized();

                var claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id!.ToString()),
                new Claim(ClaimTypes.Name, user.Username!),
                // Add more claims as needed
                };

                // Generate the JWT token using our service
                // This token contains all the claims we need and is cryptographically
                // signed with the key from Key Vault
                var token = await _tokenService.GenerateTokenAsync(user.Id.ToString(), user.Username!, new[] { user.Role });

                _logger.LogInformation("User {Username} logged in successfully", user.Username);

                _logger.LogInformation("User {Username} logged in successfully", user.Username);

                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;
                await _cosmosDbService.GetContainer("users").UpsertItemAsync(user, new PartitionKey(user.Id!.ToString()));

                // Return the token to the client
                // The client should store this securely (never in localStorage if
                // you're concerned about XSS attacks - consider httpOnly cookies instead)
                return Ok(new LoginResponse
                {
                    Token = token,
                    Username = user.Username,
                    ExpiresIn = 3600 // Seconds until expiration
                });
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Cosmos DB error during login for {Email}", request.Email);
                return StatusCode((int)ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", request.Email);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Validates the JWT token.
        /// </summary>
        /// <returns> </returns>
        /// <remarks>
        /// This endpoint allows clients to verify if their stored token is still valid.
        /// </remarks>
        /// <response code="200">If the token is valid.</response>
        /// <response code="401">If the token is invalid or expired.</response>
        [HttpGet("validate")]
        [Authorize] // This endpoint requires authentication
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult ValidateToken()
        {
            // This endpoint simply returns success if the token is valid
            // The [Authorize] attribute does all the validation work
            // This is useful for client apps to check if their stored token is still valid
            return Ok(new
            {
                valid = true,
                username = User.Identity?.Name,
                roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value)
            });
        }

        /// <summary>
        /// Refreshes the JWT token for an authenticated user.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This endpoint allows users to get a new token before their current one expires.
        /// This provides a better user experience than forcing re-login.
        /// </remarks>
        /// <response code="200">Returns the new token.</response>
        /// <response code="400">If the token claims are invalid.</response>
        [HttpPost("refresh")]
        [Authorize] // User must have a valid token to refresh it
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RefreshToken()
        {
            // This endpoint allows users to get a new token before their current one expires
            // This provides a better user experience than forcing re-login

            // Get the user information from the current token's claims
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string? username = User.Identity?.Name;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
            {
                return BadRequest("Invalid token claims");
            }

            // Generate a new token with the same claims
            var newToken = await _tokenService.GenerateTokenAsync(userId, username, roles);

            return Ok(new LoginResponse
            {
                Token = newToken,
                Username = username,
                ExpiresIn = 3600
            });
        }
    }
}
