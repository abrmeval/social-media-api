# AI Coding Agent Instructions for Social Media Backend

## Architecture Overview

This is a **three-service microservices architecture** for a social media backend:

1. **REST API (.NET 8)** (`dotnet-rest-api/`) - Primary API for user management, posts, comments, likes, media uploads
2. **GraphQL API (Node.js)** (`node-graphql-api/`) - Flexible querying layer using Apollo Server 3
3. **Python Image Service (FastAPI)** (`python-image-service/`) - Image processing microservice (placeholder currently)

**Critical Data Flow:**
```
Client → REST API → Azure Cosmos DB (write operations)
Client → GraphQL API → Azure Cosmos DB (read operations, flexible queries)
REST API → Python Service → Azure Blob Storage (image processing workflow)
```

**Shared Azure Resources:**
- **Cosmos DB**: Single database with containers: `users`, `posts`, `comments`, `likes`, `media`
- **Blob Storage**: Media file storage (raw + processed images)
- **Key Vault**: JWT signing keys (RSA for REST API)

**Future Additions:**
- Azure SignalR Service for real-time push notifications
- Azure Container Apps deployment (replacing App Service)

---

## Service-Specific Patterns

### .NET REST API (`dotnet-rest-api/SocialMedia.Api/`)

**Authentication Architecture:**
- JWT tokens with RSA signatures stored in Azure Key Vault
- `Program.cs` uses **dual credential strategy**: `ClientSecretCredential` (dev) → `DefaultAzureCredential` (production/Managed Identity)
- Role-based auth: `[Authorize(Roles = "Admin")]` for `/api/users` endpoints
- All other endpoints use `[Authorize]` (any authenticated user)

**Key Services (Dependency Injection):**
- `ICosmosDbService` - Database operations
- `IBlobStorageService` - File storage operations  
- `ITokenService` - JWT token generation/validation

**Models Convention:**
- All DTOs in `Models/` folder
- Properties use nullable reference types (`string?`)
- `Id` property is partition key for Cosmos DB

**Running Locally:**
```bash
cd dotnet-rest-api
dotnet restore
dotnet run
# Swagger UI: http://localhost:5000/swagger
```

---

### Node.js GraphQL API (`node-graphql-api/src/`)

**Critical ES Module Setup:**
- Uses `"type": "module"` in `package.json`
- `cosmos-client.js` loads `.env` from `src/` directory using `fileURLToPath` and `dirname` workaround:
  ```javascript
  const __filename = fileURLToPath(import.meta.url);
  const __dirname = dirname(__filename);
  dotenv.config({ path: join(__dirname, '.env') });
  ```
- **All imports must use `.js` extension** (e.g., `import { postAPI } from "./dataSources/postAPI.js"`)

**Architecture Pattern:**
```
src/
├── server.js         # Apollo Server setup
├── schema.js         # GraphQL type definitions (single file)
├── resolvers.js      # All resolvers (Query, Mutation, Type resolvers)
├── cosmos-client.js  # Centralized Cosmos DB client
├── dataSources/      # Data access layer (one file per entity)
│   ├── postAPI.js
│   ├── commentAPI.js
│   ├── likeAPI.js
│   ├── profileAPI.js
│   └── mediaAPI.js
└── dtos/             # Data Transfer Objects matching .NET models
```

**Resolver Pattern (4 parameters):**
```javascript
export const Query = {
  posts: async () => { ... }  // Root resolver: no parent
}

export const Post = {
  // Type resolver: parent is the Post object from parent resolver
  author: async (parent) => {
    return profileAPI.getUserById(parent.authorId);
  }
}
```

**Data Source Pattern:**
- Each API class wraps a Cosmos DB container
- Use DTO conversion: `toLikeDto(data)` for consistent data shapes
- Error handling: catch 404s separately, log others, rethrow

**Running Locally:**
```bash
cd node-graphql-api
npm install
npm start  # or: node src/server.js
# GraphQL Playground: http://localhost:4000/
```

**Environment Variables (`.env` in `src/`):**
```
COSMOS_ENDPOINT=https://...
COSMOS_KEY=...
COSMOS_DATABASE=social-media-db
```

---

### Python Image Service (`python-image-service/`)

**Architecture Pattern:**
```
python-image-service/
├── main.py                    # FastAPI app with all endpoints
├── models/
│   └── process_request.py     # Pydantic models (Request/Response)
├── services/
│   ├── image_processor.py     # PIL/Pillow image processing
│   └── blob_storage.py        # Azure Blob Storage operations
├── requirements.txt           # Python dependencies
├── Dockerfile                 # Container configuration
└── .env                       # Environment variables
```

**Core Functionality:**
- **Image Processing**: Generates 4 sizes (thumbnail: 150x150, small: 320x320, medium: 640x640, large: 1200x1200)
- **Format Optimization**: JPEG compression with quality presets (75-90%)
- **EXIF Handling**: Auto-rotation based on EXIF orientation data
- **Blob Storage**: Downloads from URL, uploads processed versions

