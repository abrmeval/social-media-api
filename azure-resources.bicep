// Azure Resources for Social Media API Project
// This Bicep file provisions Cosmos DB, Blob Storage, and a Free App Service Plan for backend deployment.

var randomId = toLower(substring(uniqueString(resourceGroup().id, deployment().name), 0, 5))

@description('Name for the Cosmos DB account.')
param cosmosDbAccountName string = 'socialmedia-cosmosdb'

@description('Name for the Cosmos DB database.')
param cosmosDbDatabaseName string = 'socialmedia-db'

@description('Name for the Blob Storage account.')
param storageAccountName string = 'socialmediastorage'

@description('Location for resources.')
param location string = resourceGroup().location

@description('App Service Plan name.')
param appServicePlanName string = 'socialmedia-appserviceplan'

@description('SKU for Cosmos DB.')
param cosmosDbSku string = 'Standard'

@description('SKU for Storage Account.')
param storageSku string = 'Standard_LRS'

@description('SKU for App Service Plan.')
param appServicePlanSku string = 'F1' // F1 is the Free tier

@description('Name for the Web App.')
param webAppName string = 'socialmedia-webapp'

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: '${cosmosDbAccountName}-${randomId}'
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
  name: '${storageAccountName}${randomId}'
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
  name: '${appServicePlanName}-${randomId}'
  location: location
  sku: {
    name: appServicePlanSku // F1 Free tier
    tier: 'Free'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true // For Linux
  }
}

resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: '${webAppName}-${randomId}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: '.NET|8.0' // .NET 8.0 runtime
    }
  }
}

// Add your App Services for .NET, Node.js, and Python as needed.

output cosmosDbAccountName string = cosmosDbAccount.name
output cosmosDbConnectionString string = 'AccountEndpoint=https://${cosmosDbAccount.name}.documents.azure.com:443/;AccountKey=${cosmosDbAccount.listKeys().primaryMasterKey};'
output cosmosDbEndpoint string = cosmosDbAccount.properties.documentEndpoint
output storageAccountName string = storageAccount.name
output storageAccountConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
output appServicePlanId string = appServicePlan.id
output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
