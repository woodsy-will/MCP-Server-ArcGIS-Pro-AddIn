"""Test the Named Pipe connection to ArcGIS Pro Bridge"""
import json
import sys

# Try to import win32 pipe module
try:
    import win32pipe
    import win32file
    import pywintypes
    HAS_WIN32 = True
except ImportError:
    HAS_WIN32 = False
    print("pywin32 not available, trying subprocess test instead")

def test_named_pipe():
    """Test direct Named Pipe connection"""
    if not HAS_WIN32:
        print("ERROR: pywin32 not installed. Install with: pip install pywin32")
        return False

    pipe_name = r'\\.\pipe\ArcGisProBridgePipe'

    try:
        print(f"Attempting to connect to Named Pipe: {pipe_name}")

        # Try to open the pipe
        handle = win32file.CreateFile(
            pipe_name,
            win32file.GENERIC_READ | win32file.GENERIC_WRITE,
            0,
            None,
            win32file.OPEN_EXISTING,
            0,
            None
        )

        print("✓ Connected to Named Pipe successfully!")

        # Test with a simple request
        request = {
            "op": "pro.getActiveMapName"
        }

        request_json = json.dumps(request) + '\n'
        print(f"\nSending request: {request}")

        # Write request
        win32file.WriteFile(handle, request_json.encode('utf-8'))

        # Read response
        result, data = win32file.ReadFile(handle, 4096)
        response = data.decode('utf-8')

        print(f"✓ Received response: {response}")

        response_data = json.loads(response)
        if response_data.get('ok'):
            print(f"✓ Success! Data: {response_data.get('data')}")
            return True
        else:
            print(f"✗ Error from bridge: {response_data.get('error')}")
            return False

    except pywintypes.error as e:
        print(f"✗ Failed to connect to Named Pipe: {e}")
        print("\nMake sure:")
        print("  1. ArcGIS Pro is running")
        print("  2. You clicked 'Start server mcp' button")
        print("  3. The bridge service started successfully")
        return False
    except Exception as e:
        print(f"✗ Unexpected error: {e}")
        import traceback
        traceback.print_exc()
        return False
    finally:
        try:
            win32file.CloseHandle(handle)
        except:
            pass

def test_via_dotnet():
    """Test via the MCP server"""
    import subprocess
    import os

    print("\n" + "="*60)
    print("Testing MCP Server (which will test the bridge)")
    print("="*60)

    project_path = r"C:\Users\wsteinley\AAA_CODE_ROOT_FOLDERS\Custom_Tools\Cloned_repos_from_github\MCP-Server-ArcGIS-Pro-AddIn\McpServer\ArcGisMcpServer\ArcGisMcpServer.csproj"

    # Create a test input
    test_request = '{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"Ping"}}\n'

    try:
        result = subprocess.run(
            ['dotnet', 'run', '--project', project_path],
            input=test_request,
            capture_output=True,
            text=True,
            timeout=5
        )

        print("STDOUT:")
        print(result.stdout)

        if result.stderr:
            print("\nSTDERR (info logs):")
            print(result.stderr[:500])

        return True
    except subprocess.TimeoutExpired:
        print("✓ MCP server started (timeout expected for stdio server)")
        return True
    except Exception as e:
        print(f"✗ Error running MCP server: {e}")
        return False

if __name__ == "__main__":
    print("ArcGIS Pro MCP Bridge Connection Test")
    print("="*60)

    # Test Named Pipe directly
    if HAS_WIN32:
        success = test_named_pipe()
    else:
        success = False

    # Always test via dotnet
    test_via_dotnet()

    sys.exit(0 if success else 1)
