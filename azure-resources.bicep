// Azure Resources for Social Media API Project
// This Bicep file provisions Cosmos DB, Blob Storage, and a B1 App Service Plan for backend deployment.

@description('Name for the Cosmos DB account.')
param cosmosDbAccountName string = 'socialmedia-cosmosdb'

@description('Name for the Cosmos DB database.')
param cosmosDbDatabaseName string = 'socialmedia-db'

@description('Name for the Blob Storage account.')
param storageAccountName string = 'socialmediastorage${uniqueString(resourceGroup().id)}'

@description('Location for resources.')
param location string = resourceGroup().location

@description('App Service Plan name.')
param appServicePlanName string = 'socialmedia-appserviceplan'

@description('SKU for Cosmos DB.')
param cosmosDbSku string = 'Standard'

@description('SKU for Storage Account.')
param storageSku string = 'Standard_LRS'

@description('SKU for App Service Plan.')
param appServicePlanSku string = 'B1' // B1 is the Basic tier

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: cosmosDbAccountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: cosmosDbSku
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
  }
}

resource cosmosDbDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-04-15' = {
  name: '${cosmosDbAccount.name}/${cosmosDbDatabaseName}'
  properties: {
    resource: {
      id: cosmosDbDatabaseName
    }
  }
  dependsOn: [
    cosmosDbAccount
  ]
}

// Example containers for users, posts, comments, likes
resource cosmosDbUsers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  name: '${cosmosDbAccount.name}/${cosmosDbDatabaseName}/users'
  properties: {
    resource: {
      id: 'users'
      partitionKey: {
        paths: ['/id']
        kind: 'Hash'
      }
    }
  }
  dependsOn: [cosmosDbDatabase]
}

resource cosmosDbPosts 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  name: '${cosmosDbAccount.name}/${cosmosDbDatabaseName}/posts'
  properties: {
    resource: {
      id: 'posts'
      partitionKey: {
        paths: ['/id']
        kind: 'Hash'
      }
    }
  }
  dependsOn: [cosmosDbDatabase]
}

resource cosmosDbComments 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  name: '${cosmosDbAccount.name}/${cosmosDbDatabaseName}/comments'
  properties: {
    resource: {
      id: 'comments'
      partitionKey: {
        paths: ['/id']
        kind: 'Hash'
      }
    }
  }
  dependsOn: [cosmosDbDatabase]
}

resource cosmosDbLikes 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  name: '${cosmosDbAccount.name}/${cosmosDbDatabaseName}/likes'
  properties: {
    resource: {
      id: 'likes'
      partitionKey: {
        paths: ['/id']
        kind: 'Hash'
      }
    }
  }
  dependsOn: [cosmosDbDatabase]
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: storageSku
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
  }
}

resource rawImagesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccount.name}/default/raw-images'
  properties: {
    publicAccess: 'None'
  }
  dependsOn: [storageAccount]
}

resource processedImagesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccount.name}/default/processed-images'
  properties: {
    publicAccess: 'None'
  }
  dependsOn: [storageAccount]
}

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSku // B1 Basic tier
    tier: 'Basic'
  }
  properties: {
    reserved: false
  }
}

// Add your App Services for .NET, Node.js, and Python as needed.

output cosmosDbConnectionString string = cosmosDbAccount.properties.connectionStrings[0].connectionString
output storageAccountConnectionString string = listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value
