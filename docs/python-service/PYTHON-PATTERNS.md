# Python Code Patterns - Quick Reference

Common Python patterns you'll see in this service, explained for C# developers.

---

## 🔄 Pattern 1: Decorators

### What They Are
Decorators are like C# attributes but more powerful - they wrap functions with additional behavior.

### C# Equivalent
```csharp
// C#
[HttpPost("/api/users")]
[Authorize]
public IActionResult CreateUser() { }
```

### Python
```python
# Python
@app.post("/api/users")      # Route decorator
@require_auth                 # Auth decorator
async def create_user():
    pass
```

### How They Work
```python
# A decorator is just a function that wraps another function

# Simple decorator
def log_calls(func):
    def wrapper(*args, **kwargs):
        print(f"Calling {func.__name__}")
        result = func(*args, **kwargs)
        print(f"Finished {func.__name__}")
        return result
    return wrapper

# Usage
@log_calls
def say_hello(name):
    return f"Hello {name}"

# Calling say_hello("John") prints:
# Calling say_hello
# Finished say_hello
# Returns: "Hello John"
```

### In This Service
```python
# FastAPI uses decorators to define endpoints
@app.post("/api/process-image")  # ← Decorator defines POST route
async def process_image(request: ProcessImageRequest):
    # This function handles POST requests to /api/process-image
    pass
```

---

## 📦 Pattern 2: Type Hints

