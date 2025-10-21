"""
Image Processing Service using Pillow (PIL)

Handles image resizing, thumbnail generation, format conversion, and optimization.
"""

from PIL import Image, ImageOps
from io import BytesIO
from typing import Dict, List
import logging

logger = logging.getLogger(__name__)


class ImageProcessor:
    """
    Image processing service for social media images
    
    Supports:
    - Multiple size generation (thumbnail, small, medium, large)
    - Automatic aspect ratio preservation
    - JPEG optimization
    - EXIF orientation correction
    """
    
    # Predefined image sizes (width, height, quality)
    SIZE_PRESETS = {
        "thumbnail": (150, 150, 75),    # Small thumbnail
        "small": (320, 320, 80),        # Mobile view
        "medium": (640, 640, 85),       # Tablet/desktop feed
        "large": (1200, 1200, 90),      # Full-size view
        "xlarge": (1920, 1920, 90)      # High-res display
    }
    
    def __init__(self):
        """Initialize image processor"""
        logger.info("ImageProcessor initialized")
    
    async def process_image(
        self,
        image_data: bytes,
        image_id: str,
        sizes: List[str] = None
    ) -> Dict[str, bytes]:
        """
        Process an image into multiple sizes
        
        Args:
            image_data: Raw image bytes
            image_id: Unique image identifier
            sizes: List of size names to generate (uses SIZE_PRESETS)
        
        Returns:
            Dictionary mapping size names to processed image bytes
        """
        if sizes is None:
            sizes = ["thumbnail", "small", "medium", "large"]
        
        logger.info(f"Processing image {image_id} into sizes: {sizes}")
        
        try:
            # Load image from bytes
            image = Image.open(BytesIO(image_data))
            
            # Auto-rotate based on EXIF data
            image = ImageOps.exif_transpose(image)
            
            # Convert to RGB if necessary (handles RGBA, grayscale, etc.)
            if image.mode in ('RGBA', 'LA', 'P'):
                # Create white background for transparency
                background = Image.new('RGB', image.size, (255, 255, 255))
                if image.mode == 'P':
                    image = image.convert('RGBA')
                background.paste(image, mask=image.split()[-1] if image.mode in ('RGBA', 'LA') else None)
                image = background
            elif image.mode != 'RGB':
                image = image.convert('RGB')
            
            # Process each requested size
            processed = {}
            for size_name in sizes:
                if size_name not in self.SIZE_PRESETS:
                    logger.warning(f"Unknown size preset: {size_name}, skipping")
                    continue
                
                try:
                    processed_bytes = await self._resize_image(
                        image=image,
                        size_name=size_name
                    )
                    processed[size_name] = processed_bytes
                    logger.info(f"Generated {size_name} version ({len(processed_bytes)} bytes)")
                except Exception as e:
                    logger.error(f"Failed to generate {size_name}: {str(e)}")
                    # Continue with other sizes even if one fails
            
            logger.info(f"Successfully processed {len(processed)}/{len(sizes)} sizes for {image_id}")
            return processed
            
        except Exception as e:
            logger.error(f"Error processing image {image_id}: {str(e)}", exc_info=True)
            raise ValueError(f"Image processing failed: {str(e)}")
    
    async def _resize_image(
        self,
        image: Image.Image,
        size_name: str
    ) -> bytes:
        """
        Resize image to specific preset size
        
        Args:
            image: PIL Image object
            size_name: Name of size preset
        
        Returns:
            JPEG image bytes
        """
        max_width, max_height, quality = self.SIZE_PRESETS[size_name]
        
        # Calculate new dimensions maintaining aspect ratio
        original_width, original_height = image.size
        aspect_ratio = original_width / original_height
        
        if original_width > original_height:
            # Landscape
            new_width = min(max_width, original_width)
            new_height = int(new_width / aspect_ratio)
        else:
            # Portrait or square
            new_height = min(max_height, original_height)
            new_width = int(new_height * aspect_ratio)
        
        # Ensure dimensions don't exceed maximum
        if new_width > max_width:
            new_width = max_width
            new_height = int(new_width / aspect_ratio)
        if new_height > max_height:
            new_height = max_height
            new_width = int(new_height * aspect_ratio)
        
        # Resize using high-quality Lanczos resampling
        resized = image.resize(
            (new_width, new_height),
            Image.Resampling.LANCZOS
        )
        
        # Save to bytes buffer as JPEG
        buffer = BytesIO()
        resized.save(
            buffer,
            format="JPEG",
            quality=quality,
            optimize=True,
            progressive=True
        )
        
        return buffer.getvalue()
    
    def get_image_info(self, image_data: bytes) -> Dict[str, any]:
        """
        Get metadata about an image
        
        Args:
            image_data: Raw image bytes
        
        Returns:
            Dictionary with image metadata
        """
        try:
            image = Image.open(BytesIO(image_data))
            
            return {
                "format": image.format,
                "mode": image.mode,
                "width": image.size[0],
                "height": image.size[1],
                "size_bytes": len(image_data),
                "has_exif": bool(image._getexif()) if hasattr(image, '_getexif') else False
            }
        except Exception as e:
            logger.error(f"Error getting image info: {str(e)}")
            return {"error": str(e)}
