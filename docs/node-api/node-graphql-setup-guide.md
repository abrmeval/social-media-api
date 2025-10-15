# Node.js GraphQL API Setup Guide

This document provides a comprehensive guide for setting up a Node.js GraphQL API using Apollo Server, including library management, debugging configuration, and migration steps from express-graphql to Apollo Server.

---

## Table of Contents
1. [Libraries Installed](#libraries-installed)
2. [Libraries Replaced/Not Used](#libraries-replaced-not-used)
3. [Benefits of Apollo Server vs Express-GraphQL](#benefits-of-apollo-server-vs-express-graphql)
4. [Setup Timeline](#setup-timeline)
5. [Debugging Configuration](#debugging-configuration)
6. [Using Apollo Server](#using-apollo-server)
7. [Troubleshooting](#troubleshooting)

---

## Libraries Installed

### Core Dependencies
- **`apollo-server`** - GraphQL server implementation with built-in features
- **`graphql`** - JavaScript reference implementation for GraphQL
- **`@azure/cosmos`** - Azure Cosmos DB SDK for data persistence
- **`dotenv`** - Environment variable management from `.env` files

### Development Dependencies
- **`express`** - Initially used, now optional (not required for Apollo Server standalone)
- **`express-graphql`** - Previously used GraphQL middleware (replaced by Apollo Server)

---

## Libraries Replaced/Not Used

### Replaced Libraries
| Old Library | New Library | Reason |
|------------|-------------|---------|
| `express-graphql` | `apollo-server` | Better resolver support, modular architecture, built-in playground |
| `buildSchema` (from graphql) | String-based typeDefs | Apollo Server accepts SDL strings directly |

### Not Currently Used
- **`express`** - No longer needed since Apollo Server runs standalone
- **`express-graphql`** - Replaced by Apollo Server

---

## Benefits of Apollo Server vs Express-GraphQL

### Apollo Server Advantages

1. **Modular Resolver Support**
   - Full support for nested type resolvers (e.g., `Post.author`, `Comment.post`)
   - Clean separation of Query, Mutation, and type resolvers
   - No need for manual `rootValue` wiring

2. **Built-in Features**
   - GraphQL Playground included out-of-the-box
   - Schema stitching and federation support
   - Context and data source patterns
   - Performance monitoring and tracing

3. **Better Developer Experience**
   - Clear error messages with stack traces
   - Schema-first or code-first approaches
   - TypeScript support
   - Extensive documentation and community support

4. **Production Ready**
   - Built-in caching
   - Error handling middleware
   - Query complexity analysis
   - Rate limiting capabilities

5. **Ecosystem Integration**
   - Apollo Client integration
   - Apollo Studio monitoring
   - Subscriptions support (WebSockets)
   - File upload support

### Express-GraphQL Limitations
- Limited nested resolver support with `buildSchema`
- Manual `rootValue` configuration required
- Less intuitive error handling
- No built-in playground (requires separate setup)
- Limited ecosystem tooling

---

## Setup Timeline

### Phase 1: Initial Setup (Express-GraphQL)
```bash
# Step 1: Initialize project
npm init -y

# Step 2: Install initial dependencies
npm install express express-graphql graphql @azure/cosmos dotenv

# Step 3: Create project structure
mkdir -p src/dataSources src/dtos
touch src/server.js src/schema.js src/resolvers.js src/.env
```

### Phase 2: Schema and Resolvers
```bash
# Step 4: Create schema file (schema.js)
# - Define types using buildSchema
# - Export compiled schema

# Step 5: Create resolvers file (resolvers.js)
# - Define Query resolvers
# - Define Mutation resolvers
# - Define type field resolvers

# Step 6: Create data sources
# - postAPI.js for post CRUD operations
# - commentAPI.js for comment operations
# - Integrate with Cosmos DB
```

### Phase 3: Environment Configuration
```bash
# Step 7: Create .env file in src directory
COSMOS_ENDPOINT=https://your-account.documents.azure.com:443/
COSMOS_KEY=your-cosmos-key
COSMOS_DATABASE=your-database-name
PORT=4000

# Step 8: Configure cosmos-client.js
# - Initialize CosmosClient
# - Export database instance
```

### Phase 4: Debugging Setup
```bash
# Step 9: Create .vscode/launch.json
# - Configure Node.js debugger
# - Set working directory
# - Point to .env file

# Step 10: Test breakpoints
# - Set breakpoints in resolvers
# - Use F5 to start debugging
```

### Phase 5: Migration to Apollo Server
```bash
# Step 11: Install Apollo Server
npm install apollo-server

# Step 12: Update schema.js
# - Remove buildSchema wrapper
# - Export plain SDL string

# Step 13: Update server.js
# - Replace express-graphql with ApolloServer
# - Pass typeDefs and resolvers
# - Configure context if needed

# Step 14: Test and verify
# - Start server
# - Open GraphQL Playground
# - Test queries and mutations
```

### Phase 6: Cleanup (Optional)
```bash
# Step 15: Remove unused dependencies
npm uninstall express express-graphql

# Step 16: Update package.json scripts
# Add start script: "node src/server.js"
```

---

## Debugging Configuration

### VS Code Launch Configuration

Create or update `.vscode/launch.json` in your workspace root:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "type": "node",
      "request": "launch",
      "name": "Debug Node.js API",
      "skipFiles": [
        "<node_internals>/**"
      ],
      "program": "${workspaceFolder}/social-media-api/node-graphql-api/src/server.js",
      "cwd": "${workspaceFolder}/social-media-api/node-graphql-api/src",
      "envFile": "${workspaceFolder}/social-media-api/node-graphql-api/src/.env"
    }
  ]
}
```

### Debugging Steps

1. **Set Breakpoints**
   - Click to the left of line numbers in your code
   - Red dots indicate active breakpoints

2. **Start Debug Session**
   - Press `F5` or click "Run and Debug" in VS Code
   - Select "Debug Node.js API" configuration

3. **Trigger Code Execution**
   - Send GraphQL queries to execute resolver code
   - Breakpoints will pause execution

4. **Inspect Variables**
   - Hover over variables to see values
   - Use Debug Console for evaluation
   - Step through code with F10 (step over) and F11 (step into)

### Common Debugging Issues

**Breakpoints Not Hitting:**
- Ensure you're using VS Code debugger (F5), not terminal
- Verify breakpoints are in the actual file being executed
- Check that code path is triggered by your query
- Restart debug session after setting new breakpoints

**Environment Variables Not Loaded:**
- Verify `envFile` path in launch.json
- Ensure `.env` file exists in specified location
- Check for typos in environment variable names

---

## Using Apollo Server

### Server Structure

```javascript
// src/server.js
require('dotenv').config();
const { ApolloServer } = require('apollo-server');
const typeDefs = require('./schema');
const resolvers = require('./resolvers');

const server = new ApolloServer({
  typeDefs,
  resolvers,
  context: () => ({
    // Add shared context here (auth, dataSources, etc.)
  })
});

server.listen({ port: process.env.PORT || 4000 })
  .then(({ url }) => {
    console.log(`🚀 Apollo Server ready at ${url}`);
  })
  .catch(err => {
    console.error('Apollo Server failed to start:', err);
  });
```

### Schema Definition

```javascript
// src/schema.js
module.exports = `
  type Query {
    posts: [Post!]!
    post(id: ID!): Post
  }
  
  type Mutation {
    createPost(authorId: ID!, content: String!): Post!
  }
  
  type Post {
    id: ID!
    content: String!
    author: User!
  }
`;
```

### Resolver Structure

```javascript
// src/resolvers.js
const postAPI = require('./dataSources/postAPI');

module.exports = {
  Query: {
    posts: async () => {
      return postAPI.getAllPosts();
    },
    post: async (_, { id }) => {
      return postAPI.getPostById(id);
    }
  },
  
  Mutation: {
    createPost: async (_, { authorId, content }) => {
      return postAPI.createPost({ authorId, content });
    }
  },
  
  // Type field resolvers
  Post: {
    author: async (post, _, { dataSources }) => {
      return dataSources.userAPI.getUserById(post.authorId);
    }
  }
};
```

### Starting the Server

```bash
# Development
node src/server.js

# With debugging
# Use VS Code debugger (F5)
```

### Accessing GraphQL Playground

Once the server is running, open your browser to:
```
http://localhost:4000/
```

The Apollo Server playground provides:
- Schema explorer
- Query/mutation editor
- Documentation browser
- Query history

### Example Queries

**Fetch all posts:**
```graphql
query {
  posts {
    id
    content
    createdAt
    author {
      username
    }
  }
}
```

**Create a post:**
```graphql
mutation {
  createPost(
    authorId: "user-123"
    content: "Hello GraphQL!"
  ) {
    id
    content
    createdAt
  }
}
```

---

## Troubleshooting

### Common Errors and Solutions

#### 1. "Cannot return null for non-nullable field"
**Cause:** Resolver returns `null` or `undefined` for a field marked with `!`

**Solution:**
```javascript
posts: async () => {
  try {
    const posts = await postAPI.getAllPosts();
    return Array.isArray(posts) ? posts : [];
  } catch (err) {
    console.error("Error fetching posts:", err);
    return []; // Return empty array instead of null
  }
}
```

#### 2. "Field defined in resolvers, but not in schema"
**Cause:** Resolver exists for a field not defined in schema

**Solution:**
- Add the field to your schema type definition, OR
- Remove the resolver if the field is not needed

#### 3. "Invalid URL" (Cosmos DB)
**Cause:** `COSMOS_ENDPOINT` is missing or invalid

**Solution:**
- Check `.env` file has valid endpoint: `https://account.documents.azure.com:443/`
- Ensure `.env` is in the correct directory
- Verify `dotenv` is loaded before Cosmos client initialization

#### 4. Breakpoints Not Hitting in Resolvers
**Cause:** Resolver code not wired correctly or not executed

**Solution:**
- For Apollo Server: Pass full `resolvers` object to ApolloServer
- Ensure you send a GraphQL query to trigger the resolver
- Add `console.log()` to verify execution

#### 5. Server Starts Then Stops Immediately
**Cause:** Error during startup or schema validation

**Solution:**
- Check terminal for error messages
- Validate schema syntax
- Ensure all required environment variables are set
- Add `.catch()` to server.listen() for error logging

---

## Best Practices

### 1. Error Handling
Always handle errors in resolvers:
```javascript
posts: async () => {
  try {
    return await postAPI.getAllPosts();
  } catch (error) {
    console.error('Error in posts resolver:', error);
    throw new Error('Failed to fetch posts');
  }
}
```

### 2. Environment Variables
- Never commit `.env` files
- Use `.env.example` for documentation
- Validate required variables at startup

### 3. Schema Organization
- Keep schema modular as it grows
- Use schema stitching for large projects
- Document complex types with comments

### 4. Resolver Patterns
- Keep resolvers thin - business logic in data sources
- Use DTOs for data transformation
- Implement pagination for large datasets

### 5. Debugging
- Use `console.log` strategically
- Set breakpoints in resolver entry points
- Test queries in isolation
- Use GraphQL Playground for rapid testing

---

## Additional Resources

- [Apollo Server Documentation](https://www.apollographql.com/docs/apollo-server/)
- [GraphQL Documentation](https://graphql.org/learn/)
- [Azure Cosmos DB Node.js SDK](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/sdk-nodejs)
- [VS Code Debugging](https://code.visualstudio.com/docs/editor/debugging)

---

## Project Structure Reference

```
node-graphql-api/
├── src/
│   ├── dataSources/
│   │   ├── postAPI.js
│   │   └── commentAPI.js
│   ├── dtos/
│   │   ├── PostDto.js
│   │   └── CommentDto.js
│   ├── cosmos-client.js
│   ├── schema.js
│   ├── resolvers.js
│   ├── server.js
│   └── .env
├── node_modules/
├── package.json
└── package-lock.json
```

---

**Last Updated:** October 11, 2025