### What They Are
Optional type annotations (like C# types, but not enforced at runtime).

### C# vs Python
```csharp
// C# - strongly typed, compile-time checks
public async Task<User> GetUser(int id, string name)
{
    return await database.GetUserAsync(id, name);
}
```

```python
# Python - type hints for clarity, runtime validation optional
async def get_user(id: int, name: str) -> User:
    return await database.get_user(id, name)
```

### Benefits
```python
# 1. IDE autocomplete works better
user: User = get_user(123)
user.  # ← IDE shows User's methods/properties

# 2. Pydantic uses them for validation
class ProcessImageRequest(BaseModel):
    image_id: str          # Must be string
    sizes: List[str]       # Must be list of strings
    quality: int = 85      # Must be int, defaults to 85
```

### Advanced Types
```python
from typing import Optional, List, Dict, Union, Any

# Optional (can be None)
name: Optional[str] = None      # Like string? in C#

# List
sizes: List[str] = []           # Like List<string>

# Dict
config: Dict[str, int] = {}     # Like Dictionary<string, int>

# Union (can be multiple types)
value: Union[int, str]          # Can be int OR string

# Any (any type)
data: Any                       # Like object in C#
```

---

## 🔧 Pattern 3: Context Managers (`with` statement)

### What They Are
Ensure resources are properly cleaned up (like `using` in C#).

### C# Equivalent
```csharp
// C#
using (var connection = new SqlConnection(connectionString))
{
    // Use connection
}  // Automatically disposed here
```

### Python
```python
# Python
with open("file.txt", "r") as file:
    content = file.read()
# File automatically closed here

# Async version
async with httpx.AsyncClient() as client:
    response = await client.get(url)
# Client automatically closed here
```

### In This Service
```python
# blob_storage.py
async def download_from_url(self, blob_url: str) -> Optional[bytes]:
    async with httpx.AsyncClient() as client:  # ← Context manager
        response = await client.get(blob_url)
        return response.content
    # client is automatically closed when we exit the 'with' block
```

---

## 🎨 Pattern 4: List Comprehensions

### What They Are
Concise way to create lists (like LINQ in C#).

### C# LINQ
```csharp
// C# LINQ
var numbers = new List<int> { 1, 2, 3, 4, 5 };
var doubled = numbers.Where(x => x > 2).Select(x => x * 2).ToList();
// [6, 8, 10]
```

### Python Comprehension
```python
# Python list comprehension
numbers = [1, 2, 3, 4, 5]
doubled = [x * 2 for x in numbers if x > 2]
# [6, 8, 10]
```

### More Examples
```python
# Create list of squares
squares = [x**2 for x in range(10)]
# [0, 1, 4, 9, 16, 25, 36, 49, 64, 81]

# Filter and transform
names = ["alice", "bob", "charlie"]
upper_long = [name.upper() for name in names if len(name) > 3]
# ["ALICE", "CHARLIE"]

# Dict comprehension
sizes = ["small", "medium", "large"]
size_map = {size: f"{size}.jpg" for size in sizes}
# {"small": "small.jpg", "medium": "medium.jpg", "large": "large.jpg"}
```

### In This Service
```python
# image_processor.py
processed = {}
for size_name in sizes:
    processed[size_name] = await self._resize_image(image, size_name)

# Could be written as comprehension (but async makes it harder):
# processed = {size: await self._resize_image(image, size) for size in sizes}
```

---

## 🔀 Pattern 5: Unpacking

### What It Is
Extract values from tuples, lists, or dicts.

### Examples
```python
# Tuple unpacking
coordinates = (10, 20)
x, y = coordinates
# x = 10, y = 20

# List unpacking
numbers = [1, 2, 3]
first, second, third = numbers

# Dict unpacking
person = {"name": "John", "age": 30}

# In function call
def greet(name, age):
    print(f"{name} is {age}")

greet(**person)  # Same as greet(name="John", age=30)

# * unpacks lists, ** unpacks dicts
```

### In This Service
```python
# services/image_processor.py
SIZE_PRESETS = {
    "thumbnail": (150, 150, 75),
}

for size_name, (width, height, quality) in SIZE_PRESETS.items():
    #              ↑ Unpacking tuple into 3 variables
    print(f"{size_name}: {width}x{height} at {quality}%")
```

---

## 📝 Pattern 6: F-Strings (String Formatting)

### What They Are
Modern string interpolation (like `$""` in C#).

### C# vs Python
```csharp
// C#
var name = "John";
var age = 30;
var message = $"Hello {name}, you are {age} years old";
```

```python
# Python
name = "John"
age = 30
message = f"Hello {name}, you are {age} years old"
```

### Advanced Usage
```python
# Expressions in f-strings
price = 19.99
message = f"Total: ${price * 1.1:.2f}"
# "Total: $21.99"

# Multi-line f-strings
message = f"""
Name: {name}
Age: {age}
Status: {"Adult" if age >= 18 else "Minor"}
"""

# Debug format (Python 3.8+)
x = 42
print(f"{x=}")  # Prints: x=42
```

### In This Service
```python
# main.py
logger.info(f"Processing image {request.image_id} from {request.blob_url}")
logger.info(f"Successfully processed {len(uploaded_urls)} sizes")

# blob_storage.py
blob_name = f"processed/{image_id}/{size_name}.jpg"
# "processed/img_123/thumbnail.jpg"
```

---

## 🚀 Pattern 7: Async/Await

### What It Is
Non-blocking I/O operations (same concept as C#).

### C# vs Python
```csharp
// C#
public async Task<string> DownloadAsync(string url)
{
    using var client = new HttpClient();
    return await client.GetStringAsync(url);
}
```

```python
# Python
async def download(url: str) -> str:
    async with httpx.AsyncClient() as client:
        response = await client.get(url)
        return response.text
```

### When to Use
```python
# Use async/await for I/O operations:
# ✅ Network requests (HTTP, database)
# ✅ File I/O (reading/writing files)
# ✅ Waiting/sleeping

# ❌ Don't use for CPU-intensive work (use multiprocessing instead)

# Good - I/O bound
async def fetch_data(url):
    async with httpx.AsyncClient() as client:
        return await client.get(url)

# Bad - CPU bound (blocking)
async def calculate_primes(n):
    # This blocks the event loop - don't use async here
    return [i for i in range(2, n) if is_prime(i)]
```

### In This Service
```python
# main.py
@app.post("/api/process-image")
async def process_image(request: ProcessImageRequest):
    # Network I/O - download from blob
    original_image = await blob_storage.download_from_url(request.blob_url)
    
    # CPU work - process image (we use async for consistency)
    processed_images = await image_processor.process_image(original_image)
    
    # Network I/O - upload to blob
    for size_name, image_data in processed_images.items():
        url = await blob_storage.upload(blob_name, image_data)
```

---

## 🎯 Pattern 8: Dictionary Methods

### Common Operations
```python
# Create dict
person = {"name": "John", "age": 30}

# Access
name = person["name"]                    # Raises KeyError if missing
age = person.get("age")                  # Returns None if missing
city = person.get("city", "Unknown")     # Returns "Unknown" if missing

# Update
person["email"] = "john@example.com"     # Add/update
person.update({"phone": "555-1234", "city": "NYC"})  # Bulk update

# Check existence
if "name" in person:
    print(person["name"])

# Iterate
for key in person:                       # Iterate keys
    print(key)

for value in person.values():            # Iterate values
    print(value)

for key, value in person.items():        # Iterate both
    print(f"{key}: {value}")

# Delete
del person["age"]                        # Remove key
removed = person.pop("city", None)       # Remove and return (or default)
```

### In This Service
```python
# image_processor.py
SIZE_PRESETS = {
    "thumbnail": (150, 150, 75),
    "small": (320, 320, 80),
}

# Iterate through presets
for size_name, (width, height, quality) in SIZE_PRESETS.items():
    resized = self._resize(image, width, height, quality)
    results[size_name] = resized
```

---

## 🔍 Pattern 9: Error Handling

### Try/Except (like Try/Catch)
```python
# C#
try {
    result = riskyOperation();
}
catch (SpecificException ex) {
    // Handle specific
}
catch (Exception ex) {
    // Handle all others
}
finally {
    // Always runs
}

# Python
try:
    result = risky_operation()
except SpecificError as e:
    # Handle specific
    pass
except Exception as e:
    # Handle all others
    pass
finally:
    # Always runs
    pass
```

### Raising Exceptions
```python
# C#
throw new InvalidOperationException("Something wrong");

# Python
raise ValueError("Something wrong")
```

### Custom Exceptions
```python
class ImageProcessingError(Exception):
    """Custom exception for image processing failures"""
    pass

# Raise it
if not valid_image:
    raise ImageProcessingError("Invalid image format")
```

### In This Service
```python
# main.py
try:
    original_image = await blob_storage.download_from_url(request.blob_url)
    
    if not original_image:
        raise HTTPException(
            status_code=404,
            detail=f"Image not found at URL: {request.blob_url}"
        )
except HTTPException:
    raise  # Re-raise HTTP exceptions
except Exception as e:
    logger.error(f"Error processing image: {str(e)}", exc_info=True)
    raise HTTPException(status_code=500, detail=str(e))
```

---

## 📊 Pattern 10: Logging

### Better Than Print
```python
# ❌ Don't use print() in production
print("User logged in")  # Goes to stdout, no control

# ✅ Use logging
import logging

logger = logging.getLogger(__name__)

logger.debug("Detailed debug info")     # Development only
logger.info("User logged in")           # Normal operations
logger.warning("Disk space low")        # Potential issues
logger.error("Failed to connect")       # Errors
logger.critical("System shutdown")      # Critical failures
```

### With Exception Info
```python
try:
    risky_operation()
except Exception as e:
    # Include full stack trace
    logger.error("Operation failed", exc_info=True)
    
    # Or just the message
    logger.error(f"Operation failed: {str(e)}")
```

### In This Service
```python
# services/image_processor.py
logger = logging.getLogger(__name__)

async def process_image(self, image_data: bytes, image_id: str):
    logger.info(f"Processing image {image_id}")
    
    try:
        result = await self._resize_image(image_data)
        logger.info(f"Successfully processed {image_id}")
        return result
    except Exception as e:
        logger.error(f"Failed to process {image_id}: {str(e)}", exc_info=True)
        raise
```

---

## 🎓 Summary: Key Patterns

| Pattern | Purpose | Example |
|---------|---------|---------|
| **Decorators** | Add behavior to functions | `@app.post("/api/users")` |
| **Type Hints** | Document expected types | `def func(x: int) -> str:` |
| **Context Managers** | Auto cleanup resources | `with open() as f:` |
| **List Comprehensions** | Create lists concisely | `[x*2 for x in nums]` |
| **Unpacking** | Extract values | `x, y = (1, 2)` |
| **F-Strings** | String formatting | `f"Hello {name}"` |
| **Async/Await** | Non-blocking I/O | `await fetch()` |
| **Dict Operations** | Work with dictionaries | `.get()`, `.items()` |
| **Try/Except** | Error handling | `except Exception as e:` |
| **Logging** | Proper output | `logger.info()` |

---

## 🔗 Quick Reference

### Common Python Operations

```python
# String operations
text = "hello world"
text.upper()              # "HELLO WORLD"
text.capitalize()         # "Hello world"
text.split()              # ["hello", "world"]
text.replace("o", "0")    # "hell0 w0rld"
text.startswith("hello")  # True

# List operations
items = [1, 2, 3]
items.append(4)           # Add to end
items.insert(0, 0)        # Insert at index
items.remove(2)           # Remove value
items.pop()               # Remove and return last
len(items)                # Length
sorted(items)             # Sorted copy

# Dict operations
data = {"key": "value"}
data.keys()               # All keys
data.values()             # All values
data.items()              # Key-value pairs
data.get("key", default)  # Safe access

# File operations
with open("file.txt", "r") as f:
    content = f.read()    # Read all
    lines = f.readlines() # Read lines

# Environment variables
import os
value = os.getenv("VAR_NAME", "default")
```

---

Ready to dive into the Python code now? You have everything you need! 🚀
