# Social Media Backend Project

A cloud-native backend for a social media app, featuring:

- **REST API** with ASP.NET Core (.NET 8+)
- **GraphQL API** with Node.js/Express.js
- **Python microservice** (FastAPI) for image processing (resize, thumbnail, etc.)
- **Azure cloud deployment:** Cosmos DB, Blob Storage, Azure Container Apps
- **Real-time notifications:** Azure SignalR Service (planned) for push notifications
- **Containerization:** Docker with Azure Container Apps for scalable deployments
- **Testing & CI/CD:** GitHub Actions, manual/automated test strategies

---

## 🏗 Architecture Overview

[Architecture Diagram](docs/architecture-diagram.md)

- **REST API (.NET Core):** Manages users, posts, comments, likes, media uploads. Admin-protected user management endpoints.
- **GraphQL API (Node.js/Apollo Server):** Flexible querying for posts, comments, and likes with modular resolvers.
- **Image Microservice (Python/FastAPI):** Handles image resizing and thumbnail creation, triggered by REST API.
- **Azure Integration:** Cosmos DB stores app data, Blob Storage manages images and media, all services containerized and deployed to Azure Container Apps.
- **Real-Time Communications (Planned):** Azure SignalR Service will provide WebSocket connections for instant push notifications to clients (new posts, likes, comments, follows, image processing status).

---

## 📁 Folder Structure

```
/dotnet-rest-api/        # ASP.NET Core REST API
/node-graphql-api/       # Node.js/Express GraphQL API
/python-image-service/   # FastAPI microservice for image processing
/infra/                  # Infra-as-Code (Bicep/Terraform/Docker), Azure scripts
/docs/                   # Architecture diagrams, documentation
/.github/workflows/      # CI/CD, test automation pipelines
README.md                # Project overview and setup instructions
```
For details, see [`docs/folder-structure.txt`](docs/folder-structure.txt).

---

## 🚀 Features

- **User Management:** Registration, authentication (JWT or Azure AD B2C), profile updates. Admin-only user CRUD endpoints.
- **Posts & Comments:** Full CRUD for posts, comments, likes. Cosmos DB containers for each entity.
- **Media Upload:** Save images to Blob Storage, trigger Python microservice for processing.
- **Authentication & Authorization:** JWT-based with role-based access control. Admin role for user management.
- **Real-Time Push Notifications (Planned):** Azure SignalR Service integration for instant notifications on new posts, likes, comments, follows, and image processing completion.
- **API Documentation:** Swagger (REST), Apollo GraphQL Playground (GraphQL), FastAPI docs (image service).
- **Cloud Native:** Cosmos DB for data, Blob Storage for media, Azure Container Apps for containerized microservices deployment.
- **Containerization:** Docker containers for each service, orchestrated via Azure Container Apps with auto-scaling and load balancing.
- **CI/CD:** GitHub Actions workflow for build, test, containerize, and deploy across all services.
- **Error Handling & Logging:** Consistent logging and error management across services.

---

## 🧪 Testing Strategy

- **Automated Tests:**
  - .NET REST API: xUnit/MSTest for controllers, services, data access.
  - Node.js GraphQL API: Jest/Mocha/Supertest for resolvers, middleware.
  - Python Image Service: pytest for image functions, endpoints.
- **Manual Testing:**
  - Swagger UI for REST API.
  - GraphQL Playground for GraphQL.
  - FastAPI `/docs` for image microservice.
- **CI/CD Integration:** Automated tests in `.github/workflows/ci.yml` run on every PR.

---

## ⚡ Setup & Getting Started

1. **Clone the Repository**
    ```bash
    git clone https://github.com/abrmeval/social-media-api.git
    cd social-media-api
    ```

