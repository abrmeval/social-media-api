using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SocialMedia.Api.Interfaces;
using SocialMedia.Api.Models;

namespace SocialMedia.Api.Services
{
    /// <summary>
    /// Service for interacting with Azure Blob Storage.
    /// </summary>
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _client;
        public BlobStorageService(IConfiguration config, BlobServiceClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Uploads a file to the specified container in Blob Storage. 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public async Task<BlobFileResponse> UploadFileAsync(IFormFile file, string containerName)
        {
            try
            {
                var container = _client.GetBlobContainerClient(containerName);
                await container.CreateIfNotExistsAsync();

                var blob = container.GetBlobClient(file.FileName);
                await blob.UploadAsync(file.OpenReadStream(), new BlobHttpHeaders { ContentType = file.ContentType });


                return new BlobFileResponse
                {
                    BlobNameOnly = Path.GetFileNameWithoutExtension(file.FileName),
                    BlobName = Path.GetFileName(file.FileName),
                    BlobFullName = blob.Name,
                    Url = blob.Uri.ToString(),
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new BlobFileResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Downloads a file from Azure Blob Storage and returns its metadata and content as bytes.
        /// </summary>
        /// <param name="blobName">The name of the blob to download.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>BlobFileResponse with file metadata and content.</returns>
        public async Task<BlobFileResponse> DownloadFileAsync(string blobName, string containerName)
        {
            try
            {
                var container = _client.GetBlobContainerClient(containerName);
                var blob = container.GetBlobClient(blobName);

                var downloadInfo = await blob.DownloadAsync();
                using var ms = new MemoryStream();

                await downloadInfo.Value.Content.CopyToAsync(ms);

                return new BlobFileResponse
                {
                    BlobNameOnly = Path.GetFileNameWithoutExtension(blobName),
                    BlobName = Path.GetFileName(blobName),
                    BlobFullName = blobName,
                    Url = blob.Uri.ToString(),
                    Size = ms.Length,
                    Content = ms.ToArray(),
                    Success = true,
                    Extension = Path.GetExtension(blobName),
                    ContentType = downloadInfo.Value.ContentType,
                };
            }
            catch (Exception ex)
            {
                return new BlobFileResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Deletes a file from the blob storage.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<BlobFileResponse> DeleteFileAsync(string fileName, string containerName)
        {
            try
            {
                var container = _client.GetBlobContainerClient(containerName);
                var blob = container.GetBlobClient(fileName);
                await blob.DeleteIfExistsAsync();
                return new BlobFileResponse
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new BlobFileResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}