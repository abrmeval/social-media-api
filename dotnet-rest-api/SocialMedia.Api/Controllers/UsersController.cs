using System.ClientModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using SocialMedia.Api.Interfaces;
using SocialMedia.Api.Models;
using SocialMedia.Api.Utils;

namespace SocialMedia.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin, User")] // Only Admins can manage users
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly PasswordHasher<UserDto> _passwordHasher;

        public UsersController(ILogger<UsersController> logger,
         ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
            _passwordHasher = new PasswordHasher<UserDto>();
        }

        /// <summary>
        /// Retrieves all users in the system.
        /// </summary>
        /// <remarks>
        /// Returns a list of all registered users wrapped in an ApiResponse.
        /// </remarks>
        /// <response code="200">Returns ApiResponse with the list of users</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserResponseDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> GetAll(int pageSize = 20, string? continuationToken = null)
        {
            // Added try-catch to handle Cosmos DB exceptions gracefully
            try
            {
                var container = _cosmosDbService.GetContainer("users");

                // Get all users
                var query = container.GetItemQueryIterator<UserDto>(
                    new QueryDefinition("SELECT * FROM c ORDER BY c.RegisteredAt DESC"),
                    requestOptions: new QueryRequestOptions
                    {
                        MaxItemCount = pageSize
                    },
                    continuationToken: continuationToken
                );

                var userList = new List<UserResponseDto>();


                while (query.HasMoreResults)
                {
                    var page = await query.ReadNextAsync();

                    // Map UserDto to UserResponseDto
                    userList.AddRange(page.Select(u => new UserResponseDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        RegisteredAt = u.RegisteredAt,
                        LastUpdatedAt = u.LastUpdatedAt,
                        LastLoginAt = u.LastLoginAt,
                        IsActive = u.IsActive,
                        ProfileImageUrl = u.ProfileImageUrl
                    }));

                    if (page.Count > 0)
                    {
                        continuationToken = page.ContinuationToken;
                        break; // Exit after the first page to respect pageSize
                    }
                }
                // Return wrapped response
                return Ok(new ApiResponse<IEnumerable<UserResponseDto>>(true, "Users retrieved successfully", userList, continuationToken));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error retrieving users from Cosmos DB");
                // Return wrapped error response
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving users");
                // Return wrapped error response
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <remarks>
        /// Returns the user details wrapped in an ApiResponse if found.
        /// </remarks>
        /// <response code="200">Returns ApiResponse with the user details</response>
        /// <response code="404">If the user is not found or container is missing</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> GetById(string id)
        {
            // Added try-catch to handle Cosmos DB exceptions gracefully
            try
            {
                var container = _cosmosDbService.GetContainer("users");

                // Get user by id
                UserDto user = await container.ReadItemAsync<UserDto>(id, new PartitionKey(id));

                // Map UserDto to UserResponseDto
                var userResponse = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    RegisteredAt = user.RegisteredAt,
                    LastUpdatedAt = user.LastUpdatedAt,
                    LastLoginAt = user.LastLoginAt,
                    IsActive = user.IsActive,
                    ProfileImageUrl = user.ProfileImageUrl,
                    IsTemporaryPassword = user.IsTemporaryPassword
                };
                // Return wrapped response
                return Ok(new ApiResponse<UserResponseDto>(true, "User retrieved successfully", userResponse));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User {id} not found in Cosmos DB");
                // Return wrapped error response
                return NotFound(new ApiResponse<string>(false, $"User {id} not found", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error retrieving user {id} from Cosmos DB");
                // Return wrapped error response
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving user");
                // Return wrapped error response
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Creates a new user in the system.
        /// </summary>
        /// <param name="user">The user details to create.</param>
        /// <remarks>
        /// Registers a new user and returns the created user object wrapped in an ApiResponse.
        /// </remarks>
        /// <response code="201">Returns ApiResponse with the newly created user</response>
        /// <response code="400">If the user data is invalid</response>
        /// <response code="404">If the users container is not found</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> Create([FromBody] UserDto user)
        {
            // Removed unnecessary ModelState validation because ASP.NET Core automatically validates [ApiController] models
            try
            {
                // Added try-catch to handle Cosmos DB exceptions gracefully
                user.Id = Guid.NewGuid().ToString();
                user.PasswordHash = _passwordHasher.HashPassword(user, PasswordGenerator.GetTempPassword());
                user.IsTemporaryPassword = true;
                user.RegisteredAt = DateTime.UtcNow;

                var container = _cosmosDbService.GetContainer("users");

                // Create new user
                await container.CreateItemAsync(user, new PartitionKey(user.Id));
                // Map UserDto to UserResponseDto
                var userResponse = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    RegisteredAt = user.RegisteredAt,
                    IsActive = user.IsActive,
                    IsTemporaryPassword = user.IsTemporaryPassword,
                    ProfileImageUrl = user.ProfileImageUrl
                };
                // Return wrapped response
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, new ApiResponse<UserResponseDto>(true, "User created successfully", userResponse));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error creating user in Cosmos DB");
                // Return wrapped error response
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating user");
                // Return wrapped error response
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Updates an existing user's profile.
        /// </summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <param name="user">The updated user details.</param>
        /// <remarks>
        /// Updates the user profile if the user exists.
        /// </remarks>
        /// <response code="200">User updated successfully</response>
        /// <response code="400">If the user data is invalid</response>
        /// <response code="404">If the user is not found or container is missing</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> Update(string id, [FromBody] UserDto user)
        {
            // Removed unnecessary ModelState validation because ASP.NET Core automatically validates [ApiController] models
            // Added try-catch to handle Cosmos DB exceptions gracefully
            try
            {
                var container = _cosmosDbService.GetContainer("users");

                UserDto existingUser = await container.ReadItemAsync<UserDto>(id, new PartitionKey(id));

                existingUser.Username = user.Username;
                existingUser.Email = user.Email;
                existingUser.ProfileImageUrl = user.ProfileImageUrl;
                existingUser.LastUpdatedAt = DateTime.UtcNow;
                await container.UpsertItemAsync(existingUser, new PartitionKey(existingUser.Id!.ToString()));

                // Map UserDto to UserResponseDto
                var userResponse = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    RegisteredAt = user.RegisteredAt,
                    LastUpdatedAt = user.LastUpdatedAt,
                    LastLoginAt = user.LastLoginAt,
                    IsActive = user.IsActive,
                    ProfileImageUrl = user.ProfileImageUrl,
                    IsTemporaryPassword = user.IsTemporaryPassword
                };

                // Return wrapped response
                return Ok(new ApiResponse<UserResponseDto>(true, "User updated successfully", userResponse));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User {id} not found in Cosmos DB");
                // Return wrapped error response
                return NotFound(new ApiResponse<string>(false, $"User {id} not found", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error updating user {id} in Cosmos DB");
                // Return wrapped error response
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating user");
                // Return wrapped error response
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Deactivates a user account.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// Deactivates the user account if found.
        /// </remarks>
        /// <response code="200">User deactivated successfully</response>
        /// <response code="404">If the user is not found or container is missing</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpPut("{id}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> Deactivate(string id)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("users");

                UserDto existingUser = await container.ReadItemAsync<UserDto>(id, new PartitionKey(id));

                // Soft delete by setting IsActive to false
                existingUser.IsActive = false;
                existingUser.LastUpdatedAt = DateTime.UtcNow;
                await container.UpsertItemAsync(existingUser, new PartitionKey(existingUser.Id!.ToString()));

                // Return wrapped response
                return Ok(new ApiResponse<UserResponseDto>(true, "User deactivated successfully", null));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User {id} not found in Cosmos DB");
                // Return wrapped error response
                return NotFound(new ApiResponse<string>(false, $"User {id} not found", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error deactivating user {id} in Cosmos DB");
                // Return wrapped error response
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deactivating user");
                // Return wrapped error response
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Deletes a user from the system.
        /// </summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <remarks>
        /// Removes the user if found.
        /// </remarks>
        /// <response code="200">User deleted successfully</response>
        /// <response code="404">If the user is not found or container is missing</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> Delete(string id)
        {
            // Added try-catch to handle Cosmos DB exceptions gracefully
            try
            {
                var container = _cosmosDbService.GetContainer("users");

                await container.DeleteItemAsync<UserDto>(id.ToString(), new PartitionKey(id.ToString()));

                // Return wrapped response
                return Ok(new ApiResponse<string>(true, "User deleted successfully", null));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User {id} not found in Cosmos DB");
                // Return wrapped error response
                return NotFound(new ApiResponse<string>(false, $"User {id} not found", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error deleting user {id} from Cosmos DB");
                // Return wrapped error response
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting user");
                // Return wrapped error response
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }
    }
}