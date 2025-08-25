# Social Media Backend Project

This project provides a cloud-native backend for a social media application, featuring:
- REST API built with ASP.NET Core (.NET 8+)
- GraphQL API built with Node.js/Express.js
- Python microservice (FastAPI) for image processing (resize, thumbnail)
- Cloud deployment using Azure (Cosmos DB, Blob Storage, App Service)
- Version control with Git & GitHub
- Automated and manual testing for all services

---

## Architecture Overview

![Architecture Diagram](docs/architecture.png) <!-- Replace with your actual diagram path -->

- **REST API (.NET Core):** Manages users, posts, comments, likes, and media uploads.
- **GraphQL API (Node.js):** Flexible querying of users, posts, comments, and likes.
- **Image Processing Microservice (Python/FastAPI):** Handles image resizing and thumbnail creation.
- **Azure Integration:** Cosmos DB stores app data, Blob Storage manages images and media, services are deployed to Azure App Service.

---

## Folder Structure

```
/backend-dotnet         # ASP.NET Core REST API
/backend-graphql        # Node.js/Express.js GraphQL API
/image-service-python   # Python FastAPI microservice for image processing
/docs                   # Architecture diagrams, API docs
/.github/workflows      # CI/CD and test automation pipelines
```

---

## Features

- **User Management:** Register, authenticate, update profiles.
- **Posts & Comments:** CRUD operations for posts, comments, and likes.
- **Media Upload:** Save images to Azure Blob Storage, trigger image processing.
- **Authentication:** JWT or Azure AD B2C integration.
- **API Documentation:** Swagger for REST API, GraphQL Playground for GraphQL.
- **Cloud Native:** Azure Cosmos DB for data, Blob Storage for media, App Service for deployment.
- **CI/CD:** GitHub Actions or Azure DevOps for automated deployment.
- **Error Handling & Logging:** Consistent logging and error management across services.

---

## 🗂️ Version Control Workflow

1. **Initialize Git Repository**
    - Clone/fork this repository:
      ```bash
      git clone https://github.com/<your-org>/<repo-name>.git
      cd <repo-name>
      ```

2. **Branching Strategy**
    - Use `main` for production-ready code.
    - Create feature branches for new features (e.g., `feature/image-upload`, `feature/graphql-comments`).
    - Use pull requests (PRs) for code review before merging into `main`.
    - Protect `main` with required PR reviews and passing tests.

3. **Commit Guidelines**
    - Use clear, descriptive commit messages.
    - Example: `feat: add user profile update endpoint`
    - Reference issues in commit messages when relevant.

4. **Collaboration**
    - Open issues for bugs, enhancements, and tasks.
    - Assign, review, and discuss PRs using GitHub.

---

## 🧪 Testing Strategy

**A. Automated Testing**

- **.NET REST API**
  - Use xUnit or MSTest for unit and integration tests.
  - Test controllers, services, and data access.
  - Example:
    ```bash
    cd backend-dotnet
    dotnet test
    ```

- **Node.js GraphQL API**
  - Use Jest, Mocha, or Supertest for unit and integration tests.
  - Test resolvers, schema, and middleware.
  - Example:
    ```bash
    cd backend-graphql
    npm test
    ```

- **Python Image Service**
  - Use pytest for unit tests.
  - Test image processing functions and API endpoints.
  - Example:
    ```bash
    cd image-service-python
    pytest
    ```

**B. Manual Testing**

- Use Swagger UI for REST API testing.
- Use GraphQL Playground for interactive GraphQL queries.
- Use FastAPI’s built-in `/docs` endpoint for API exploration.

**C. CI/CD Integration**