**Integration Workflow:**
1. REST API uploads raw image → Azure Blob Storage
2. REST API calls `POST /api/process-image` with blob URL
3. Python service downloads, processes (4 sizes), uploads to Blob Storage
4. Returns processed URLs to REST API
5. REST API updates Cosmos DB `media` container with metadata

**Key Endpoints:**
- `POST /api/process-image` - Process existing blob (main workflow)
- `POST /api/upload-and-process` - Direct upload (alternative)
- `GET /health` - Health check with blob storage status
- `GET /docs` - OpenAPI documentation

**Running Locally:**
```bash
cd python-image-service
python -m venv venv
venv\Scripts\activate  # Windows
pip install -r requirements.txt
uvicorn main:app --reload --port 8000
# Test: python test_service.py
# Docs: http://localhost:8000/docs
```

**Environment Variables (`.env`):**
```
AZURE_BLOB_STORAGE_CONNECTION_STRING=...
BLOB_CONTAINER_NAME=media
PORT=8000
```

**Size Presets** (in `ImageProcessor.SIZE_PRESETS`):
- Maintains aspect ratio during resize
- Uses Lanczos resampling for quality
- Progressive JPEG encoding
- Optimized compression

---

## Cross-Service Data Consistency

**Shared Entity Models:**
- Both .NET and Node.js use identical DTO structures for `User`, `Post`, `Comment`, `Like`, `Media`
- **String properties are nullable by default** (convention from `.github/instructions/context.instructions.md`)
- All entities have `isActive` flag for soft deletes

**Cosmos DB Queries:**
- Partition key = `id` field for all containers
- Use parameterized queries to prevent injection: `parameters: [{ name: "@postId", value: postId }]`
- Active records filter: `WHERE c.isActive = true`

---

## Development Workflows

### Building & Testing

**.NET API:**
```bash
cd dotnet-rest-api
dotnet build
dotnet test
```

**Node.js GraphQL:**
```bash
cd node-graphql-api
npm install
npm start
# No tests configured yet
```

**CI/CD:**
- `.github/workflows/dotnet.yml` - Builds/tests .NET API on push to `main`
- Working directory must be specified: `working-directory: dotnet-rest-api`

### Common Development Tasks

**Adding a New Entity:**
1. Create DTO in both `dotnet-rest-api/SocialMedia.Api/Models/` and `node-graphql-api/src/dtos/`
2. Add Cosmos DB container (if needed)
3. .NET: Create controller in `Controllers/`, add service methods
4. Node.js: Create `dataSources/entityAPI.js`, update `schema.js`, add resolvers to `resolvers.js`

**Debugging Cosmos DB Connection Issues:**
- Node.js: Verify `.env` is in `node-graphql-api/src/` (NOT root)
- Check environment variables are loaded: `console.log(process.env.COSMOS_ENDPOINT)`
- Common error: "Invalid URL, input: 'undefined'" → `.env` not found

---

## Key Documentation Files

- `docs/architecture-diagram.md` - System architecture and sequence diagrams
- `docs/rest-api-endpoints.md` - Complete REST API endpoint reference with auth requirements
- `docs/node-api/graphql-resolvers-guide.md` - Comprehensive resolver parameter guide (813 lines)
- `README.md` - Setup instructions, Azure service configuration, container deployment plans

---

## Conventions & Code Style

- **.NET:** PascalCase for classes/methods, camelCase for parameters
- **Node.js:** camelCase for everything, descriptive function names (e.g., `getLikesByPost`)
- **Error Handling:** Log errors with context, return meaningful error messages to client
- **Comments:** Add JSDoc/XML comments for complex logic explaining "why", not "what"
- **Nullable Strings:** Use `string?` in C#, accept `null` in JavaScript unless specific reason not to

---

## Common Pitfalls

1. **Node.js ES Modules:** Forgetting `.js` extension in imports causes runtime errors
2. **Environment Variables:** Node.js `.env` must be in `src/` directory (not root) due to `cosmos-client.js` path resolution
3. **Authentication:** Remember REST API uses RSA keys from Key Vault (complex setup), not simple secrets
4. **Cosmos DB Partition Keys:** Always use `id` as partition key when creating/reading items
5. **GraphQL Resolvers:** Type resolvers (Post.author) receive `parent`, root resolvers (Query.posts) don't - forgetting this causes undefined errors
6. **Soft Deletes:** Always filter by `isActive = true` in queries unless specifically retrieving deleted items

---

## When Making Changes

- **Read docs first:** Check `docs/` folder for architecture context before modifying services
- **Maintain parity:** Keep .NET and Node.js DTOs synchronized
- **Follow patterns:** Match existing code style (look at `likeAPI.js`, `AuthController.cs` as examples)
- **Test cross-service:** Changes to Cosmos DB schema affect both REST and GraphQL APIs
- **Update docs:** If adding endpoints or changing architecture, update relevant `.md` files
