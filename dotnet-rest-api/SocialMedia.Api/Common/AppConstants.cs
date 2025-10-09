namespace SocialMedia.Api.Common
{
    /// <summary>
    /// Application-wide constants.
    /// </summary>
    public static class AppConstants
    {
        public const string DefaultRole = "User";
        public const string AdminRole = "Admin";
        public const int MaxPageSize = 100;

        // File categories
        public const string ProfileImage = "PROFILEIMAGE";
        public const string PostImage = "POSTIMAGE";
        public const string PostVideo = "POSTVIDEO";
        public const string PostDocument = "POSTDOCUMENT";
        public const string Other = "OTHER";
    }
}