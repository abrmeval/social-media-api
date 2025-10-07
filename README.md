# Social Media Backend Project

A cloud-native backend for a social media app, featuring:

- **REST API** with ASP.NET Core (.NET 8+)
- **GraphQL API** with Node.js/Express.js
- **Python microservice** (FastAPI) for image processing (resize, thumbnail, etc.)
- **Azure cloud deployment:** Cosmos DB, Blob Storage, App Service
- **Testing & CI/CD:** GitHub Actions, manual/automated test strategies

---

## 🏗 Architecture Overview

[Architecture Diagram](docs/architecture-diagram.md)

- **REST API (.NET Core):** Manages users, posts, comments, likes, media uploads.
- **GraphQL API (Node.js):** Flexible querying for users, posts, timelines.
- **Image Microservice (Python/FastAPI):** Handles image resizing and thumbnail creation, triggered by REST API.
- **Azure Integration:** Cosmos DB stores app data, Blob Storage manages images and media, all services deployed to Azure App Service.

---

## 📁 Folder Structure

```
/dotnet-rest-api/        # ASP.NET Core REST API
/node-graphql-api/       # Node.js/Express GraphQL API
/python-image-service/   # FastAPI microservice for image processing
/infra/                  # Infra-as-Code (Bicep/Terraform), Azure scripts
/docs/                   # Architecture diagrams, documentation
/.github/workflows/      # CI/CD, test automation pipelines
README.md                # Project overview and setup instructions
```
For details, see [`docs/folder-structure.txt`](docs/folder-structure.txt).

---

## 🚀 Features

- **User Management:** Registration, authentication (JWT or Azure AD B2C), profile updates.
- **Posts & Comments:** Full CRUD for posts, comments, likes. Cosmos DB containers for each entity.
- **Media Upload:** Save images to Blob Storage, trigger Python microservice for processing.
- **Authentication:** JWT-based (REST API) or Azure AD B2C integration.
- **API Documentation:** Swagger (REST), GraphQL Playground (GraphQL), FastAPI docs (image service).
- **Cloud Native:** Cosmos DB for data, Blob Storage for media, App Service for deployment.
- **CI/CD:** GitHub Actions workflow for build, test, deploy across all services.
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
    - [Azure App Service](https://learn.microsoft.com/azure/app-service/)

3. **Environment Variables**
    - Each service requires connection strings for Cosmos DB, Blob Storage, and authentication settings.
      ```
      AZURE_COSMOS_DB_CONNECTION_STRING=
      AZURE_BLOB_STORAGE_CONNECTION_STRING=
      JWT_SECRET=
      ```

4. **Running Locally**
    - **.NET Core REST API**
      ```bash
      cd dotnet-rest-api
      dotnet run
      ```
    - **Node.js GraphQL API**
      ```bash
      cd node-graphql-api
      npm install
      npm start
      ```
    - **Python Image Service**
      ```bash
      cd python-image-service
      pip install -r requirements.txt
      uvicorn main:app --reload
      ```

5. **Deploying to Azure**
    - See [`docs/deployment.md`](docs/deployment.md) for deployment steps.
    - Infrastructure provisioned via Bicep in [`azure-resources.bicep`](azure-resources.bicep).
    - Configure CI/CD pipelines as in `.github/workflows/ci.yml`.

---

## 📚 API Documentation

- **REST API:** [Swagger UI](http://localhost:5000/swagger)
- **GraphQL API:** [GraphQL Playground](http://localhost:4000/graphql)
- **Image Service:** [OpenAPI Docs](http://localhost:8000/docs)

---

## 🔄 Integration Workflow

1. User uploads an image via REST API (.NET Core).
2. Image is saved to Azure Blob Storage.
3. REST API triggers Python image service via REST call.
4. Python service processes the image, saves result to Blob Storage.
5. Metadata is updated in Cosmos DB (users, posts, media).

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

## 🌐 Useful Resources

- [ASP.NET Core Docs](https://learn.microsoft.com/aspnet/core/)
- [Express.js Docs](https://expressjs.com/)
- [FastAPI Docs](https://fastapi.tiangolo.com/)
- [Azure Cosmos DB](https://learn.microsoft.com/azure/cosmos-db/)
- [Azure Blob Storage](https://learn.microsoft.com/azure/storage/blobs/)
- [Azure App Service](https://learn.microsoft.com/azure/app-service/)
- [GitHub Actions](https://docs.github.com/actions)
- [Swagger](https://swagger.io/)
- [GraphQL](https://graphql.org/)

---

## 📄 License

MIT License — see [`LICENSE`](LICENSE) for details.