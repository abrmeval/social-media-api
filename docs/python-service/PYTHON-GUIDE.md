# Python & FastAPI - Complete Guide for Beginners

## 📚 Table of Contents
1. [Python Basics](#python-basics)
2. [Understanding the Service Structure](#understanding-the-service-structure)
3. [File-by-File Explanation](#file-by-file-explanation)
4. [How Everything Works Together](#how-everything-works-together)
5. [Python Best Practices Used](#python-best-practices-used)
6. [Key Differences from C# and Node.js](#key-differences-from-c-and-nodejs)

---

## 🐍 Python Basics

### What is Python?

Python is an **interpreted, dynamically-typed** programming language. Unlike C# (which needs compilation), Python code runs directly.

**Key Characteristics:**
```python
# 1. No semicolons needed
name = "John"
age = 25

# 2. Indentation matters (instead of curly braces)
if age > 18:
    print("Adult")    # This MUST be indented
    print("Can vote") # Same indentation level
else:
    print("Minor")

# 3. Dynamic typing (no need to declare types)
x = 5           # x is an integer
x = "hello"     # Now x is a string - this is OK in Python!

# 4. Everything is an object
number = 42
text = "hello"
list_items = [1, 2, 3]
```

### Python vs C# Quick Comparison

| Feature | C# | Python |
|---------|-----|--------|
| **Type Declaration** | `string name = "John";` | `name = "John"` |
| **Blocks** | `{ }` curly braces | Indentation (4 spaces) |
| **Line Ending** | `;` semicolon | Nothing |
| **Null** | `null` | `None` |
| **Boolean** | `true/false` | `True/False` |
| **Comments** | `// comment` | `# comment` |
| **Method** | `public void MyMethod() { }` | `def my_method():` |
| **Class** | `public class MyClass { }` | `class MyClass:` |

---

## 🏗 Understanding the Service Structure

### What is FastAPI?

**FastAPI** is like **ASP.NET Core** for Python:
- It's a modern web framework
- Built-in API documentation (like Swagger)
- Type validation (using Pydantic)
- Async/await support
- Very fast performance

**Comparison:**

```csharp
// C# ASP.NET Core
[HttpGet("/users/{id}")]
public IActionResult GetUser(int id)
{
    var user = _userService.GetUser(id);
    return Ok(user);
}
```

```python
# Python FastAPI
@app.get("/users/{id}")
async def get_user(id: int):
    user = await user_service.get_user(id)
    return user
```

### Project Architecture

```
python-image-service/          # Root directory
│
├── main.py                    # 🚪 ENTRY POINT - The FastAPI app
│
├── models/                    # 📦 DATA MODELS (like DTOs in C#)
│   ├── __init__.py           # Makes this folder a Python "package"
│   └── process_request.py    # Request/Response models
│
├── services/                  # 🔧 BUSINESS LOGIC (like Services in C#)
│   ├── __init__.py           # Makes this folder a Python "package"
│   ├── image_processor.py    # Image processing logic
│   └── blob_storage.py       # Azure Blob Storage operations
│
├── requirements.txt           # 📋 DEPENDENCIES (like packages in .csproj)
├── .env                       # 🔐 CONFIGURATION (like appsettings.json)
├── .env.example              # Template for .env
├── .gitignore                # Files to ignore in git
├── README.md                 # Full documentation
├── SETUP.md                  # Setup instructions
└── test_service.py           # Testing script
```

---

## 📄 File-by-File Explanation

### 1. **`main.py`** - The Heart of the Application

**What it does:** This is your web server, like `Program.cs` + Controllers in .NET

```python
from fastapi import FastAPI  # Import the framework

# Create the app instance (like builder.Build() in .NET)
app = FastAPI(
    title="Social Media Image Processing Service",
    description="...",
    version="1.0.0"
)

# Define endpoints (like [HttpPost] in C#)
@app.get("/")              # Decorator: defines a GET endpoint
async def root():          # async function (like async Task in C#)
    return {               # Automatic JSON serialization
        "service": "Image Processing",
        "status": "running"
    }

@app.post("/api/process-image")
async def process_image(request: ProcessImageRequest):
    # request is automatically validated and parsed!
    result = await some_processing(request)
    return result
```

**Python Concepts Used:**

1. **Decorators** (`@app.get`, `@app.post`):
   ```python
   # Think of decorators as attributes in C#
   # C#: [HttpPost("/api/users")]
   # Python: @app.post("/api/users")
   
   @app.get("/health")  # This decorator "wraps" the function
   async def health():
       return {"status": "ok"}
   ```

2. **Type Hints** (optional but recommended):
   ```python
   # Tell Python what types you expect (like C# parameters)
   async def process_image(request: ProcessImageRequest) -> ProcessImageResponse:
       #                    ↑ input type              ↑ return type
       pass
   ```

3. **Async/Await** (same as C#):
   ```python
   # C#
   public async Task<User> GetUser(int id)
   {
       return await _database.GetUserAsync(id);
   }
   
   # Python
   async def get_user(id: int) -> User:
       return await database.get_user(id)
   ```

---

### 2. **`models/process_request.py`** - Data Validation Models

**What it does:** Defines the structure of your API requests/responses (like DTOs in C#)

```python
from pydantic import BaseModel, Field

# Pydantic models are like C# classes with data annotations
class ProcessImageRequest(BaseModel):
    # Field() is like [Required], [MaxLength] in C#
    image_id: str = Field(..., description="Unique ID")
    blob_url: str = Field(..., description="Azure blob URL")
    sizes: Optional[List[str]] = Field(
        default=["thumbnail", "small", "medium"],
        description="Sizes to generate"
    )
```

**Python Concepts:**

1. **Class Definition**:
   ```python
   # C#
   public class ProcessImageRequest
   {
       public string ImageId { get; set; }
       public string BlobUrl { get; set; }
   }
   
   # Python
   class ProcessImageRequest(BaseModel):  # Inherits from BaseModel
       image_id: str  # Property with type hint
       blob_url: str
   ```

2. **Optional Types**:
   ```python
   from typing import Optional, List, Dict
   
   # Optional[str] means "str or None" (like string? in C#)
   name: Optional[str] = None
   
   # List[str] means "list of strings" (like List<string> in C#)
   sizes: List[str] = []
   
   # Dict[str, str] means "dictionary" (like Dictionary<string, string>)
   urls: Dict[str, str] = {}
   ```

3. **Field Validation**:
   ```python
   # Automatic validation happens before your function runs!
   age: int = Field(..., gt=0, le=120)  # Greater than 0, less/equal 120
   email: str = Field(..., regex="^.+@.+$")  # Must match regex
   ```

---

### 3. **`models/__init__.py`** - Package Initialization

**What it does:** Makes the `models/` folder a Python "package" (like a namespace)

```python
# This file allows you to do:
from models import ProcessImageRequest

# Instead of:
from models.process_request import ProcessImageRequest

# It's like using directives in C#:
# using SocialMedia.Models;
```

**Why it exists:**
- Python requires `__init__.py` to treat a directory as a package
- It can be empty, or it can export common items
- Think of it as the "public interface" of your package

```python
# models/__init__.py
from .process_request import ProcessImageRequest, ProcessImageResponse

# Now you can import from models directly:
from models import ProcessImageRequest
```

---

### 4. **`services/image_processor.py`** - Image Processing Logic

**What it does:** Handles image resizing, optimization (like a Service class in C#)

```python
from PIL import Image  # Pillow library (like System.Drawing in .NET)

class ImageProcessor:
    """Image processing service"""
    
    # Class-level constant (like static readonly in C#)
    SIZE_PRESETS = {
        "thumbnail": (150, 150, 75),
        "small": (320, 320, 80)
    }
    
    def __init__(self):
        """Constructor - called when you create new instance"""
        self.logger = logging.getLogger(__name__)
    
    async def process_image(self, image_data: bytes) -> Dict[str, bytes]:
        """
        Process image into multiple sizes
        
        Args:     # Like C# XML comments
            image_data: Raw image bytes
        
        Returns:
            Dictionary of size name -> processed bytes
        """
        # Load image from bytes
        image = Image.open(BytesIO(image_data))
        
        # Process each size
        results = {}
        for size_name, (width, height, quality) in self.SIZE_PRESETS.items():
            resized = image.resize((width, height))
            results[size_name] = self._save_to_bytes(resized, quality)
        
        return results
```

**Python Concepts:**

1. **Class Structure**:
   ```python
   class MyClass:
       # Constructor (like C# constructor)
       def __init__(self, param1, param2):
           self.param1 = param1  # self is like 'this' in C#
           self.param2 = param2
       
       # Instance method
       def my_method(self):  # Always takes 'self' as first parameter
           return self.param1 + self.param2
   ```

2. **Dictionaries** (like Dictionary<T,K> in C#):
   ```python
   # Create dictionary
   person = {
       "name": "John",
       "age": 30,
       "city": "New York"
   }
   
   # Access values
   print(person["name"])  # "John"
   
   # Add/update
   person["email"] = "john@example.com"
   
   # Loop through
   for key, value in person.items():
       print(f"{key}: {value}")
   ```

3. **List Comprehensions** (powerful Python feature):
   ```python
   # C# LINQ:
   var numbers = list.Where(x => x > 5).Select(x => x * 2).ToList();
   
   # Python list comprehension:
   numbers = [x * 2 for x in original_list if x > 5]
   ```

---

### 5. **`services/blob_storage.py`** - Azure Storage Operations

**What it does:** Uploads/downloads files to Azure Blob Storage (like BlobServiceClient in .NET)

```python
from azure.storage.blob import BlobServiceClient

class BlobStorageService:
    """Service for Azure Blob Storage operations"""
    
    def __init__(self, connection_string: str, container_name: str):
        """Initialize with connection string"""
        # Create Azure client (same as C#)
        self.blob_service_client = BlobServiceClient.from_connection_string(
            connection_string
        )
        self.container_client = self.blob_service_client.get_container_client(
            container_name
        )
    
    async def upload(self, blob_name: str, data: bytes) -> str:
        """Upload data and return URL"""
        blob_client = self.container_client.get_blob_client(blob_name)
        blob_client.upload_blob(data, overwrite=True)
        return blob_client.url
```

**Python Concepts:**

1. **Import Styles**:
   ```python
   # Import entire module
   import os
   os.path.join("folder", "file.txt")
   
   # Import specific items
   from os.path import join
   join("folder", "file.txt")
   
   # Import with alias
   import azure.storage.blob as asb
   client = asb.BlobServiceClient(...)
   ```

2. **String Formatting** (f-strings):
   ```python
   # Old way
   message = "Hello " + name + ", you are " + str(age)
   
   # New way (f-strings) - like $"..." in C#
   message = f"Hello {name}, you are {age}"
   
   # Multi-line
   message = f"""
   Name: {name}
   Age: {age}
   City: {city}
   """
   ```

---

### 6. **`services/__init__.py`** - Package Exports

Same concept as `models/__init__.py` - makes `services/` a package:

```python
from .image_processor import ImageProcessor
from .blob_storage import BlobStorageService

__all__ = ["ImageProcessor", "BlobStorageService"]
```

---

### 7. **`requirements.txt`** - Dependencies

**What it does:** Lists all Python packages needed (like NuGet packages in .csproj)

```txt
fastapi==0.115.5       # Pin exact version
uvicorn[standard]      # [standard] means "with extra features"
pillow>=11.0.0         # >= means "this version or newer"
```

**How to use:**
```bash
# Install all dependencies (like dotnet restore)
pip install -r requirements.txt

# Add new package
pip install requests
pip freeze > requirements.txt  # Save to file
```

**Python Concepts:**

1. **Package Manager (pip)**:
   ```bash
   # Like NuGet in .NET
   pip install fastapi          # Install package
   pip list                     # Show installed packages
   pip uninstall fastapi        # Remove package
   ```

2. **Virtual Environments** (venv):
   ```bash
   # Create isolated environment (like a project-specific package folder)
   python -m venv venv
   
   # Activate it
   venv\Scripts\activate  # Windows
   
   # Now pip installs only affect this project
   pip install fastapi
   ```

---

### 8. **`.env`** - Configuration File

**What it does:** Stores environment variables (like appsettings.json in .NET)

```ini
# .env file
AZURE_BLOB_STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=https;...
BLOB_CONTAINER_NAME=media
PORT=8000
```

**How to use:**
```python
# Load environment variables
from dotenv import load_dotenv
import os

load_dotenv()  # Reads .env file

# Access variables
connection_string = os.getenv("AZURE_BLOB_STORAGE_CONNECTION_STRING")
port = int(os.getenv("PORT", 8000))  # Default to 8000 if not set
```

---

### 9. **`test_service.py`** - Testing Script

**What it does:** Quick test to verify service is running

```python
import httpx  # Like HttpClient in C#
import asyncio  # For async operations

async def test_health():
    """Test the health endpoint"""
    async with httpx.AsyncClient() as client:
        response = await client.get("http://localhost:8000/health")
        print(response.json())

# Run the async function
asyncio.run(test_health())
```

---

## 🔄 How Everything Works Together

### Request Flow Example:

```
1. Client sends POST request
   ↓
2. FastAPI (main.py) receives request
   ↓
3. Pydantic (models) validates data
   ↓
4. Endpoint function called
   ↓
5. ImageProcessor.process_image() resizes image
   ↓
6. BlobStorageService.upload() saves to Azure
   ↓
7. Return ProcessImageResponse
   ↓
8. FastAPI auto-converts to JSON
   ↓
9. Client receives response
```

### Code Flow:

```python
# 1. CLIENT REQUEST
POST /api/process-image
{
    "image_id": "img123",
    "blob_url": "https://..."
}

# 2. FASTAPI RECEIVES (main.py)
@app.post("/api/process-image")
async def process_image(request: ProcessImageRequest):  # ← Auto-validated!
    
    # 3. DOWNLOAD IMAGE (services/blob_storage.py)
    original = await blob_storage.download_from_url(request.blob_url)
    
    # 4. PROCESS IMAGE (services/image_processor.py)
    processed = await image_processor.process_image(original, request.image_id)
    # Returns: {"thumbnail": bytes, "small": bytes, ...}
    
    # 5. UPLOAD RESULTS (services/blob_storage.py)
    urls = {}
    for size, data in processed.items():
        url = await blob_storage.upload(f"{request.image_id}/{size}.jpg", data)
        urls[size] = url
    
    # 6. RETURN RESPONSE (models/process_request.py)
    return ProcessImageResponse(
        image_id=request.image_id,
        processed_urls=urls,
        status="success"
    )
    # FastAPI auto-converts to JSON ↑
```

---

## ✅ Python Best Practices Used

### 1. **Type Hints** (Python 3.5+)

```python
# Makes code self-documenting and enables IDE autocomplete
def process_image(data: bytes, size: int) -> bytes:
    #              ↑ input types    ↑ return type
    pass
```

### 2. **Async/Await** (for I/O operations)

```python
# Use async for operations that wait (network, disk, database)
async def download_image(url: str) -> bytes:
    async with httpx.AsyncClient() as client:
        response = await client.get(url)
        return response.content

# Call with await
data = await download_image("https://...")
```

### 3. **Context Managers** (`with` statement)

```python
# Automatically closes resources (like using in C#)
# C#: using (var file = File.Open(...)) { }
# Python:
with open("file.txt", "r") as file:
    content = file.read()
# file is automatically closed here

# Works with async too
async with httpx.AsyncClient() as client:
    response = await client.get(url)
# client is automatically closed
```

### 4. **List/Dict Comprehensions**

```python
# Concise way to create lists/dicts
sizes = ["thumbnail", "small", "medium"]

# Create list of tuples
dimensions = [(name, 150 if name == "thumbnail" else 320) for name in sizes]

# Create dictionary
size_map = {name: f"{name}.jpg" for name in sizes}
```

### 5. **Docstrings** (documentation)

```python
def process_image(data: bytes) -> Dict[str, bytes]:
    """
    Process image into multiple sizes.
    
    Args:
        data: Raw image bytes
    
    Returns:
        Dictionary mapping size names to processed image bytes
    
    Raises:
        ValueError: If image data is invalid
    """
    pass
```

### 6. **Logging** (instead of print)

```python
import logging

logger = logging.getLogger(__name__)

# Use logger instead of print()
logger.info("Processing image...")
logger.warning("Image size is large")
logger.error("Failed to process", exc_info=True)  # Includes stack trace
```

### 7. **Error Handling**

```python
try:
    result = risky_operation()
except ValueError as e:
    # Handle specific error
    logger.error(f"Invalid value: {e}")
except Exception as e:
    # Catch all other errors
    logger.error(f"Unexpected error: {e}", exc_info=True)
finally:
    # Always runs (like finally in C#)
    cleanup()
```

---

## 🆚 Key Differences from C# and Node.js

### Indentation is Syntax

```python
# Python - indentation defines blocks
if condition:
    do_something()      # Must be indented
    do_another_thing()  # Same level
else:
    do_else()

# C# - curly braces define blocks
if (condition) {
    doSomething();
    doAnotherThing();
} else {
    doElse();
}
```

### No Semicolons

```python
# Python
name = "John"
age = 30
print(name)

# C# / Node.js
name = "John";
age = 30;
console.log(name);
```

### Dynamic Typing

```python
# Python - type can change
x = 5        # x is int
x = "hello"  # Now x is string - OK!

# C# - strongly typed
int x = 5;
x = "hello";  // ERROR - can't assign string to int
```

### Everything is an Object

```python
# Even numbers have methods!
number = 42
binary = number.to_bytes(2, byteorder='big')

# Strings have tons of methods
text = "hello world"
text.upper()        # "HELLO WORLD"
text.split()        # ["hello", "world"]
text.replace("o", "0")  # "hell0 w0rld"
```

### No `new` Keyword

```python
# Python
processor = ImageProcessor()

# C#
var processor = new ImageProcessor();
```

### Multiple Return Values (tuples)

```python
# Python can return multiple values easily
def get_user():
    return "John", 30, "New York"  # Returns tuple

name, age, city = get_user()  # Unpack tuple

# C# needs tuple syntax
(string, int, string) GetUser() => ("John", 30, "New York");
var (name, age, city) = GetUser();
```

---

## 🎯 Summary

### What This Service Does:

1. **Receives image processing request** (blob URL, image ID)
2. **Downloads image** from Azure Blob Storage
3. **Processes image** into 4 sizes (thumbnail, small, medium, large)
4. **Uploads processed images** back to Blob Storage
5. **Returns URLs** of all processed versions

### File Purposes:

- **`main.py`** → Web server & API endpoints (like Controllers)
- **`models/*.py`** → Data structures (like DTOs)
- **`services/*.py`** → Business logic (like Services)
- **`requirements.txt`** → Dependencies (like .csproj packages)
- **`.env`** → Configuration (like appsettings.json)
- **`__init__.py`** → Makes folders into packages (like namespaces)

### Key Python Concepts:

- **Indentation matters** (not curly braces)
- **No semicolons needed**
- **Dynamic typing** (but use type hints for clarity)
- **`async`/`await`** for I/O operations
- **Everything is an object** with methods
- **Simple, readable syntax**

Want me to explain any specific part in more detail? 😊
