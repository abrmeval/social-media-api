# Python Image Processing Service

FastAPI microservice for processing and optimizing images in the social media backend.

## Features

- 📸 **Multiple Size Generation** - Automatically creates thumbnail, small, medium, and large versions
- 🎨 **Image Optimization** - JPEG compression with quality presets
- 🔄 **EXIF Auto-Rotation** - Automatically corrects image orientation
- ☁️ **Azure Blob Storage** - Seamless integration with Azure Storage
- 🚀 **Async Processing** - Non-blocking image operations
- 🏥 **Health Checks** - Built-in health monitoring endpoints
- 📊 **OpenAPI Docs** - Auto-generated API documentation

---

## Architecture Integration

```
REST API (.NET) → Upload image → Azure Blob Storage (raw)
                ↓
         Trigger Python Service
                ↓
    Process image (4 sizes) → Azure Blob Storage (processed)
                ↓
         Return URLs to REST API
                ↓
    Update Cosmos DB with metadata
```

---

## API Endpoints

### Core Endpoints

#### `POST /api/process-image`
Process an existing image from Blob Storage URL.

**Request:**
```json
{
  "image_id": "img_20251020_123456",
  "blob_url": "https://storage.blob.core.windows.net/media/raw/image.jpg",
  "user_id": "user123",
  "sizes": ["thumbnail", "small", "medium", "large"]
}
```

**Response:**
```json
{
  "image_id": "img_20251020_123456",
  "status": "success",
  "original_url": "https://storage.blob.core.windows.net/media/raw/image.jpg",
  "processed_urls": {
    "thumbnail": "https://storage.blob.core.windows.net/media/processed/img_20251020_123456/thumbnail.jpg",
    "small": "https://storage.blob.core.windows.net/media/processed/img_20251020_123456/small.jpg",
    "medium": "https://storage.blob.core.windows.net/media/processed/img_20251020_123456/medium.jpg",
    "large": "https://storage.blob.core.windows.net/media/processed/img_20251020_123456/large.jpg"
  },
  "message": "Image processed successfully into 4 sizes",
  "processed_at": "2025-10-20T12:34:56.789Z"
}
```

#### `POST /api/upload-and-process`
Direct file upload (alternative workflow).

**Form Data:**
- `file`: Image file
- `image_id`: Optional custom ID
- `sizes`: Optional comma-separated sizes

#### `GET /api/images/{image_id}`
Get information about processed image versions.

#### `DELETE /api/images/{image_id}`
Delete all processed versions of an image.

### Utility Endpoints

- `GET /` - Service info
- `GET /health` - Health check with dependency status
- `GET /docs` - OpenAPI documentation (Swagger UI)
- `GET /redoc` - ReDoc documentation

---

## Image Size Presets

| Size | Dimensions | Quality | Use Case |
|------|-----------|---------|----------|
| **thumbnail** | 150×150 | 75% | Profile pictures, small previews |
| **small** | 320×320 | 80% | Mobile feed view |
| **medium** | 640×640 | 85% | Tablet/desktop feed |
| **large** | 1200×1200 | 90% | Full-size viewing |
| **xlarge** | 1920×1920 | 90% | High-res displays |

*Note: Actual dimensions preserve aspect ratio*

---

## Setup & Running

### Prerequisites

- Python 3.12+
- Azure Blob Storage account
- pip or poetry for package management

### Local Development

1. **Create virtual environment**
   ```bash
   cd python-image-service
   python -m venv venv
   
   # Windows
   venv\Scripts\activate
   
   # macOS/Linux
   source venv/bin/activate
   ```

2. **Install dependencies**
   ```bash
   pip install -r requirements.txt
   ```

3. **Configure environment**
   ```bash
   cp .env.example .env
   # Edit .env with your Azure credentials
   ```

4. **Run the service**
   ```bash
   # Development mode with auto-reload
   uvicorn main:app --reload --port 8000
   
   # Or using Python directly
   python main.py
   ```

5. **Access documentation**
   - Swagger UI: http://localhost:8000/docs
   - ReDoc: http://localhost:8000/redoc
   - Health check: http://localhost:8000/health

---

## Docker Deployment

### Build Image
```bash
docker build -t social-media-image-service .
```

