using Microsoft.Azure.Cosmos;
using SocialMedia.Api.Interfaces;

namespace SocialMedia.Api.Services
{
    /// <summary>
    /// Service for interacting with Azure Cosmos DB.
    /// </summary>
    public class CosmosDbService : ICosmosDbService
    {
        public CosmosClient Client { get; }
        public Database Database { get; }

        /// <summary>
        /// Constructor to initialize Cosmos DB client and database.
        /// </summary>
        /// <param name="config"></param>
        public CosmosDbService(IConfiguration config)
        {
            var endpoint = config["CosmosDb:AccountEndpoint"];
            var key = config["CosmosDb:AccountKey"];
            var dbName = config["CosmosDb:DatabaseName"];

            Client = new CosmosClient(endpoint, key);
            Database = Client.GetDatabase(dbName);
        }

        /// <summary>
        /// Gets the Cosmos DB container by name.
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public Container GetContainer(string containerName)
        {
            return Database.GetContainer(containerName);
        }
    }
}