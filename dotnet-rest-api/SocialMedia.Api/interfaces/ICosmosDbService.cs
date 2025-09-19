using Microsoft.Azure.Cosmos;

namespace SocialMedia.Api.Interfaces
{ 
    /// <summary>
    /// Interface for Cosmos DB service.
    /// </summary>
    public interface ICosmosDbService
    {
        /// <summary>
        /// Gets the Cosmos DB container by name.
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        Container GetContainer(string containerName);
    }
}
