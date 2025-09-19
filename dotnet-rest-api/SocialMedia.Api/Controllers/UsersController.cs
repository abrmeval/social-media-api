using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using SocialMedia.Api.Interfaces;
using SocialMedia.Api.Models;

namespace SocialMedia.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly ICosmosDbService _cosmosDbService;

        public UsersController(ILogger<UsersController> logger,
         ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
        }

        /// <summary>
        /// Retrieves all users in the system.
        /// </summary>
        /// <remarks>
        /// Returns a list of all registered users.
        /// </remarks>
        /// <response code="200">Returns the list of users</response>
        /// <response code="404">If the users container is not found</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetAll()
        {
            var container = _cosmosDbService.GetContainer("users");

            if (container == null)
                return NotFound();

            var users = container.GetItemLinqQueryable<UserDto>().ToList();

            return Ok(users);
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
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetById(string id)
        {
            var container = _cosmosDbService.GetContainer("users");

            if (container == null)
                return NotFound();

            var user = container.GetItemLinqQueryable<UserDto>().FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            return Ok(user);
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
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Create([FromBody] UserDto user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            user.Id = Guid.NewGuid().ToString();
            user.RegisteredAt = DateTime.UtcNow;

            try
            {
                var container = _cosmosDbService.GetContainer("users");

                if (container == null)
                    return NotFound();

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
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(string id, [FromBody] UserDto user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // TODO: DB lookup/update
            // If user not found: return NotFound();

            var container = _cosmosDbService.GetContainer("users");

            if (container == null)
                return NotFound();

            var existingUser = container.GetItemLinqQueryable<UserDto>().FirstOrDefault(u => u.Id == id);

            if (existingUser == null)
                return NotFound();

            user.Id = id; // Ensure the ID remains unchanged
            await container.UpsertItemAsync(user, new PartitionKey(user.Id.ToString()));
            
            // Update user logic
            return NoContent();
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
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(string id)
        {
            // TODO: DB lookup/delete
            // If user not found: return NotFound();
            var container = _cosmosDbService.GetContainer("users");

            if (container == null)
                return NotFound();

            var existingUser = container.GetItemLinqQueryable<UserDto>().FirstOrDefault(u => u.Id == id);
            if (existingUser == null)
                return NotFound();

            // Delete user logic
            await container.DeleteItemAsync<UserDto>(id.ToString(), new PartitionKey(id.ToString()));
            return NoContent();
        }
    }
}