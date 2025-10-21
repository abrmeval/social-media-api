# Service Architecture - Visual Guide

## 🏗 Complete System Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                         CLIENT APP                              │
│                  (Web, Mobile, Desktop)                         │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ↓
              POST /api/media/upload
              (User uploads photo.jpg)
                         │
                         ↓
┌────────────────────────────────────────────────────────────────┐
│              .NET REST API (Port 5000)                         │
│                  MediaController.cs                            │
│                                                                │
│  1. Validate file                                              │
│  2. Upload raw image → Azure Blob Storage (raw/photo.jpg)      │
│  3. Call Python service                                        │
└────────────────────────┬──────────────────────────────────────┘
                         │
                         ↓
              POST /api/process-image
              {
                "image_id": "img_123",
                "blob_url": "https://...raw/photo.jpg"
              }
                         │
                         ↓
┌────────────────────────────────────────────────────────────────┐
│         PYTHON IMAGE SERVICE (Port 8000)                       │
│                    main.py                                     │
│                                                                │
│  ┌──────────────────────────────────────────────────┐         │
│  │  1. Receive & Validate Request                   │         │
│  │     ↓ (models/process_request.py)                │         │
│  │  ProcessImageRequest validates JSON              │         │
│  └──────────────────────────────────────────────────┘         │
│                         ↓                                      │
│  ┌──────────────────────────────────────────────────┐         │
│  │  2. Download Original Image                      │         │
│  │     ↓ (services/blob_storage.py)                 │         │
│  │  BlobStorageService.download_from_url()          │         │
│  │  Downloads: raw/photo.jpg → bytes                │         │
│  └──────────────────────────────────────────────────┘         │
│                         ↓                                      │
│  ┌──────────────────────────────────────────────────┐         │
│  │  3. Process Image                                │         │
│  │     ↓ (services/image_processor.py)              │         │
│  │  ImageProcessor.process_image()                  │         │
│  │                                                   │         │
│  │  Creates 4 versions:                             │         │
│  │  • thumbnail.jpg (150x150, 75% quality)          │         │
│  │  • small.jpg     (320x320, 80% quality)          │         │
│  │  • medium.jpg    (640x640, 85% quality)          │         │
│  │  • large.jpg     (1200x1200, 90% quality)        │         │
│  └──────────────────────────────────────────────────┘         │
│                         ↓                                      │
│  ┌──────────────────────────────────────────────────┐         │
│  │  4. Upload Processed Images                      │         │
│  │     ↓ (services/blob_storage.py)                 │         │
│  │  BlobStorageService.upload() × 4                 │         │
│  │  Uploads to:                                     │         │
│  │  • processed/img_123/thumbnail.jpg               │         │
│  │  • processed/img_123/small.jpg                   │         │
│  │  • processed/img_123/medium.jpg                  │         │
│  │  • processed/img_123/large.jpg                   │         │
│  └──────────────────────────────────────────────────┘         │
│                         ↓                                      │
│  ┌──────────────────────────────────────────────────┐         │
│  │  5. Return Response                              │         │
│  │     ↓ (models/process_request.py)                │         │
│  │  ProcessImageResponse with URLs                  │         │
│  └──────────────────────────────────────────────────┘         │
│                                                                │
└────────────────────────┬───────────────────────────────────────┘
                         │
                         ↓
              Returns JSON:
              {
                "image_id": "img_123",
                "status": "success",
                "processed_urls": {
                  "thumbnail": "https://...thumbnail.jpg",
                  "small": "https://...small.jpg",
                  "medium": "https://...medium.jpg",
                  "large": "https://...large.jpg"
                }
              }
                         │
                         ↓
┌────────────────────────────────────────────────────────────────┐
│              .NET REST API (Port 5000)                         │
│                                                                │
│  6. Update Cosmos DB 'media' container:                        │
│     {                                                          │
│       "id": "media_123",                                       │
│       "imageId": "img_123",                                    │
│       "rawUrl": "https://...raw/photo.jpg",                    │
│       "thumbnailUrl": "https://...thumbnail.jpg",              │
│       "smallUrl": "https://...small.jpg",                      │
│       "mediumUrl": "https://...medium.jpg",                    │
│       "largeUrl": "https://...large.jpg"                       │
│     }                                                          │
│                                                                │
│  7. Return to client with all URLs                            │
└────────────────────────┬───────────────────────────────────────┘
                         │
                         ↓
