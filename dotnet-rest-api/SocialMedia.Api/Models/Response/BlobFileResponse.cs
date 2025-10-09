namespace SocialMedia.Api.Models
{

    /// <summary>
    /// Response model for files retrieved from Blob Storage.
    /// </summary>
    public class BlobFileResponse
    {
        /// <summary>
        /// Name of the file.
        /// </summary>
        public string? FileName { get; init; }

        /// <summary>
        /// Name of the blob in storage.
        /// </summary>
        public string? BlobName { get; init; }

        /// <summary>
        /// URL of the file in Blob Storage.
        /// </summary>
        public string? FileUrl { get; init; }

        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        public long FileSize { get; init; }

        /// <summary>
        /// File extension (e.g., .jpg, .png, .pdf).
        /// </summary>
        public string? FileExtension { get; init; }

        /// <summary>
        /// File content as a byte array.
        /// </summary>
        public byte[]? Content { get; init; }

        /// <summary>
        /// MIME type of the file.
        /// </summary>
        public string? ContentType { get; init; }


        /// <summary>
        /// Indicates if the file retrieval was successful.
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Indicates the response message
        /// </summary>
        public string? Message { get; init; }
    }
}