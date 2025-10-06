using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        /// Gets all posts.
        /// </summary>
        /// <response code="200">Returns the list of posts</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PostDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllPosts()
        {
            try
            {
                // Added try-catch to handle Cosmos DB exceptions gracefully
                var container = _cosmosDbService.GetContainer("posts");
                var posts = container.GetItemQueryIterator<PostDto>();
                var results = new List<PostDto>();
                while (posts.HasMoreResults)
                {
                    var response = await posts.ReadNextAsync();
                    results.AddRange(response);
                }
                return Ok(new ApiResponse<IEnumerable<PostDto>>(true, "Posts retrieved successfully", results));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error fetching posts from Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching posts");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Gets a specific post by id.
        /// </summary>
        /// <param name="id">The id of the post to retrieve.</param>
        /// <response code="200">Returns the requested post</response>
        /// <response code="404">If the post is not found</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PostDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
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
                return NotFound(new ApiResponse<string>(false, $"Post {id} not found", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error fetching post from Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching post");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        [HttpPost]
        public IActionResult CreatePost([FromBody] PostDto post)
        {
            if (post == null)
            {
                return BadRequest("Post data is null");
            }

            post.Id = Guid.NewGuid().ToString();
            post.CreatedAt = DateTime.UtcNow;

            var container = _cosmosDbService.GetContainer("posts");
            container.CreateItemAsync(post, new PartitionKey(post.Id));

            // Return wrapped response
            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, new ApiResponse<PostDto>(true, "Post created successfully", post));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(string id, [FromBody] PostDto post)
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

            // Return wrapped response
            return Ok(new ApiResponse<PostDto>(true, "Post updated successfully", existingPost));
        }

        public IActionResult DeactivatePost(string id)
        {
            var container = _cosmosDbService.GetContainer("posts");

            // Fetch the existing post
            PostDto existingPost = container.ReadItemAsync<PostDto>(id, new PartitionKey(id)).Result;

            existingPost.IsActive = false;
            existingPost.LastUpdatedAt = DateTime.UtcNow;

            // Save the updated post back to Cosmos DB
            container.ReplaceItemAsync(existingPost, existingPost.Id, new PartitionKey(existingPost.Id)).Wait();

            // Return wrapped response
            return Ok(new ApiResponse<PostDto>(true, "Post deactivated successfully", existingPost));
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePost(string id)
        {
            var container = _cosmosDbService.GetContainer("posts");
            container.DeleteItemAsync<PostDto>(id, new PartitionKey(id));

            // Return wrapped response
            return Ok(new ApiResponse<string>(true, "Post deleted successfully", null));
        }
    }
}
