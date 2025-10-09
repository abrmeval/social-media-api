namespace SocialMedia.Api.Models
{ 
    /// <summary>
    /// Standard API response wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Success"></param>
    /// <param name="Message"></param>
    /// <param name="Data"></param>
    public record ApiResponse<T>(
        /// <summary>Indicates if the API call was successful.</summary>
        bool Success,
        /// <summary>Message providing additional information about the API call.</summary>
        string? Message,
        /// <summary>The data returned by the API call.</summary>
        T? Data = default,
        /// <summary>Token for fetching the next page of results, if applicable.</summary>
        string? ContinuationToken = null
    );
    
}