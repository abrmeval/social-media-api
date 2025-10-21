# Python Image Service - Quick Reference

## 🚀 Start Service
```bash
cd python-image-service
venv\Scripts\activate
uvicorn main:app --reload --port 8000
```

## 📋 Key Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/` | GET | Service info |
| `/health` | GET | Health check |
| `/docs` | GET | Swagger UI |
| `/api/process-image` | POST | Process from blob URL |
| `/api/upload-and-process` | POST | Direct upload |
| `/api/images/{id}` | GET | Get image info |
| `/api/images/{id}` | DELETE | Delete processed images |

## 📁 Project Structure
```
python-image-service/
├── main.py                 # FastAPI app
├── models/
│   └── process_request.py  # Request/Response models
├── services/
│   ├── image_processor.py  # Image processing (Pillow)
│   └── blob_storage.py     # Azure Blob operations
├── requirements.txt        # Dependencies
├── .env                    # Configuration (create from .env.example)
├── Dockerfile              # Container config
├── README.md               # Full documentation
└── SETUP.md                # Setup instructions
```

## 🔧 Common Commands

### Development
```bash
# Install dependencies
pip install -r requirements.txt

# Run with auto-reload
uvicorn main:app --reload

# Test service
python test_service.py
```

### Docker
```bash
# Build
docker build -t image-service .

# Run
docker run -d -p 8000:8000 --env-file .env image-service
```

## 📊 Image Sizes

| Name | Dimensions | Quality | Size (approx) |
|------|-----------|---------|---------------|
| thumbnail | 150×150 | 75% | 5-15 KB |
| small | 320×320 | 80% | 15-40 KB |
| medium | 640×640 | 85% | 40-100 KB |
| large | 1200×1200 | 90% | 100-300 KB |

## 🔗 Integration Example (C#)

```csharp
// From .NET REST API MediaController
var request = new {
    image_id = mediaId,
    blob_url = rawImageUrl,
    user_id = userId
};

var response = await _httpClient.PostAsJsonAsync(
    "http://localhost:8000/api/process-image",
    request
);

var result = await response.Content
    .ReadFromJsonAsync<ProcessImageResponse>();

// Update Cosmos DB with result.processed_urls
```

## 📝 Sample Request

```bash
curl -X POST http://localhost:8000/api/process-image \
  -H "Content-Type: application/json" \
  -d '{
    "image_id": "img_test_001",
    "blob_url": "https://storage.blob.core.windows.net/media/raw/test.jpg",
    "sizes": ["thumbnail", "medium"]
  }'
```

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| Import errors | `pip install -r requirements.txt` |
| Blob storage connection | Check `.env` connection string |
| Port in use | Change `PORT` in `.env` or use `--port` flag |
| Image processing fails | Check image format (JPEG, PNG supported) |

## 🌐 URLs (Local)

- **Service**: http://localhost:8000
- **Health**: http://localhost:8000/health
- **Docs**: http://localhost:8000/docs
- **ReDoc**: http://localhost:8000/redoc

## 📦 Dependencies

- **fastapi** - Web framework
- **uvicorn** - ASGI server
- **pillow** - Image processing
- **azure-storage-blob** - Blob operations
- **pydantic** - Data validation
- **httpx** - HTTP client
- **python-dotenv** - Environment config

## ⚙️ Environment Variables

```ini
AZURE_BLOB_STORAGE_CONNECTION_STRING=...  # Required
BLOB_CONTAINER_NAME=media                 # Optional (default: media)
PORT=8000                                 # Optional (default: 8000)
LOG_LEVEL=INFO                            # Optional (default: INFO)
```

## 📈 Performance

- **Thumbnail**: ~50-100ms
- **Full processing (4 sizes)**: ~200-400ms
- **Concurrent**: Async support for multiple images
- **Memory**: Minimal footprint with streaming

## 🎯 Next Steps

1. ✅ Install dependencies
2. ✅ Configure `.env`
3. ✅ Start service
4. ✅ Test with `test_service.py`
5. 🔄 Integrate with REST API
6. 🚀 Deploy to Azure Container Apps
