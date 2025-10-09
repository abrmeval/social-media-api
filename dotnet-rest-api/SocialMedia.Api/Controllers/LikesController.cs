using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using SocialMedia.Api.Interfaces;
using SocialMedia.Api.Models;

namespace SocialMedia.Api.Controllers
{
    [ApiController]
    [Route("api/posts/{postId}/likes")]
    [Authorize]
    public class LikesController : ControllerBase
    {
        private readonly ILogger<LikesController> _logger;
        private readonly ICosmosDbService _cosmosDbService;

        public LikesController(ILogger<LikesController> logger, ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
        }

        /// <summary>
        /// Retrieves all likes for a specific post.
        /// </summary>
        /// <param name="postId">The post's unique identifier.</param>
        /// <remarks>Returns a list of likes wrapped in an ApiResponse.</remarks>
        /// <response code="200">Returns ApiResponse with the list of likes</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LikeDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> GetLikesForPost(string postId)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("likes");
                var query = container.GetItemQueryIterator<LikeDto>(
                    new QueryDefinition("SELECT * FROM c WHERE c.PostId = @postId ORDER BY c.CreatedAt DESC").WithParameter("@postId", postId)
                );

                var likes = new List<LikeDto>();
                while (query.HasMoreResults)
                {
                    var page = await query.ReadNextAsync();
                    likes.AddRange(page);
                }
                return Ok(new ApiResponse<IEnumerable<LikeDto>>(true, "Likes retrieved successfully", likes));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error retrieving likes from Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving likes");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Likes a post (add a like).
        /// </summary>
        /// <param name="postId">The post's unique identifier.</param>
        /// <remarks>Returns the created like wrapped in an ApiResponse.</remarks>
        /// <response code="201">Returns ApiResponse with the created like</response>
        /// <response code="400">If the user already liked the post</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LikeDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> LikePost(string postId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var container = _cosmosDbService.GetContainer("likes");

                // Prevent duplicate likes by same user for same post
                var query = container.GetItemQueryIterator<LikeDto>(
                    new QueryDefinition("SELECT * FROM c WHERE c.PostId = @postId AND c.UserId = @userId")
                        .WithParameter("@postId", postId)
                        .WithParameter("@userId", userId)
                );
                LikeDto? existingLike = null;
                while (query.HasMoreResults)
                {
                    var page = await query.ReadNextAsync();
                    existingLike = page.FirstOrDefault();
                    if (existingLike != null) break;
                }
                if (existingLike != null)
                    return BadRequest(new ApiResponse<string>(false, "You already liked this post.", null));

                var like = new LikeDto
                {
                    Id = Guid.NewGuid().ToString(),
                    PostId = postId,
                    AuthorId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await container.CreateItemAsync(like, new PartitionKey(like.Id));
                return CreatedAtAction(nameof(GetLikesForPost), new { postId = postId }, new ApiResponse<LikeDto>(true, "Post liked successfully", like));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error liking post in Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error liking post");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Unlikes a post (remove a like).
        /// </summary>
        /// <param name="postId">The post's unique identifier.</param>
        /// <param name="likeId">The like's unique identifier.</param>
        /// <remarks>Returns ApiResponse indicating success or error.</remarks>
        /// <response code="200">Like removed successfully</response>
        /// <response code="404">If the like is not found</response>
        /// <response code="403">If the user is not the author of the like</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpDelete("{likeId}")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 403)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> UnlikePost(string postId, string likeId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var container = _cosmosDbService.GetContainer("likes");

                var query = container.GetItemQueryIterator<LikeDto>(
                    new QueryDefinition("SELECT * FROM c WHERE c.id = @id AND c.PostId = @postId")
                        .WithParameter("@id", likeId)
                        .WithParameter("@postId", postId)
                );
                LikeDto? like = null;
                while (query.HasMoreResults)
                {
                    var page = await query.ReadNextAsync();
                    like = page.FirstOrDefault();
                    if (like != null) break;
                }

                if (like == null)
                    return NotFound(new ApiResponse<string>(false, "Like not found", null));

                if (like.AuthorId != userId)
                    return StatusCode(403, new ApiResponse<string>(false, "You are not the author of this like", null));

                await container.DeleteItemAsync<LikeDto>(likeId, new PartitionKey(likeId));
                return Ok(new ApiResponse<string>(true, "Like removed successfully", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error unliking post in Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error unliking post");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }


        /// <summary>
        /// Deactivates a like instead of deleting it.
        /// </summary>
        /// <param name="postId">The ID of the post.</param>
        /// <param name="likeId">The ID of the like.</param>
        /// <response code="200">Like deactivated successfully</response>
        /// <response code="404">If the like is not found</response>
        /// <response code="403">If the user is not the author of the like</response>
        /// <response code="500">If there is an internal server error</response>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{likeId}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 403)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> DeactivateLike(string postId, string likeId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var container = _cosmosDbService.GetContainer("likes");

                var query = container.GetItemQueryIterator<LikeDto>(
                    new QueryDefinition("SELECT * FROM c WHERE c.id = @id AND c.PostId = @postId")
                        .WithParameter("@id", likeId)
                        .WithParameter("@postId", postId)
                );
                LikeDto? like = null;
                while (query.HasMoreResults)
                {
                    var page = await query.ReadNextAsync();
                    like = page.FirstOrDefault();
                    if (like != null) break;
                }

                if (like == null)
                    return NotFound(new ApiResponse<string>(false, "Like not found", null));

                if (like.AuthorId != userId)
                    return StatusCode(403, new ApiResponse<string>(false, "You are not the author of this like", null));

                like.IsActive = false;
                await container.ReplaceItemAsync(like, like.Id, new PartitionKey(like.Id));
                return Ok(new ApiResponse<string>(true, "Like deactivated successfully", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error deactivating like in Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deactivating like");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }
    }
}