using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using SocialMedia.Api.Interfaces;
using SocialMedia.Api.Models;

namespace SocialMedia.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ILogger<CommentsController> _logger;
        private readonly ICosmosDbService _cosmosDbService;

        public CommentsController(ILogger<CommentsController> logger, ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
        }

        /// <summary>
        /// Retrieves all comments for a specific post with pagination.
        /// </summary>
        /// <param name="postId">The post's unique identifier.</param>
        /// <param name="pageSize">Number of comments per page (default: 20).</param>
        /// <param name="continuationToken">Token for fetching the next page of results.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <remarks>Returns a paginated list of comments wrapped in an ApiResponse.</remarks>
        /// <response code="200">Returns ApiResponse with the list of comments</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet("post/{postId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CommentDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> GetCommentsForPost(string postId, int pageSize = 20, string? continuationToken = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("comments");

                var query = container.GetItemQueryIterator<CommentDto>(
                    new QueryDefinition("SELECT * FROM c WHERE c.PostId = @postId ORDER BY c.CreatedAt DESC").WithParameter("@postId", postId),
                    requestOptions: new QueryRequestOptions
                    {
                        MaxItemCount = pageSize
                    },
                    continuationToken: continuationToken
                );

                var commentList = new List<CommentDto>();
                string? nextContinuationToken = null;

                while (query.HasMoreResults)
                {
                    var page = await query.ReadNextAsync(cancellationToken);
                    commentList.AddRange(page);
                    nextContinuationToken = page.ContinuationToken;
                    break; // Only fetch one page per request
                }
                return Ok(new ApiResponse<IEnumerable<CommentDto>>(true, "Comments retrieved successfully", commentList, nextContinuationToken));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error retrieving comments from Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving comments");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Retrieves a comment by its unique identifier.
        /// </summary>
        /// <param name="id">Comment ID.</param>
        /// <remarks>Returns the comment wrapped in an ApiResponse if found.</remarks>
        /// <response code="200">Returns ApiResponse with the comment</response>
        /// <response code="404">If the comment is not found</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CommentDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("comments");

                //Read item by id and partition key (assuming id is the partition key)
                CommentDto comment = await container.ReadItemAsync<CommentDto>(id, new PartitionKey(id));
                return Ok(new ApiResponse<CommentDto>(true, "Comment retrieved successfully", comment));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound(new ApiResponse<string>(false, $"Comment {id} not found", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error retrieving comment {id} from Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving comment");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Creates a new comment for a post.
        /// </summary>
        /// <param name="comment">Comment data.</param>
        /// <remarks>Returns the created comment wrapped in an ApiResponse.</remarks>
        /// <response code="201">Returns ApiResponse with the created comment</response>
        /// <response code="400">If the comment data is invalid</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CommentDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> Create([FromBody] CommentDto comment)
        {
            if (comment == null)
                return BadRequest(new ApiResponse<string>(false, "Invalid comment data", null));
            try
            {
                comment.Id = Guid.NewGuid().ToString();
                comment.CreatedAt = DateTime.UtcNow;

                var container = _cosmosDbService.GetContainer("comments");
                await container.CreateItemAsync(comment, new PartitionKey(comment.Id));

                return CreatedAtAction(nameof(GetById), new { id = comment.Id }, new ApiResponse<CommentDto>(true, "Comment created successfully", comment));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error creating comment in Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating comment");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Updates an existing comment.
        /// </summary>
        /// <param name="id">Comment ID.</param>
        /// <param name="comment">Updated comment data.</param>
        /// <remarks>Returns the updated comment wrapped in an ApiResponse.</remarks>
        /// <response code="200">Returns ApiResponse with the updated comment</response>
        /// <response code="400">If the comment data is invalid</response>
        /// <response code="404">If the comment is not found</response>
        /// <response code="403">If the user is not the author</response>
        /// <response code="500">If there is an internal server error</response>
        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CommentDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 403)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> Update(string id, [FromBody] CommentDto comment)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("comments");
                CommentDto existingComment = await container.ReadItemAsync<CommentDto>(id, new PartitionKey(id));

                if (existingComment.AuthorId != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
                    return StatusCode(403, new ApiResponse<string>(false, "You are not the author of this comment", null));

                // Update the existing comment with the new values    
                existingComment.Content = comment.Content;
                existingComment.LastUpdatedAt = DateTime.UtcNow;

                await container.UpsertItemAsync(existingComment, new PartitionKey(existingComment.Id));
                return Ok(new ApiResponse<CommentDto>(true, "Comment updated successfully", existingComment));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound(new ApiResponse<string>(false, $"Comment {id} not found", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error updating comment {id} in Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating comment");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Deactivates a comment.
        /// </summary>
        /// <param name="id">The comment's unique identifier.</param>
        /// <remarks>
        /// Deactivates the comment if found.
        /// </remarks>
        /// <response code="200">Comment deactivated successfully</response>
        /// <response code="404">If the comment is not found or container is missing</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> DeactivateComment(string id)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("comments");

                // Fetch the existing comment
                CommentDto existingComment = await container.ReadItemAsync<CommentDto>(id, new PartitionKey(id));

                existingComment.IsActive = false;
                existingComment.LastUpdatedAt = DateTime.UtcNow;

                // Save the updated comment back to Cosmos DB
                await container.ReplaceItemAsync(existingComment, existingComment.Id, new PartitionKey(existingComment.Id));

                return Ok(new ApiResponse<string>(true, "Comment deactivated successfully", null));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Comment {id} not found in Cosmos DB");
                return NotFound(new ApiResponse<string>(false, $"Comment {id} not found", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error deactivating comment {id} in Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error deactivating comment {id}");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Deletes a comment.
        /// </summary>
        /// <param name="id">Comment ID.</param>
        /// <remarks>Returns ApiResponse indicating success or error.</remarks>
        /// <response code="200">Comment deleted successfully</response>
        /// <response code="404">If the comment is not found</response>
        /// <response code="403">If the user is not the author</response>
        /// <response code="500">If there is an internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("comments");
                await container.DeleteItemAsync<CommentDto>(id, new PartitionKey(id));

                return Ok(new ApiResponse<string>(true, "Comment deleted successfully", null));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error deleting comment {id} from Cosmos DB");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting comment");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }
    }
}