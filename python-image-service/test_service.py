"""
Quick test script for Python Image Processing Service

Run this after starting the service to verify it's working correctly.
"""

import httpx
import asyncio
import sys


async def test_health():
    """Test health check endpoint"""
    print("🏥 Testing health check...")
    try:
        async with httpx.AsyncClient() as client:
            response = await client.get("http://localhost:8000/health", timeout=10.0)
            if response.status_code == 200:
                data = response.json()
                print(f"✅ Health check passed: {data['status']}")
                print(f"   Blob Storage: {data['services']['blob_storage']}")
                print(f"   Image Processor: {data['services']['image_processor']}")
                return True
            else:
                print(f"❌ Health check failed: {response.status_code}")
                return False
    except Exception as e:
        print(f"❌ Health check error: {str(e)}")
        return False


async def test_service_info():
    """Test root endpoint"""
    print("\n📋 Testing service info...")
    try:
        async with httpx.AsyncClient() as client:
            response = await client.get("http://localhost:8000/", timeout=10.0)
            if response.status_code == 200:
                data = response.json()
                print(f"✅ Service: {data['service']}")
                print(f"   Version: {data['version']}")
                print(f"   Status: {data['status']}")
                return True
            else:
                print(f"❌ Service info failed: {response.status_code}")
                return False
    except Exception as e:
        print(f"❌ Service info error: {str(e)}")
        return False


async def main():
    """Run all tests"""
    print("=" * 60)
    print("Python Image Processing Service - Quick Test")
    print("=" * 60)
    
    # Test service is running
    health_ok = await test_health()
    info_ok = await test_service_info()
    
    print("\n" + "=" * 60)
    if health_ok and info_ok:
        print("✅ All tests passed! Service is running correctly.")
        print("\n📚 Next steps:")
        print("   - View API docs: http://localhost:8000/docs")
        print("   - View ReDoc: http://localhost:8000/redoc")
        print("   - Test image processing: See README.md for examples")
        sys.exit(0)
    else:
        print("❌ Some tests failed. Check the service logs.")
        print("\n🔍 Troubleshooting:")
        print("   - Is the service running? (uvicorn main:app --reload)")
        print("   - Check .env file has correct Azure credentials")
        print("   - Review logs for errors")
        sys.exit(1)


if __name__ == "__main__":
    asyncio.run(main())