┌────────────────────────────────────────────────────────────────┐
│                      CLIENT APP                                │
│                                                                │
│  Can now display:                                              │
│  • Thumbnail in list view                                      │
│  • Small on mobile                                             │
│  • Medium on tablet                                            │
│  • Large on desktop                                            │
└────────────────────────────────────────────────────────────────┘


┌────────────────────────────────────────────────────────────────┐
│                    AZURE BLOB STORAGE                          │
│                                                                │
│  Container: media                                              │
│  ├── raw/                                                      │
│  │   └── photo.jpg                (original upload)           │
│  │                                                             │
│  └── processed/                                                │
│      └── img_123/                                              │
│          ├── thumbnail.jpg         (150x150)                   │
│          ├── small.jpg             (320x320)                   │
│          ├── medium.jpg            (640x640)                   │
│          └── large.jpg             (1200x1200)                 │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

---

## 📦 Python Service Internal Architecture

```
main.py (FastAPI App)
    │
    ├─→ Endpoints (@app.post, @app.get)
    │   │
    │   ├─ POST /api/process-image
    │   │   └─→ process_image(request)
    │   │
    │   ├─ POST /api/upload-and-process
    │   │   └─→ upload_and_process(file)
    │   │
    │   ├─ GET /health
    │   │   └─→ health_check()
    │   │
    │   └─ GET /api/images/{id}
    │       └─→ get_image_info(id)
    │
    ├─→ Models (Data Validation)
    │   │
    │   └─ models/process_request.py
    │       ├─ ProcessImageRequest    (input validation)
    │       ├─ ProcessImageResponse   (output format)
    │       ├─ ImageSize              (size configuration)
    │       └─ ErrorResponse          (error format)
    │
    └─→ Services (Business Logic)
        │
        ├─ services/image_processor.py
        │   │
        │   └─ ImageProcessor
        │       ├─ SIZE_PRESETS (configuration)
        │       ├─ process_image() (main processing)
        │       ├─ _resize_image() (helper method)
        │       └─ get_image_info() (metadata)
        │
        └─ services/blob_storage.py
            │
            └─ BlobStorageService
                ├─ __init__() (setup Azure client)
                ├─ upload() (save to blob)
                ├─ download() (get by name)
                ├─ download_from_url() (get by URL)
                ├─ delete() (remove blob)
                └─ list_blobs_by_prefix() (query blobs)
```

---

## 🔄 Request Processing Flow (Detailed)

```
┌─────────────────────────────────────────────────────────────┐
│ 1. HTTP REQUEST arrives                                    │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. FastAPI Framework                                        │
│    • Routes request to endpoint decorator                   │
│    • Parses JSON body                                       │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. Pydantic Validation                                      │
│    • Checks all required fields exist                       │
│    • Validates types (string, int, list, etc.)              │
│    • Runs field validators                                  │
│    • Creates ProcessImageRequest object                     │
│    • If invalid → Returns 400 Bad Request                   │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. Endpoint Function Executes                               │
│    async def process_image(request: ProcessImageRequest)    │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Download Original Image                                  │
│    blob_storage.download_from_url(request.blob_url)         │
│    • Makes HTTP GET request to Azure Blob                   │
│    • Returns bytes                                          │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. Process Image                                            │
│    image_processor.process_image(bytes, sizes)              │
│    │                                                         │
│    ├─→ Load image with PIL (Image.open)                     │
│    ├─→ Auto-rotate based on EXIF                            │
│    ├─→ Convert to RGB if needed                             │
│    │                                                         │
│    └─→ For each size (thumbnail, small, medium, large):     │
│        ├─ Calculate new dimensions (preserve aspect)        │
│        ├─ Resize with Lanczos algorithm                     │
│        ├─ Compress to JPEG with quality setting            │
│        └─ Save to BytesIO buffer                            │
│                                                              │
│    Returns: {"thumbnail": bytes, "small": bytes, ...}       │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. Upload Processed Images                                  │
│    For each size:                                           │
│    blob_storage.upload(blob_name, image_bytes)              │
│    • Gets BlobClient for path                               │
│    • Uploads bytes with content-type                        │
│    • Sets cache headers                                     │
│    • Returns blob URL                                       │
│                                                              │
│    Collects all URLs in dictionary                          │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 8. Create Response Object                                   │
│    ProcessImageResponse(                                    │
│        image_id=request.image_id,                           │
│        status="success",                                    │
│        processed_urls=url_dict                              │
│    )                                                         │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 9. FastAPI Serialization                                    │
│    • Converts ProcessImageResponse to JSON                  │
│    • Sets Content-Type: application/json                    │
│    • Returns HTTP 200 OK                                    │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 10. HTTP RESPONSE sent to client                            │
└─────────────────────────────────────────────────────────────┘
```