2. **Configure Azure Services**
    - [Azure Cosmos DB](https://learn.microsoft.com/azure/cosmos-db/)
    - [Azure Blob Storage](https://learn.microsoft.com/azure/storage/blobs/)
    - [Azure Container Apps](https://learn.microsoft.com/azure/container-apps/)
    - [Azure SignalR Service](https://learn.microsoft.com/azure/azure-signalr/) (planned for real-time notifications)

3. **Environment Variables**
    - Each service requires connection strings for Cosmos DB, Blob Storage, and authentication settings.
      ```
      AZURE_COSMOS_DB_CONNECTION_STRING=
      AZURE_BLOB_STORAGE_CONNECTION_STRING=
      AZURE_SIGNALR_CONNECTION_STRING= (future implementation)
      JWT_SECRET=
      ```

4. **Running Locally**
    - **.NET Core REST API**
      ```bash
      cd dotnet-rest-api
      dotnet run
      ```
    - **Node.js GraphQL API (Apollo Server)**
      ```bash
      cd node-graphql-api
      npm install
      node src/server.js
      ```
      See [Node.js GraphQL Setup Guide](docs/node-api/node-graphql-setup-guide.md) for detailed configuration.
    - **Python Image Service**
      ```bash
      cd python-image-service
      pip install -r requirements.txt
      uvicorn main:app --reload
      ```

5. **Deploying to Azure**
    - **Container-based Deployment:**
      - Each service is containerized using Docker
      - Deploy to Azure Container Apps for auto-scaling and load balancing
      - See [`docs/deployment.md`](docs/deployment.md) for deployment steps
    - **Infrastructure as Code:**
      - Infrastructure provisioned via Bicep in [`azure-resources.bicep`](azure-resources.bicep)
      - Includes Cosmos DB, Blob Storage, Container Apps, and SignalR Service (planned)
    - **CI/CD Pipeline:**
      - GitHub Actions builds Docker images and pushes to Azure Container Registry
      - Automated deployment to Azure Container Apps
      - Configure pipelines in `.github/workflows/ci.yml`
    
    **Docker Build Commands:**
    ```bash
    # Build .NET REST API container
    cd dotnet-rest-api
    docker build -t social-media-rest-api .
    
    # Build Node.js GraphQL API container
    cd node-graphql-api
    docker build -t social-media-graphql-api .
    
    # Build Python Image Service container
    cd python-image-service
    docker build -t social-media-image-service .
    ```

---

## 📚 API Documentation

- **REST API:** [Swagger UI](http://localhost:5000/swagger) - See [REST API Endpoints](docs/rest-api-endpoints.md)
- **GraphQL API:** [Apollo GraphQL Playground](http://localhost:4000/) - See [GraphQL Endpoints](docs/node-api/graphql-endpoints.md)
- **Image Service:** [OpenAPI Docs](http://localhost:8000/docs)
- **Node.js Setup Guide:** [GraphQL Setup & Migration Guide](docs/node-api/node-graphql-setup-guide.md)

---

## 🔄 Integration Workflow

1. User uploads an image via REST API (.NET Core).
2. Image is saved to Azure Blob Storage.
3. REST API triggers Python image service via REST call.
4. Python service processes the image, saves result to Blob Storage.
5. Metadata is updated in Cosmos DB (users, posts, media).
6. **(Future)** Azure SignalR Service sends real-time notification to client that image processing is complete.

### Real-Time Notifications (Planned with Azure SignalR)
- **New Post Created:** Instant notification to followers
- **Post Liked/Commented:** Real-time updates to post author
- **User Followed:** Immediate notification to followed user
- **Image Processing Complete:** Push notification when Python service finishes processing
- **Live Feed Updates:** Automatic refresh of user feed without polling

---

## 🗂️ Version Control Workflow

- Branching: `main` for production; feature branches for new features (e.g., `feature/image-upload`).
- PRs required for merges to `main`, with tests passing.
- Issues for bugs, enhancements, and tasks.
- Example commit: `feat: add user profile update endpoint`.

---

## 🛠 Troubleshooting & Alternatives

- If Azure Blob Storage is unavailable, use local storage for dev/testing.
- Cosmos DB can be emulated locally for development.
- See [`docs/architecture-diagram.md`](docs/architecture-diagram.md) for system overview and alternatives.

---

## 🐳 Containerization & Deployment

This project uses **Docker** containers deployed to **Azure Container Apps** for scalable, cloud-native microservices architecture.

### Benefits of Azure Container Apps:
- ✅ **Auto-scaling:** Scales containers based on HTTP traffic or CPU/memory usage
- ✅ **Managed infrastructure:** No need to manage Kubernetes clusters
- ✅ **Microservices-friendly:** Each service runs in its own container
- ✅ **Cost-effective:** Pay only for resources used, scale to zero when idle
- ✅ **Built-in load balancing:** Automatic traffic distribution
- ✅ **CI/CD integration:** Seamless GitHub Actions deployment

### Container Architecture:
```
Azure Container Apps Environment
├── REST API Container (.NET Core)
├── GraphQL API Container (Node.js)
├── Image Service Container (Python/FastAPI)
└── (Future) SignalR Service for real-time notifications
```

---

## 🌐 Useful Resources

- [ASP.NET Core Docs](https://learn.microsoft.com/aspnet/core/)
- [Express.js Docs](https://expressjs.com/)
- [FastAPI Docs](https://fastapi.tiangolo.com/)
- [Azure Cosmos DB](https://learn.microsoft.com/azure/cosmos-db/)
- [Azure Blob Storage](https://learn.microsoft.com/azure/storage/blobs/)
- [Azure Container Apps](https://learn.microsoft.com/azure/container-apps/)
- [Azure SignalR Service](https://learn.microsoft.com/azure/azure-signalr/)
- [Docker Documentation](https://docs.docker.com/)
- [GitHub Actions](https://docs.github.com/actions)
- [Swagger](https://swagger.io/)
- [GraphQL](https://graphql.org/)

---

## 📄 License

MIT License — see [`LICENSE`](LICENSE) for details.