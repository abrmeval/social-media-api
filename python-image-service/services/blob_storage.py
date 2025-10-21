"""
Azure Blob Storage Service

Handles uploading, downloading, and managing images in Azure Blob Storage.
"""

from azure.storage.blob import BlobServiceClient, BlobClient, ContainerClient
from azure.core.exceptions import ResourceNotFoundError
from typing import Optional, List, Dict
import logging
import httpx
from io import BytesIO

logger = logging.getLogger(__name__)


class BlobStorageService:
    """
    Service for interacting with Azure Blob Storage
    
    Handles:
    - Uploading processed images
    - Downloading images from URLs
    - Listing blobs by prefix
    - Deleting blobs
    """
    
    def __init__(self, connection_string: str, container_name: str = "media"):
        """
        Initialize blob storage service
        
        Args:
            connection_string: Azure Storage connection string
            container_name: Container to use for uploads
        """
        if not connection_string:
            raise ValueError("Azure Blob Storage connection string is required")
        
        self.connection_string = connection_string
        self.container_name = container_name
        
        # Initialize clients
        self.blob_service_client = BlobServiceClient.from_connection_string(connection_string)
        self.container_client = self.blob_service_client.get_container_client(container_name)
        
        logger.info(f"BlobStorageService initialized for container: {container_name}")
    
    async def health_check(self) -> bool:
        """
        Check if blob storage connection is healthy
        
        Returns:
            True if connection is healthy, False otherwise
        """
        try:
            # Try to get container properties
            self.container_client.get_container_properties()
            return True
        except Exception as e:
            logger.error(f"Blob storage health check failed: {str(e)}")
            return False
    
    async def upload(
        self,
        blob_name: str,
        data: bytes,
        content_type: str = "image/jpeg",
        overwrite: bool = True
    ) -> str:
        """
        Upload data to blob storage
        
        Args:
            blob_name: Name/path of the blob
            data: Binary data to upload
            content_type: MIME type of the content
            overwrite: Whether to overwrite existing blob
        
        Returns:
            Full URL of the uploaded blob
        """
        try:
            blob_client = self.container_client.get_blob_client(blob_name)
            
            # Upload with metadata
            blob_client.upload_blob(
                data,
                overwrite=overwrite,
                content_settings={
                    "content_type": content_type,
                    "cache_control": "public, max-age=31536000"  # Cache for 1 year
                }
            )
            
            blob_url = blob_client.url
            logger.info(f"Uploaded blob: {blob_name} ({len(data)} bytes)")
            return blob_url
            
        except Exception as e:
            logger.error(f"Error uploading blob {blob_name}: {str(e)}")
            raise
    
    async def download(self, blob_name: str) -> Optional[bytes]:
        """
        Download a blob by name
        
        Args:
            blob_name: Name/path of the blob
        
        Returns:
            Blob data as bytes, or None if not found
        """
        try:
            blob_client = self.container_client.get_blob_client(blob_name)
            download_stream = blob_client.download_blob()
            data = download_stream.readall()
            logger.info(f"Downloaded blob: {blob_name} ({len(data)} bytes)")
            return data
            
        except ResourceNotFoundError:
            logger.warning(f"Blob not found: {blob_name}")
            return None
        except Exception as e:
            logger.error(f"Error downloading blob {blob_name}: {str(e)}")
            raise
    
    async def download_from_url(self, blob_url: str) -> Optional[bytes]:
        """
        Download an image from a blob URL
        
        Args:
            blob_url: Full URL to the blob
        
        Returns:
            Image data as bytes, or None if download fails
        """
        try:
            async with httpx.AsyncClient() as client:
                response = await client.get(blob_url, timeout=30.0)
                response.raise_for_status()
                data = response.content
                logger.info(f"Downloaded from URL: {blob_url} ({len(data)} bytes)")
                return data
                
        except httpx.HTTPError as e:
            logger.error(f"HTTP error downloading from URL {blob_url}: {str(e)}")
            return None
        except Exception as e:
            logger.error(f"Error downloading from URL {blob_url}: {str(e)}")
            return None
    
    async def delete(self, blob_name: str) -> bool:
        """
        Delete a blob
        
        Args:
            blob_name: Name/path of the blob
        
        Returns:
            True if deleted, False if not found
        """
        try:
            blob_client = self.container_client.get_blob_client(blob_name)
            blob_client.delete_blob()
            logger.info(f"Deleted blob: {blob_name}")
            return True
            
        except ResourceNotFoundError:
            logger.warning(f"Blob not found for deletion: {blob_name}")
            return False
        except Exception as e:
            logger.error(f"Error deleting blob {blob_name}: {str(e)}")
            raise
    
    async def delete_by_prefix(self, prefix: str) -> int:
        """
        Delete all blobs with a given prefix
        
        Args:
            prefix: Blob name prefix (e.g., "processed/image123/")
        
        Returns:
            Number of blobs deleted
        """
        try:
            deleted_count = 0
            blobs = self.container_client.list_blobs(name_starts_with=prefix)
            
            for blob in blobs:
                await self.delete(blob.name)
                deleted_count += 1
            
            logger.info(f"Deleted {deleted_count} blobs with prefix: {prefix}")
            return deleted_count
            
        except Exception as e:
            logger.error(f"Error deleting blobs with prefix {prefix}: {str(e)}")
            raise
    
    async def list_blobs_by_prefix(self, prefix: str) -> List[Dict[str, str]]:
        """
        List all blobs with a given prefix
        
        Args:
            prefix: Blob name prefix
        
        Returns:
            List of blob information dictionaries
        """
        try:
            blobs = []
            blob_list = self.container_client.list_blobs(name_starts_with=prefix)
            
            for blob in blob_list:
                blob_client = self.container_client.get_blob_client(blob.name)
                blobs.append({
                    "name": blob.name,
                    "url": blob_client.url,
                    "size": blob.size,
                    "last_modified": blob.last_modified.isoformat() if blob.last_modified else None
                })
            
            logger.info(f"Found {len(blobs)} blobs with prefix: {prefix}")
            return blobs
            
        except Exception as e:
            logger.error(f"Error listing blobs with prefix {prefix}: {str(e)}")
            raise
    
    async def exists(self, blob_name: str) -> bool:
        """
        Check if a blob exists
        
        Args:
            blob_name: Name/path of the blob
        
        Returns:
            True if blob exists, False otherwise
        """
        try:
            blob_client = self.container_client.get_blob_client(blob_name)
            return blob_client.exists()
        except Exception as e:
            logger.error(f"Error checking blob existence {blob_name}: {str(e)}")
            return False
