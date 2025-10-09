
using SocialMedia.Api.Models;

namespace SocialMedia.Api.Interfaces
{
    public interface IBlobStorageService
    {
        /// <summary>
        /// Uploads a file to the specified container in Blob Storage.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        Task<BlobFileResponse> UploadFileAsync(IFormFile file, string containerName);

        /// <summary>
        /// Downloads a file from the specified container in Blob Storage.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        Task<BlobFileResponse> DownloadFileAsync(string fileName, string containerName);

        /// <summary>
        /// Deletes a file from the specified container in Blob Storage.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        Task<BlobFileResponse> DeleteFileAsync(string fileName, string containerName);
    }
}