### Run Container
```bash
docker run -d \
  --name image-service \
  -p 8000:8000 \
  -e AZURE_BLOB_STORAGE_CONNECTION_STRING="your_connection_string" \
  -e BLOB_CONTAINER_NAME="media" \
  social-media-image-service
```

### Docker Compose
```yaml
version: '3.8'
services:
  image-service:
    build: .
    ports:
      - "8000:8000"
    environment:
      - AZURE_BLOB_STORAGE_CONNECTION_STRING=${AZURE_BLOB_STORAGE_CONNECTION_STRING}
      - BLOB_CONTAINER_NAME=media
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
```

---

## Environment Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `AZURE_BLOB_STORAGE_CONNECTION_STRING` | Yes | - | Azure Storage connection string |
| `BLOB_CONTAINER_NAME` | No | `media` | Blob container for images |
| `PORT` | No | `8000` | Server port |
| `LOG_LEVEL` | No | `INFO` | Logging level |

---

## Testing

### Manual Testing with curl

**Process an image:**
```bash
curl -X POST http://localhost:8000/api/process-image \
  -H "Content-Type: application/json" \
  -d '{
    "image_id": "test123",
    "blob_url": "https://storage.blob.core.windows.net/media/raw/test.jpg",
    "sizes": ["thumbnail", "medium"]
  }'
```

**Upload and process:**
```bash
curl -X POST http://localhost:8000/api/upload-and-process \
  -F "file=@/path/to/image.jpg" \
  -F "sizes=thumbnail,small,medium"
```

**Get image info:**
```bash
curl http://localhost:8000/api/images/test123
```

### Testing with Python
```python
import httpx
import asyncio

async def test_process():
    async with httpx.AsyncClient() as client:
        response = await client.post(
            "http://localhost:8000/api/process-image",
            json={
                "image_id": "test123",
                "blob_url": "https://storage.blob.core.windows.net/media/raw/test.jpg"
            }
        )
        print(response.json())

asyncio.run(test_process())
```

---

## Integration with REST API

The .NET REST API should call this service after uploading an image:

```csharp
// C# example in MediaController
var processRequest = new {
    image_id = mediaId,
    blob_url = blobUrl,
    user_id = userId
};

var response = await httpClient.PostAsJsonAsync(
    "http://image-service:8000/api/process-image",
    processRequest
);

var result = await response.Content.ReadFromJsonAsync<ProcessImageResponse>();
```

---

## Performance

- **Thumbnail generation**: ~50-100ms per image
- **Full processing (4 sizes)**: ~200-400ms per image
- **Concurrent processing**: Supports async processing of multiple images
- **Memory efficient**: Streaming uploads/downloads, minimal memory footprint

---

## Monitoring

### Health Check
```bash
curl http://localhost:8000/health
```

**Response:**
```json
{
  "status": "healthy",
  "services": {
    "blob_storage": "connected",
    "image_processor": "ready"
  },
  "timestamp": "2025-10-20T12:34:56.789Z"
}
```

### Logs
The service logs all operations with structured logging:
- Image processing operations
- Blob storage interactions
- Errors and warnings
- Performance metrics

---

## Error Handling

All endpoints return standard error responses:

```json
{
  "error": "ImageProcessingError",
  "detail": "Failed to process image: Invalid image format",
  "timestamp": "2025-10-20T12:34:56.789Z"
}
```

**Common HTTP Status Codes:**
- `200 OK` - Success
- `400 Bad Request` - Invalid input
- `404 Not Found` - Image/resource not found
- `500 Internal Server Error` - Processing failure
- `503 Service Unavailable` - Service unhealthy

---

## Future Enhancements

- [ ] Azure SignalR integration for real-time processing notifications
- [ ] Image format conversion (WebP, AVIF)
- [ ] Advanced filters and effects
- [ ] Face detection and cropping
- [ ] Automated content moderation
- [ ] Redis caching for processed images
- [ ] Metrics and telemetry (Application Insights)

---

## Dependencies

See `requirements.txt` for full list:
- **FastAPI** - Web framework
- **Pillow** - Image processing
- **Azure SDK** - Blob storage integration
- **Uvicorn** - ASGI server
- **Pydantic** - Data validation

---

## License

MIT License - See LICENSE file in repository root
