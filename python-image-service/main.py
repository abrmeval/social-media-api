"""
FastAPI Image Processing Service for Social Media Backend

This service handles image processing tasks including:
- Image resizing (multiple sizes)
- Thumbnail generation
- Format optimization
- Upload to Azure Blob Storage

Triggered by the .NET REST API when users upload media.
"""

from fastapi import FastAPI, HTTPException, File, UploadFile
from fastapi.responses import JSONResponse
from pydantic import BaseModel
from typing import Optional, List
import os
from dotenv import load_dotenv
import logging
from datetime import datetime

# Import image processing and Azure services
from services.image_processor import ImageProcessor
from services.blob_storage import BlobStorageService
from models.process_request import ProcessImageRequest, ProcessImageResponse, ImageSize

# Load environment variables
load_dotenv()

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# Initialize FastAPI app
app = FastAPI(
    title="Social Media Image Processing Service",
    description="Microservice for processing and optimizing uploaded images",
    version="1.0.0",
    docs_url="/docs",
    redoc_url="/redoc"
)

# Initialize services
image_processor = ImageProcessor()
blob_storage = BlobStorageService(
    connection_string=os.getenv("AZURE_BLOB_STORAGE_CONNECTION_STRING"),
    container_name=os.getenv("BLOB_CONTAINER_NAME", "media")
)


@app.get("/")
async def root():
    """Health check endpoint"""
    return {
        "service": "Social Media Image Processing Service",
        "status": "running",
        "version": "1.0.0",
        "timestamp": datetime.utcnow().isoformat()
    }


@app.get("/health")
async def health_check():
    """Detailed health check with service dependencies"""
    try:
        # Check blob storage connection
        blob_healthy = await blob_storage.health_check()
        
        return {
            "status": "healthy" if blob_healthy else "degraded",
            "services": {
                "blob_storage": "connected" if blob_healthy else "unavailable",
                "image_processor": "ready"
            },
            "timestamp": datetime.utcnow().isoformat()
        }
    except Exception as e:
        logger.error(f"Health check failed: {str(e)}")
        return JSONResponse(
            status_code=503,
            content={
                "status": "unhealthy",
                "error": str(e),
                "timestamp": datetime.utcnow().isoformat()
            }
        )


