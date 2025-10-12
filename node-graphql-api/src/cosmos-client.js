require('dotenv').config();
const { CosmosClient } = require('@azure/cosmos');

const client = new CosmosClient({
  endpoint: process.env.COSMOS_ENDPOINT,
  key: process.env.COSMOS_KEY
});
const database = client.database(process.env.COSMOS_DATABASE);

/// Export the client and database for use in other modules
module.exports = { client, database };