---

## 🧩 How Python Components Work Together

### Example: Processing "cat.jpg"

```python
# 1. REQUEST ARRIVES
# HTTP POST to /api/process-image with JSON:
{
    "image_id": "cat_001",
    "blob_url": "https://storage.blob.core.windows.net/media/raw/cat.jpg"
}

# 2. PYDANTIC VALIDATES (models/process_request.py)
request = ProcessImageRequest(
    image_id="cat_001",
    blob_url="https://storage.blob.core.windows.net/media/raw/cat.jpg",
    sizes=["thumbnail", "small", "medium", "large"]  # default value
)
# ✅ All fields valid, object created

# 3. DOWNLOAD IMAGE (services/blob_storage.py)
blob_storage = BlobStorageService(connection_string, "media")
original_bytes = await blob_storage.download_from_url(request.blob_url)
# Downloads cat.jpg → 2.4 MB bytes

# 4. PROCESS IMAGE (services/image_processor.py)
processor = ImageProcessor()
processed = await processor.process_image(
    image_data=original_bytes,
    image_id="cat_001",
    sizes=["thumbnail", "small", "medium", "large"]
)

# Inside process_image():
image = Image.open(BytesIO(original_bytes))  # Load with PIL
# Original: 3000x2000 pixels

results = {}
for size_name in ["thumbnail", "small", "medium", "large"]:
    width, height, quality = SIZE_PRESETS[size_name]
    # thumbnail: (150, 150, 75)
    
    # Resize maintaining aspect ratio
    resized = image.resize((150, 100), Image.Resampling.LANCZOS)
    # 150x100 (maintains 3:2 ratio)
    
    # Save as JPEG
    buffer = BytesIO()
    resized.save(buffer, format="JPEG", quality=75)
    results["thumbnail"] = buffer.getvalue()  # bytes

# Returns: {
#     "thumbnail": <12KB bytes>,
#     "small": <35KB bytes>,
#     "medium": <85KB bytes>,
#     "large": <220KB bytes>
# }

# 5. UPLOAD PROCESSED (services/blob_storage.py)
urls = {}
for size, data in processed.items():
    blob_name = f"processed/cat_001/{size}.jpg"
    # "processed/cat_001/thumbnail.jpg"
    
    url = await blob_storage.upload(blob_name, data)
    urls[size] = url

# urls = {
#     "thumbnail": "https://.../processed/cat_001/thumbnail.jpg",
#     "small": "https://.../processed/cat_001/small.jpg",
#     "medium": "https://.../processed/cat_001/medium.jpg",
#     "large": "https://.../processed/cat_001/large.jpg"
# }

# 6. CREATE RESPONSE (models/process_request.py)
response = ProcessImageResponse(
    image_id="cat_001",
    status="success",
    original_url=request.blob_url,
    processed_urls=urls,
    message="Image processed successfully into 4 sizes"
)

# 7. FASTAPI AUTO-CONVERTS TO JSON
# Returns:
{
    "image_id": "cat_001",
    "status": "success",
    "original_url": "https://.../raw/cat.jpg",
    "processed_urls": {
        "thumbnail": "https://.../processed/cat_001/thumbnail.jpg",
        "small": "https://.../processed/cat_001/small.jpg",
        "medium": "https://.../processed/cat_001/medium.jpg",
        "large": "https://.../processed/cat_001/large.jpg"
    },
    "message": "Image processed successfully into 4 sizes",
    "processed_at": "2025-10-20T14:35:22.123Z"
}
```

---

## 📚 Learning Resources

### Understanding Python
- **Python.org Tutorial**: https://docs.python.org/3/tutorial/
- **Real Python**: https://realpython.com/
- **Python for C# Developers**: https://realpython.com/python-vs-csharp/

### FastAPI
- **Official Docs**: https://fastapi.tiangolo.com/
- **Tutorial**: https://fastapi.tiangolo.com/tutorial/

### Pillow (Image Processing)
- **Documentation**: https://pillow.readthedocs.io/

### Azure Python SDK
- **Blob Storage**: https://learn.microsoft.com/python/api/azure-storage-blob/

---

This guide should help you understand exactly what's happening in the Python service! 
Let me know if you want me to explain any specific part in more detail. 😊
