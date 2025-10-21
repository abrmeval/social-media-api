"""
Pydantic models for image processing requests and responses
"""

from pydantic import BaseModel, Field, HttpUrl
from typing import Optional, Dict, List
from datetime import datetime


class ImageSize(BaseModel):
    """Configuration for an image size"""
    name: str = Field(..., description="Size name (e.g., 'thumbnail', 'small', 'medium', 'large')")
    width: int = Field(..., gt=0, description="Target width in pixels")
    height: int = Field(..., gt=0, description="Target height in pixels")
    quality: int = Field(default=85, ge=1, le=100, description="JPEG quality (1-100)")


class ProcessImageRequest(BaseModel):
    """Request model for image processing"""
    image_id: str = Field(..., description="Unique identifier for the image")
    blob_url: str = Field(..., description="Azure Blob Storage URL of the original image")
    user_id: Optional[str] = Field(None, description="User ID who uploaded the image")
    sizes: Optional[List[str]] = Field(
        default=["thumbnail", "small", "medium", "large"],
        description="List of sizes to generate"
    )
    
    class Config:
        json_schema_extra = {
            "example": {
                "image_id": "img_20251020_123456",
                "blob_url": "https://mystorage.blob.core.windows.net/media/raw/image.jpg",
                "user_id": "user123",
                "sizes": ["thumbnail", "small", "medium", "large"]
            }
        }


class ProcessImageResponse(BaseModel):
    """Response model after image processing"""
    image_id: str = Field(..., description="Image identifier")
    status: str = Field(..., description="Processing status (success/failed)")
    original_url: str = Field(..., description="URL of the original image")
    processed_urls: Dict[str, str] = Field(
        ...,
        description="Dictionary of size names to processed image URLs"
    )
    message: Optional[str] = Field(None, description="Additional information")
    processed_at: str = Field(
        default_factory=lambda: datetime.utcnow().isoformat(),
        description="Timestamp when processing completed"
    )
    
    class Config:
        json_schema_extra = {
            "example": {
                "image_id": "img_20251020_123456",
                "status": "success",
                "original_url": "https://mystorage.blob.core.windows.net/media/raw/image.jpg",
                "processed_urls": {
                    "thumbnail": "https://mystorage.blob.core.windows.net/media/processed/img_20251020_123456/thumbnail.jpg",
                    "small": "https://mystorage.blob.core.windows.net/media/processed/img_20251020_123456/small.jpg",
                    "medium": "https://mystorage.blob.core.windows.net/media/processed/img_20251020_123456/medium.jpg",
                    "large": "https://mystorage.blob.core.windows.net/media/processed/img_20251020_123456/large.jpg"
                },
                "message": "Image processed successfully into 4 sizes",
                "processed_at": "2025-10-20T12:34:56.789Z"
            }
        }


class ErrorResponse(BaseModel):
    """Error response model"""
    error: str = Field(..., description="Error type")
    detail: str = Field(..., description="Detailed error message")
    timestamp: str = Field(
        default_factory=lambda: datetime.utcnow().isoformat(),
        description="Error timestamp"
    )
