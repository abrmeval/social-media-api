# Python Image Service - Setup Guide

Quick guide to get the Python image processing service running.

## Prerequisites

- Python 3.12+ installed
- Azure Blob Storage account
- Azure Storage connection string

## Quick Start (5 minutes)

### 1. Navigate to Service Directory
```powershell
cd python-image-service
```

### 2. Create Virtual Environment
```powershell
# Create venv
python -m venv venv

# Activate (Windows PowerShell)
.\venv\Scripts\Activate.ps1

# Or (Command Prompt)
venv\Scripts\activate.bat
```

### 3. Install Dependencies
```powershell
pip install -r requirements.txt
```

### 4. Configure Environment
```powershell
# Copy example env file
copy .env.example .env

# Edit .env with your credentials (use notepad or VS Code)
notepad .env
```

Required values in `.env`:
```ini
AZURE_BLOB_STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=https;AccountName=YOUR_ACCOUNT;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net
BLOB_CONTAINER_NAME=media
```

### 5. Run the Service
```powershell
# Development mode with auto-reload
uvicorn main:app --reload --port 8000
```

You should see:
```
INFO:     Uvicorn running on http://127.0.0.1:8000
INFO:     Application startup complete.
```

### 6. Test the Service
Open a new terminal and run:
```powershell
python test_service.py
```

Or visit:
- **API Docs**: http://localhost:8000/docs
- **Health Check**: http://localhost:8000/health

---

## Troubleshooting

### Issue: "Import "fastapi" could not be resolved"
**Solution**: Install dependencies
```powershell
pip install -r requirements.txt
```

### Issue: "Blob storage health check failed"
**Solution**: Verify your `.env` file has correct Azure connection string
```powershell
# Check if .env exists
dir .env

# Verify blob storage connection
# The health check endpoint will show details: http://localhost:8000/health
```

### Issue: "ModuleNotFoundError: No module named 'PIL'"
**Solution**: Install Pillow
```powershell
pip install Pillow==11.0.0
```

### Issue: Virtual environment not activating
**Solution**: PowerShell execution policy
```powershell
# Run as Administrator
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Then try activating again
.\venv\Scripts\Activate.ps1
```

---

## Next Steps

Once the service is running:

1. **Test with sample image**:
   - Use the Swagger UI at http://localhost:8000/docs
   - Try the "POST /api/upload-and-process" endpoint
   - Upload a test image

2. **Integrate with REST API**:
   - The .NET REST API should call this service after uploading images
   - See `README.md` for integration examples

3. **Monitor logs**:
   - The service logs all operations to console
   - Look for image processing operations, blob uploads, errors

---

## Docker Alternative

If you prefer Docker:

```powershell
# Build image
docker build -t image-service .

# Run container
docker run -d `
  --name image-service `
  -p 8000:8000 `
  -e AZURE_BLOB_STORAGE_CONNECTION_STRING="your_connection_string" `
  -e BLOB_CONTAINER_NAME="media" `
  image-service

# Check logs
docker logs image-service

# Stop container
docker stop image-service
```

---

## Configuration Reference

### Environment Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `AZURE_BLOB_STORAGE_CONNECTION_STRING` | ✅ Yes | - | Azure Storage account connection string |
| `BLOB_CONTAINER_NAME` | No | `media` | Container name for storing images |
| `PORT` | No | `8000` | Port the service runs on |
| `LOG_LEVEL` | No | `INFO` | Logging level (DEBUG, INFO, WARNING, ERROR) |

### Image Size Presets

The service generates these sizes by default:

| Size | Max Dimensions | Quality | Use Case |
|------|---------------|---------|----------|
| thumbnail | 150×150 | 75% | Avatars, small previews |
| small | 320×320 | 80% | Mobile feed |
| medium | 640×640 | 85% | Desktop feed |
| large | 1200×1200 | 90% | Full-size view |

*Aspect ratios are preserved - these are maximum dimensions*

---

## Development Tips

- **Auto-reload**: Use `--reload` flag during development
- **Debug logging**: Set `LOG_LEVEL=DEBUG` in `.env`
- **Test images**: Keep sample images in `temp/` for testing
- **API testing**: Use the built-in Swagger UI at `/docs`

---

## Getting Azure Credentials

If you don't have Azure Blob Storage yet:

1. **Create Storage Account**:
   - Go to Azure Portal → Create Resource → Storage Account
   - Note the account name

2. **Get Connection String**:
   - Navigate to your Storage Account
   - Settings → Access Keys
   - Copy "Connection string" under key1 or key2

3. **Create Container**:
   - In Storage Account → Containers → + Container
   - Name it "media"
   - Set public access level as needed

---

## Support

- **Full Documentation**: See `README.md`
- **API Reference**: http://localhost:8000/docs (when running)
- **Architecture**: See main project `docs/architecture-diagram.md`
- **Issues**: Check service logs for detailed error messages
