using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using SocialMedia.Api.Interfaces;
using SocialMedia.Api.Models;

namespace SocialMedia.Api.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly ICosmosDbService _cosmosDbService;

        public ProfileController(ILogger<ProfileController> logger, ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
        }

        /// <summary>
        /// Get the current user's profile, including followed users.
        /// </summary>
        /// <returns> The current user's profile information.</returns>
        /// <response code="200">Returns the user's profile information.</response>
        /// <response code="401">Unauthorized if the user is not logged in.</response>  
        /// <response code="404">Not Found if the user does not exist.</response>
        /// <response code="500">Internal Server Error if there is an unexpected error.</response>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserDto), 200)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse<string>(false, "Unauthorized", null));

                var usersContainer = _cosmosDbService.GetContainer("users");
                UserDto user = await usersContainer.ReadItemAsync<UserDto>(userId, new PartitionKey(userId));
                return Ok(new ApiResponse<UserDto>(true, "Profile retrieved successfully", user));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User {userId} not found in Cosmos DB");
                // Return wrapped error response
                return NotFound(new ApiResponse<string>(false, $"User {userId} not found", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Error retrieving user {userId}: {ex.Message}");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user {userId}: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Follow another user.
        /// </summary>
        /// <param name="id">The user ID to follow.</param>
        /// <remarks>Only logged-in users can follow other users.</remarks>
        /// <response code="200">Returns a success message.</response>
        /// <response code="401">Unauthorized if the user is not logged in.</response>
        /// <response code="404">Not Found if the user does not exist.</response>
        /// <response code="500">Internal Server Error if there is an unexpected error.</response>
        [HttpPost("{id}/follow")]
        public async Task<IActionResult> FollowUser(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse<string>(false, "Unauthorized", null));

                var usersContainer = _cosmosDbService.GetContainer("users");
                UserDto currentUser = await usersContainer.ReadItemAsync<UserDto>(userId, new PartitionKey(userId));

                currentUser.Following ??= [];

                if (!currentUser.Following.Contains(id))
                {
                    currentUser.Following.Add(id);
                    await usersContainer.UpsertItemAsync(currentUser, new PartitionKey(currentUser.Id));
                }
                return Ok(new ApiResponse<string>(true, "User followed successfully", null));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User {userId} not found in Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Error following user {id} by {userId}: {ex.Message}");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error following user {id} by {userId}: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Unfollow another user.
        /// </summary>
        /// <param name="id">The user ID to unfollow.</param>
        /// <remarks>Only logged-in users can unfollow other users.</remarks>
        /// <response code="200">Returns a success message.</response>
        /// <response code="401">Unauthorized if the user is not logged in.</response>
        /// <response code="404">Not Found if the user does not exist.</response>
        /// <response code="500">Internal Server Error if there is an unexpected error.</response>
        [HttpPost("{id}/unfollow")]
        public async Task<IActionResult> UnfollowUser(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            try
            {
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse<string>(false, "Unauthorized", null));

                var usersContainer = _cosmosDbService.GetContainer("users");
                UserDto currentUser = await usersContainer.ReadItemAsync<UserDto>(userId, new PartitionKey(userId));

                if (currentUser.Following != null && currentUser.Following.Contains(id))
                {
                    currentUser.Following.Remove(id);
                    await usersContainer.UpsertItemAsync(currentUser, new PartitionKey(currentUser.Id));
                }
                return Ok(new ApiResponse<string>(true, "User unfollowed successfully", null));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User {userId} not found in Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Error unfollowing user {id} by {userId}: {ex.Message}");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error unfollowing user {id} by {userId}: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Get the feed: posts from the current user and those they follow.
        /// </summary>
        /// <remarks>Only logged-in users can access their feed.</remarks>
        /// <response code="200">Returns the user's feed.</response>
        /// <response code="401">Unauthorized if the user is not logged in.</response>
        /// <response code="404">Not Found if the user does not exist.</response>
        /// <response code="500">Internal Server Error if there is an unexpected error.</response>
        [HttpGet("feed")]
        [ProducesResponseType(typeof(IEnumerable<PostDto>), 200)]
        public async Task<IActionResult> GetFeed()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            try
            {
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse<string>(false, "Unauthorized", null));

                var usersContainer = _cosmosDbService.GetContainer("users");
                var postsContainer = _cosmosDbService.GetContainer("posts");

                UserDto currentUser = await usersContainer.ReadItemAsync<UserDto>(userId, new PartitionKey(userId));
                if (currentUser == null)
                    return NotFound(new ApiResponse<string>(false, "Current user not found.", null));

                var authorIds = new List<string> { userId };
                if (currentUser.Following != null)
                    authorIds.AddRange(currentUser.Following);

                var query = postsContainer.GetItemQueryIterator<PostDto>(
                                new QueryDefinition("SELECT * FROM c WHERE ARRAY_CONTAINS(@authorIds, c.authorId) ORDER BY c.createdAt DESC")
                                    .WithParameter("@authorIds", authorIds));

                var posts = new List<PostDto>();
                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    posts.AddRange(response);
                }
                return Ok(new ApiResponse<IEnumerable<PostDto>>(true, "Feed retrieved successfully", posts));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User {userId} not found in Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Error retrieving feed for user {userId}: {ex.Message}");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving feed for user {userId}: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }
    }
}