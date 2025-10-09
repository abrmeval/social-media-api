using System.ComponentModel.DataAnnotations;

namespace SocialMedia.Api.Models
{

    /// <summary>
    /// DTO for media upload requests.
    /// </summary>
    public class MediaRequestDto
    {
        public string? PostId { get; set; }
        
        [Required]
        public string FileCategory { get; set; } = "POSTDOCUMENT";

        [Required]
        [FileExtensions(Extensions = "jpg,jpeg,png,gif,bmp,mp4,mov,avi,wmv,doc,docx,pdf,txt", ErrorMessage = "Invalid file type.")]
        [File(10485760, ErrorMessage = "Document must be less than or equal to 10MB.")]
        public IFormFile? File { get; set; }
    }
}