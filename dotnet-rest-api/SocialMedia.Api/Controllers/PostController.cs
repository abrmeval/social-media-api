using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using SocialMedia.Api.Interfaces;
using SocialMedia.Api.Models;

namespace SocialMedia.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin, User")]
    public class PostController : ControllerBase
    {
        private readonly ILogger<PostController> _logger;
        private readonly ICosmosDbService _cosmosDbService;

        public PostController(ICosmosDbService cosmosDbService,
         ILogger<PostController> logger)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all posts in the system.
        /// </summary>
        /// <param name="pageSize">Number of posts to return per page (default is 20).</param>
        /// <param name="continuationToken">Token for fetching the next page of results.</param>
        /// <remarks>
        /// Returns a list of all posts wrapped in an ApiResponse.
        /// </remarks>
        /// <response code="200">Returns ApiResponse with the list of posts</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PostDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> GetAllPosts(int pageSize = 20, string? continuationToken = null)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("posts");

                var query = container.GetItemQueryIterator<PostDto>(
                    new QueryDefinition("SELECT * FROM c ORDER BY c.CreatedAt DESC"),
                    requestOptions: new QueryRequestOptions
                    {
                        MaxItemCount = pageSize
                    },
                    continuationToken: continuationToken
                );

                var postList = new List<PostDto>();
                string? newContinuationToken = null;

                if (query.HasMoreResults)
                {
                    var page = await query.ReadNextAsync();
                    postList.AddRange(page);
                    newContinuationToken = page.ContinuationToken;
                }

                return Ok(new ApiResponse<IEnumerable<PostDto>>(true, "Posts retrieved successfully", postList, newContinuationToken));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error retrieving posts from Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving posts");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Retrieves a post by its unique identifier.
        /// </summary>
        /// <param name="id">The post's unique identifier.</param>
        /// <remarks>
        /// Returns the post details wrapped in an ApiResponse if found.
        /// </remarks>
        /// <response code="200">Returns ApiResponse with the post details</response>
        /// <response code="404">If the post is not found or container is missing</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PostDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> GetPost(string id)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("posts");

                // Read the item from Cosmos DB using the provided id
                PostDto response = await container.ReadItemAsync<PostDto>(id, new PartitionKey(id));
                return Ok(new ApiResponse<PostDto>(true, "Post retrieved successfully", response));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Post {id} not found in Cosmos DB");
                return NotFound(new ApiResponse<string>(false, $"Post {id} not found", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error retrieving post {id} from Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error retrieving post {id}");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Creates a new post.
        /// </summary>
        /// <param name="post">The post to create.</param>
        /// <remarks>
        /// Returns the created post wrapped in an ApiResponse.
        /// </remarks>
        /// <response code="201">Returns ApiResponse with the newly created post</response>
        /// <response code="400">If the post data is invalid</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PostDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> CreatePost([FromBody] PostDto post)
        {
            if (post == null)
            {
                return BadRequest(new ApiResponse<string>(false, "Post data is null", null));
            }

            try
            {
                post.Id = Guid.NewGuid().ToString();
                post.CreatedAt = DateTime.UtcNow;

                var container = _cosmosDbService.GetContainer("posts");
                await container.CreateItemAsync(post, new PartitionKey(post.Id));

                return CreatedAtAction(nameof(GetPost), new { id = post.Id }, new ApiResponse<PostDto>(true, "Post created successfully", post));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error creating post in Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating post");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Updates an existing post.
        /// </summary>
        /// <param name="id">The post's unique identifier.</param>
        /// <param name="post">The updated post details.</param>
        /// <remarks>
        /// Updates the post if it exists.
        /// </remarks>
        /// <response code="200">Post updated successfully</response>
        /// <response code="404">If the post is not found or container is missing</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PostDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> UpdatePost(string id, [FromBody] PostDto post)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("posts");

                PostDto existingPost = await container.ReadItemAsync<PostDto>(id, new PartitionKey(id));

                // Update the existing post with the new values
                existingPost.Content = post.Content;
                existingPost.MediaUrl = post.MediaUrl;
                existingPost.LikeCount = post.LikeCount;
                existingPost.CommentCount = post.CommentCount;
                existingPost.LastUpdatedAt = DateTime.UtcNow;

                await container.ReplaceItemAsync(existingPost, existingPost.Id, new PartitionKey(existingPost.Id));

                return Ok(new ApiResponse<PostDto>(true, "Post updated successfully", existingPost));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Post {id} not found in Cosmos DB");
                return NotFound(new ApiResponse<string>(false, $"Post {id} not found", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error updating post {id} in Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error updating post {id}");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Deactivates a post.
        /// </summary>
        /// <param name="id">The post's unique identifier.</param>
        /// <remarks>
        /// Deactivates the post if found.
        /// </remarks>
        /// <response code="200">Post deactivated successfully</response>
        /// <response code="404">If the post is not found or container is missing</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> DeactivatePost(string id)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("posts");

                // Fetch the existing post
                PostDto existingPost = await container.ReadItemAsync<PostDto>(id, new PartitionKey(id));

                existingPost.IsActive = false;
                existingPost.LastUpdatedAt = DateTime.UtcNow;

                // Save the updated post back to Cosmos DB
                await container.ReplaceItemAsync(existingPost, existingPost.Id, new PartitionKey(existingPost.Id));

                return Ok(new ApiResponse<string>(true, "Post deactivated successfully", null));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Post {id} not found in Cosmos DB");
                return NotFound(new ApiResponse<string>(false, $"Post {id} not found", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error deactivating post {id} in Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error deactivating post {id}");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Deletes a post from the system.
        /// </summary>
        /// <param name="id">The post's unique identifier.</param>
        /// <remarks>
        /// Removes the post if found.
        /// </remarks>
        /// <response code="200">Post deleted successfully</response>
        /// <response code="404">If the post is not found or container is missing</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> DeletePost(string id)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("posts");
                await container.DeleteItemAsync<PostDto>(id, new PartitionKey(id));

                return Ok(new ApiResponse<string>(true, "Post deleted successfully", null));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Post {id} not found in Cosmos DB");
                return NotFound(new ApiResponse<string>(false, $"Post {id} not found", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error deleting post {id} from Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error deleting post {id}");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }
    }
}