- Automated tests run on every pull request via GitHub Actions or Azure DevOps.
- Example workflow (see `.github/workflows/ci.yml`):
    ```yaml
    jobs:
      test-dotnet:
        runs-on: ubuntu-latest
        steps:
          - uses: actions/checkout@v3
          - name: Setup .NET
            uses: actions/setup-dotnet@v3
            with:
              dotnet-version: '8.0.x'
          - run: cd backend-dotnet && dotnet test
      test-node:
        runs-on: ubuntu-latest
        steps:
          - uses: actions/checkout@v3
          - name: Setup Node
            uses: actions/setup-node@v3
            with:
              node-version: '20.x'
          - run: cd backend-graphql && npm install && npm test
      test-python:
        runs-on: ubuntu-latest
        steps:
          - uses: actions/checkout@v3
          - name: Setup Python
            uses: actions/setup-python@v3
            with:
              python-version: '3.11'
          - run: cd image-service-python && pip install -r requirements.txt && pytest
    ```

---

## Setup & Getting Started

1. **Clone the Repository**
    ```bash
    git clone https://github.com/<your-org>/<repo-name>.git
    cd <repo-name>
    ```

2. **Configure Azure Services**
    - [Azure Cosmos DB](https://docs.microsoft.com/azure/cosmos-db/)
    - [Azure Blob Storage](https://docs.microsoft.com/azure/storage/blobs/)
    - [Azure App Service](https://docs.microsoft.com/azure/app-service/)

3. **Environment Variables**
    - Each service requires connection strings for Cosmos DB, Blob Storage, and authentication settings.
    - Example (.NET Core):
      ```
      AZURE_COSMOS_DB_CONNECTION_STRING=
      AZURE_BLOB_STORAGE_CONNECTION_STRING=
      JWT_SECRET=
      ```

4. **Running Locally**
    - **.NET Core REST API**
      ```bash
      cd backend-dotnet
      dotnet run
      ```
    - **Node.js GraphQL API**
      ```bash
      cd backend-graphql
      npm install
      npm start
      ```
    - **Python Image Service**
      ```bash
      cd image-service-python
      pip install -r requirements.txt
      uvicorn main:app --reload
      ```

5. **Deploying to Azure**
    - See `/docs/deployment.md` for step-by-step instructions.
    - Configure CI/CD pipelines with GitHub Actions or Azure DevOps.

---

## API Documentation

- **REST API:** [Swagger UI](http://localhost:5000/swagger) — full endpoint documentation.
- **GraphQL API:** [GraphQL Playground](http://localhost:4000/graphql) — interactive query explorer.
- **Image Service:** [OpenAPI Docs](http://localhost:8000/docs) — FastAPI auto-generated docs.

---

## Integration Workflow

1. User uploads an image via REST API (.NET Core).
2. Image is saved to Azure Blob Storage.
3. REST API triggers Python image service via REST call.
4. Python service processes the image, saves result back to Blob Storage.
5. Metadata is updated in Cosmos DB (users, posts, media).

---

## Troubleshooting & Alternatives

- If Azure Blob Storage is unavailable, consider [AWS S3](https://aws.amazon.com/s3/) or [Google Cloud Storage](https://cloud.google.com/storage).
- For authentication, you can use [Auth0](https://auth0.com/) or [Firebase Auth](https://firebase.google.com/products/auth).
- For database, alternatives include [MongoDB Atlas](https://www.mongodb.com/atlas) or [Amazon DynamoDB](https://aws.amazon.com/dynamodb/).

---

## Useful Resources

- [ASP.NET Core Documentation](https://learn.microsoft.com/aspnet/core/)
- [Express.js Documentation](https://expressjs.com/)
- [FastAPI Documentation](https://fastapi.tiangolo.com/)
- [Azure Cosmos DB](https://docs.microsoft.com/azure/cosmos-db/)
- [Azure Blob Storage](https://docs.microsoft.com/azure/storage/blobs/)
- [Azure App Service](https://docs.microsoft.com/azure/app-service/)
- [GitHub Actions](https://docs.github.com/actions)
- [Swagger](https://swagger.io/)
- [GraphQL](https://graphql.org/)

---

## Contributing

See [CONTRIBUTING.md](docs/CONTRIBUTING.md) for guidelines.

---

## License

This project is licensed under the MIT License.
