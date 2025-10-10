using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using SocialMedia.Api.Interfaces;
using SocialMedia.Api.Models;
using Azure.Storage.Blobs;
using SocialMedia.Api.Common;

namespace SocialMedia.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MediaController : ControllerBase
    {
        private readonly ILogger<MediaController> _logger;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;


        public MediaController(
            ILogger<MediaController> logger,
            ICosmosDbService cosmosDbService,
            BlobServiceClient blobServiceClient,
            IBlobStorageService blobStorageService,
            IConfiguration configuration)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
            _blobStorageService = blobStorageService;
            _configuration = configuration;
        }

        /// <summary>
        /// Upload a media file (image/video) to Azure Blob Storage.
        /// </summary>
        /// <param name="media">The media file information.</param>
        /// <remarks>Returns the uploaded media file information wrapped in an ApiResponse.</remarks>
        /// <response code="201">Returns ApiResponse with the uploaded media file information</response>
        /// <response code="400">If the file is null or invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(ApiResponse<MediaDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 401)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> Upload([FromForm] MediaRequestDto media)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse<string>(false, "Unauthorized", null));

                string? containerName = media.FileCategory switch
                {
                    AppConstants.ProfileImage
                    or AppConstants.PostImage => _configuration["BlobStorage:RawImageContainerName"],
                    AppConstants.PostVideo => _configuration["BlobStorage:VideoContainerName"],
                    AppConstants.PostDocument => _configuration["BlobStorage:DocumentContainerName"],
                    _ => _configuration["BlobStorage:OtherContainerName"]
                };

                if (media.File == null)
                    return BadRequest(new ApiResponse<string>(false, "File is required.", null));

                // Upload file to Blob Storage
                var blobFileResponse = await _blobStorageService.UploadFileAsync(media.File, containerName!);

                if (!blobFileResponse.Success)
                {
                    _logger.LogError("Error uploading file to Blob Storage: {Message}", blobFileResponse.Message);
                    return StatusCode(500, new ApiResponse<string>(false, "Error uploading file to Blob Storage", null));
                }

                // Save media metadata to Cosmos DB
                var newMedia = new MediaDto
                {
                    Id = Guid.NewGuid().ToString(),
                    FileName = media.File.FileName,
                    BlobUrl = blobFileResponse.FileUrl,
                    AuthorId = userId,
                    PostId = media.PostId,
                    FileCategory = media.FileCategory,
                    UploadedAt = DateTime.UtcNow
                };

                var container = _cosmosDbService.GetContainer("media");
                await container.CreateItemAsync(newMedia, new PartitionKey(newMedia.Id));

                return CreatedAtAction(nameof(GetById), new { mediaId = newMedia.Id }, new ApiResponse<MediaDto>(true, "File uploaded successfully", newMedia));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Cosmos DB error while uploading media file.");
                return StatusCode((int)ex.StatusCode, new ApiResponse<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading media file.");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Download a media file by its ID.
        /// </summary>
        /// <param name="mediaId">The ID of the media file to download.</param>
        /// <remarks>Returns the media file as a FileResult if found, otherwise returns an ApiResponse with an error message.</remarks>
        /// <response code="200">Returns the media file as a FileResult</response>
        /// <response code="404">If the media file is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{mediaId}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> GetById(string mediaId)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("media");

                MediaDto media = await container.ReadItemAsync<MediaDto>(mediaId, new PartitionKey(mediaId));
                var blobFileResponse = await _blobStorageService.DownloadFileAsync(media.FileName!, media.ContainerName!);

                if (blobFileResponse.Success && blobFileResponse.Content != null)
                {
                    if (blobFileResponse.ContentType != null)
                        return File(blobFileResponse.Content, blobFileResponse.ContentType, media.FileName);
                    else
                        return File(blobFileResponse.Content, "application/octet-stream", media.FileName);

                }
                _logger.LogWarning("File not found in Blob Storage: {Message}", blobFileResponse.Message);
                return NotFound(new ApiResponse<string>(false, "File not found in Blob Storage", null));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning(ex, "Media not found");
                return NotFound(new ApiResponse<string>(false, "Media not found", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading media file.");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }

        /// <summary>
        /// Delete a media file by its ID.
        /// </summary>
        /// <param name="mediaId">The ID of the media file to delete.</param>
        /// <remarks>Deletes the media file from both Azure Blob Storage and Cosmos DB if the user is the author of the media.</remarks>
        /// <response code="200">If the media file was deleted successfully</response>
        /// <response code="403">If the user is not the author of the media</response>
        /// <response code="404">If the media file is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("{mediaId}")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 403)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> Delete(string mediaId)
        {
            try
            {
                var container = _cosmosDbService.GetContainer("media");
                MediaDto media = await container.ReadItemAsync<MediaDto>(mediaId, new PartitionKey(mediaId));

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse<string>(false, "Unauthorized", null));
                    
                if (media.AuthorId != userId)
                    return StatusCode(403, new ApiResponse<string>(false, "You are not the author of this media", null));

                var blobFileResponse = await _blobStorageService.DeleteFileAsync(media.FileName!, media.ContainerName!);

                if (!blobFileResponse.Success)
                {
                    _logger.LogError("Error deleting file from Blob Storage: {Message}", blobFileResponse.Message);
                    return StatusCode(500, new ApiResponse<string>(false, "Error deleting file from Blob Storage", null));
                }

                await container.DeleteItemAsync<MediaDto>(mediaId, new PartitionKey(mediaId));
                return Ok(new ApiResponse<string>(true, "Media deleted successfully", null));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning(ex, "Media not found");
                return NotFound(new ApiResponse<string>(false, "Media not found", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting media file.");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error", null));
            }
        }
    }
}