@app.post("/api/process-image", response_model=ProcessImageResponse)
async def process_image(request: ProcessImageRequest):
    """
    Process an image from Azure Blob Storage
    
    Workflow:
    1. Download original image from blob URL
    2. Generate multiple sizes (thumbnail, small, medium, large)
    3. Upload processed images back to Blob Storage
    4. Return URLs for all processed versions
    
    Args:
        request: ProcessImageRequest with blob URL and metadata
    
    Returns:
        ProcessImageResponse with URLs for all processed images
    """
    try:
        logger.info(f"Processing image: {request.image_id} from {request.blob_url}")
        
        # Download original image from blob storage
        original_image = await blob_storage.download_from_url(request.blob_url)
        
        if not original_image:
            raise HTTPException(
                status_code=404,
                detail=f"Image not found at URL: {request.blob_url}"
            )
        
        # Process image into multiple sizes
        processed_images = await image_processor.process_image(
            image_data=original_image,
            image_id=request.image_id,
            sizes=request.sizes or ["thumbnail", "small", "medium", "large"]
        )
        
        # Upload processed images to blob storage
        uploaded_urls = {}
        for size_name, image_data in processed_images.items():
            blob_name = f"processed/{request.image_id}/{size_name}.jpg"
            blob_url = await blob_storage.upload(
                blob_name=blob_name,
                data=image_data,
                content_type="image/jpeg"
            )
            uploaded_urls[size_name] = blob_url
        
        logger.info(f"Successfully processed image {request.image_id} into {len(uploaded_urls)} sizes")
        
        return ProcessImageResponse(
            image_id=request.image_id,
            status="success",
            original_url=request.blob_url,
            processed_urls=uploaded_urls,
            message=f"Image processed successfully into {len(uploaded_urls)} sizes"
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error processing image {request.image_id}: {str(e)}", exc_info=True)
        raise HTTPException(
            status_code=500,
            detail=f"Image processing failed: {str(e)}"
        )


@app.post("/api/upload-and-process")
async def upload_and_process(
    file: UploadFile = File(...),
    image_id: Optional[str] = None,
    sizes: Optional[str] = None
):
    """
    Direct upload and process endpoint (alternative workflow)
    
    Allows direct file upload to this service instead of going through REST API first.
    Useful for testing or alternative integration patterns.
    
    Args:
        file: Image file to upload
        image_id: Optional custom image ID (generated if not provided)
        sizes: Comma-separated list of sizes (e.g., "thumbnail,small,medium")
    
    Returns:
        ProcessImageResponse with URLs for all processed images
    """
    try:
        # Generate image ID if not provided
        if not image_id:
            image_id = f"img_{datetime.utcnow().strftime('%Y%m%d_%H%M%S_%f')}"
        
        logger.info(f"Direct upload: {image_id}, filename: {file.filename}")
        
        # Validate file type
        if not file.content_type or not file.content_type.startswith("image/"):
            raise HTTPException(
                status_code=400,
                detail=f"Invalid file type: {file.content_type}. Must be an image."
            )
        
        # Read file data
        file_data = await file.read()
        
        # Upload original to blob storage
        original_blob_name = f"raw/{image_id}/original{os.path.splitext(file.filename)[1]}"
        original_url = await blob_storage.upload(
            blob_name=original_blob_name,
            data=file_data,
            content_type=file.content_type
        )
        
        # Parse sizes
        size_list = sizes.split(",") if sizes else ["thumbnail", "small", "medium", "large"]
        
        # Process image
        processed_images = await image_processor.process_image(
            image_data=file_data,
            image_id=image_id,
            sizes=size_list
        )
        
        # Upload processed images
        uploaded_urls = {}
        for size_name, image_data in processed_images.items():
            blob_name = f"processed/{image_id}/{size_name}.jpg"
            blob_url = await blob_storage.upload(
                blob_name=blob_name,
                data=image_data,
                content_type="image/jpeg"
            )
            uploaded_urls[size_name] = blob_url
        
        logger.info(f"Direct upload successful: {image_id}")
        
        return ProcessImageResponse(
            image_id=image_id,
            status="success",
            original_url=original_url,
            processed_urls=uploaded_urls,
            message=f"Image uploaded and processed into {len(uploaded_urls)} sizes"
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error in direct upload: {str(e)}", exc_info=True)
        raise HTTPException(
            status_code=500,
            detail=f"Upload and processing failed: {str(e)}"
        )


@app.delete("/api/images/{image_id}")
async def delete_image(image_id: str):
    """
    Delete all processed versions of an image
    
    Args:
        image_id: Image ID to delete
    
    Returns:
        Deletion status
    """
    try:
        logger.info(f"Deleting image: {image_id}")
        
        # Delete all blobs with this image_id prefix
        deleted_count = await blob_storage.delete_by_prefix(f"processed/{image_id}/")
        
        return {
            "image_id": image_id,
            "status": "deleted",
            "files_deleted": deleted_count,
            "message": f"Successfully deleted {deleted_count} files"
        }
        
    except Exception as e:
        logger.error(f"Error deleting image {image_id}: {str(e)}")
        raise HTTPException(
            status_code=500,
            detail=f"Deletion failed: {str(e)}"
        )


@app.get("/api/images/{image_id}")
async def get_image_info(image_id: str):
    """
    Get information about processed image versions
    
    Args:
        image_id: Image ID to query
    
    Returns:
        List of available processed versions with URLs
    """
    try:
        logger.info(f"Getting info for image: {image_id}")
        
        # List all blobs for this image
        blobs = await blob_storage.list_blobs_by_prefix(f"processed/{image_id}/")
        
        if not blobs:
            raise HTTPException(
                status_code=404,
                detail=f"No processed versions found for image: {image_id}"
            )
        
        return {
            "image_id": image_id,
            "versions": blobs,
            "count": len(blobs)
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error getting image info for {image_id}: {str(e)}")
        raise HTTPException(
            status_code=500,
            detail=f"Failed to retrieve image info: {str(e)}"
        )


if __name__ == "__main__":
    import uvicorn
    
    # Get port from environment or use default
    port = int(os.getenv("PORT", 8000))
    
    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=port,
        reload=True,
        log_level="info"
    )