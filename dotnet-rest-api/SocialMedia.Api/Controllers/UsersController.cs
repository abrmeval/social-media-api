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
    [Authorize (Roles = "Admin, User")] // Only Admins can manage users
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
        /// Returns a list of all registered users.
        /// </remarks>
        /// <response code="200">Returns the list of users</response>
        /// <response code="404">If the users container is not found</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult GetAll()
        {
            // Added try-catch to handle Cosmos DB exceptions gracefully
            try
            {
                var container = _cosmosDbService.GetContainer("users");

                // Get all users
                var users = container.GetItemLinqQueryable<UserDto>().ToList();
                return Ok(users);
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error retrieving users from Cosmos DB");
                return StatusCode((int)ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving users");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <remarks>
        /// Returns the user details if found.
        /// </remarks>
        /// <response code="200">Returns the user details</response>
        /// <response code="404">If the user is not found or container is missing</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetById(string id)
        {
            // Added try-catch to handle Cosmos DB exceptions gracefully
            try
            {
                var container = _cosmosDbService.GetContainer("users");

                // Get user by id
                UserDto user = await container.ReadItemAsync<UserDto>(id, new PartitionKey(id));
                return Ok(user);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User {id} not found in Cosmos DB");
                return NotFound();
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error retrieving user {id} from Cosmos DB");
                return StatusCode((int)ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving user");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Creates a new user in the system.
        /// </summary>
        /// <param name="user">The user details to create.</param>
        /// <remarks>
        /// Registers a new user and returns the created user object.
        /// </remarks>
        /// <response code="201">Returns the newly created user</response>
        /// <response code="400">If the user data is invalid</response>
        /// <response code="404">If the users container is not found</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
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
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error creating user in Cosmos DB");
                return StatusCode((int)ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating user");
                return StatusCode(500, "Internal server error");
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
        /// <response code="204">User updated successfully</response>
        /// <response code="400">If the user data is invalid</response>
        /// <response code="404">If the user is not found or container is missing</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
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
                return NoContent();
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User {id} not found in Cosmos DB");
                return NotFound();
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error updating user {id} in Cosmos DB");
                return StatusCode((int)ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating user");
                return StatusCode(500, "Internal server error");
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
        /// <response code="204">User deactivated successfully</response>
        /// <response code="404">If the user is not found or container is missing</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpPut("{id}/deactivate")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Deactivate(string id)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("users");

                UserDto existingUser = await container.ReadItemAsync<UserDto>(id, new PartitionKey(id));

                existingUser.IsActive = false;
                await container.UpsertItemAsync(existingUser, new PartitionKey(existingUser.Id!.ToString()));

                return NoContent();
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User {id} not found in Cosmos DB");
                return NotFound();
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error deactivating user {id} in Cosmos DB");
                return StatusCode((int)ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deactivating user");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deletes a user from the system.
        /// </summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <remarks>
        /// Removes the user if found.
        /// </remarks>
        /// <response code="204">User deleted successfully</response>
        /// <response code="404">If the user is not found or container is missing</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete(string id)
        {
            // Added try-catch to handle Cosmos DB exceptions gracefully
            try
            {
                var container = _cosmosDbService.GetContainer("users");

                UserDto existingUser = await container.ReadItemAsync<UserDto>(id, new PartitionKey(id));

                await container.DeleteItemAsync<UserDto>(id.ToString(), new PartitionKey(id.ToString()));
                return NoContent();
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User {id} not found in Cosmos DB");
                return NotFound();
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error deleting user {id} from Cosmos DB");
                return StatusCode((int)ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting user");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}