# MCP Server with ArcGIS Pro Add-In - Windows Setup Guide

## Project Overview
This solution integrates Model Context Protocol (MCP) with ArcGIS Pro, enabling AI agents (like GitHub Copilot) to interact with ArcGIS Pro through natural language commands.

### Architecture
The system consists of two main components:

1. **ArcGIS Pro Add-In** (C# .NET 8, Windows-only)
   - Runs in-process with ArcGIS Pro
   - Exposes GIS operations via Named Pipes IPC
   - Provides a button in ArcGIS Pro to start the bridge service

2. **MCP Server** (.NET 8 Console Application)
   - Communicates with Add-In via Named Pipes
   - Exposes MCP tools for AI agent consumption
   - Can run standalone or via Visual Studio/MCP clients

### Communication Flow
```
AI Agent (Copilot) 
    ↓
MCP Client (Visual Studio/Claude Desktop)
    ↓ (stdio)
MCP Server (.NET Console App)
    ↓ (Named Pipes: "ArcGisProBridgePipe")
ArcGIS Pro Add-In
    ↓
ArcGIS Pro API
```

---

## Prerequisites

### Required Software
- **Windows 10/11** (64-bit)
- **Visual Studio 2022** version 17.14 or later (for MCP Agent Mode support)
- **ArcGIS Pro** (version 3.5 or later recommended)
- **ArcGIS Pro SDK for .NET** ([Download here](https://pro.arcgis.com/en/pro-app/latest/sdk/))
- **.NET 8.0 SDK** ([Download here](https://dotnet.microsoft.com/download/dotnet/8.0))

### Optional (for Advanced Usage)
- **GitHub Copilot** subscription (for VS Code/Visual Studio Agent Mode)
- **Claude Desktop** (for standalone MCP client testing)

---

## Installation Steps

### Step 1: Install Prerequisites

1. **Install Visual Studio 2022** (version 17.14+)
   - During installation, select:
     - ".NET desktop development" workload
     - "Desktop development with C++" workload (for ArcGIS Pro SDK)

2. **Install .NET 8.0 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation: Open PowerShell and run `dotnet --version`
   - Should show version 8.0.x

3. **Install ArcGIS Pro SDK for .NET**
   - Download from: https://pro.arcgis.com/en/pro-app/latest/sdk/
   - This adds templates and tools to Visual Studio
   - Restart Visual Studio after installation

4. **Verify ArcGIS Pro Installation**
   - Ensure ArcGIS Pro is installed at: `C:\Program Files\ArcGIS\Pro\`
   - If installed elsewhere, you'll need to update project references

---

### Step 2: Open and Build the Solution

1. **Open the Solution**
   - Navigate to your repository folder
   - Double-click `McpServer.sln` to open in Visual Studio 2022

2. **Verify Project References**
   - Open the **APBridgeAddIn** project
   - Check that all ArcGIS references resolve correctly
   - If you see yellow warning triangles, verify your ArcGIS Pro installation path

3. **Restore NuGet Packages**
   - In Visual Studio, go to: Tools → NuGet Package Manager → Manage NuGet Packages for Solution
   - Click "Restore" button
   - Required packages:
     - `Microsoft.Extensions.Hosting` (v8.0.0)
     - `ModelContextProtocol` (v0.3.0-preview.2)

4. **Build the Solution**
   - Right-click solution in Solution Explorer → Build Solution
   - Verify both projects build successfully:
     - ✅ ArcGisMcpServer
     - ✅ APBridgeAddIn

---

### Step 3: Deploy the ArcGIS Pro Add-In

1. **Build Configuration**
   - Set build configuration to **Debug** or **Release**
   - Build the **APBridgeAddIn** project

2. **Locate the .esriAddinX File**
   - After successful build, find the Add-In package:
   - Path: `AddIn\APBridgeAddIn\bin\Debug\APBridgeAddIn.esriAddinX`
   - (or `\Release\` if you built in Release mode)

3. **Install the Add-In**
   - Double-click the `.esriAddinX` file
   - ArcGIS Pro Add-In Installation Utility will open
   - Click "Install Add-In"
   - Restart ArcGIS Pro if it's currently running

4. **Verify Add-In Installation**
   - Open ArcGIS Pro
   - Look for the "Add-In" tab in the ribbon
   - You should see "Start server mcp" button under "Group 1"

---

### Step 4: Configure the MCP Server

The `.mcp.json` file is already configured in the repository root:

```json
{
  "inputs": [],
  "servers": {
    "arcgis": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "McpServer/ArcGisMcpServer/ArcGisMcpServer.csproj"
      ],
      "env": {}
    }
  }
}
```

**Important Notes:**
- This configuration assumes you're running from the solution root directory
- The MCP server will be started automatically by MCP clients (VS Code, Claude Desktop, etc.)
- The server communicates via stdio (standard input/output)

---

### Step 5: Test the System

#### Test 1: Start the Bridge Service in ArcGIS Pro

1. Open ArcGIS Pro
2. Create or open a project with a map
3. Click the **"Start server mcp"** button in the Add-In tab
4. You should see a message: "Hello, I start ArcGisProBridgePipe!"
5. The Named Pipe server is now running and waiting for connections

#### Test 2: Test MCP Server (Standalone)

Open PowerShell in the solution root directory:

```powershell
cd McpServer\ArcGisMcpServer
dotnet run
```

The server will start and wait for stdio input. You can test with a simple JSON request:

```json
{"jsonrpc":"2.0","id":1,"method":"tools/list"}
```

To properly test, you need an MCP client. See "Using with Visual Studio" section below.

#### Test 3: Verify Named Pipe Connection

With ArcGIS Pro and the bridge service running:

1. Open PowerShell
2. Navigate to: `McpServer\ArcGisMcpServer`
3. Run: `dotnet run`
4. In another PowerShell window, send a test request using the MCP client

---

## Using with Visual Studio Copilot (Agent Mode)

**Requirements:**
- Visual Studio 2022 version 17.14+
- GitHub Copilot subscription

**Steps:**

1. **Open the Solution in Visual Studio 2022**

2. **Ensure ArcGIS Pro is Running**
   - Open ArcGIS Pro
   - Load a project with map data
   - Click "Start server mcp" button in the Add-In tab

3. **Open Copilot Chat in Agent Mode**
   - In Visual Studio, open the Copilot Chat window
   - Enable "Agent Mode" (look for agent icon)

4. **Verify MCP Server is Loaded**
   - Copilot should automatically detect `.mcp.json`
   - The "arcgis" server should be available

5. **Test with Commands**
   Try these example queries in Copilot Chat:
   
   ```
   @agent What is the active map name?
   @agent List all layers in the current map
   @agent Count features in the Buildings layer
   @agent Zoom to the Roads layer
   ```

---

## Using with Claude Desktop

1. **Install Claude Desktop**
   - Download from: https://claude.ai/download

2. **Configure MCP Server**
   - Copy the contents of `.mcp.json` to Claude Desktop's MCP configuration
   - Location: `%APPDATA%\Claude\claude_desktop_config.json`

3. **Update Configuration**
   Edit the config to use absolute paths:

   ```json
   {
     "mcpServers": {
       "arcgis": {
         "type": "stdio",
         "command": "dotnet",
         "args": [
           "run",
           "--project",
           "C:\\Users\\YourUsername\\Path\\To\\McpServer\\ArcGisMcpServer\\ArcGisMcpServer.csproj"
         ]
       }
     }
   }
   ```

4. **Start ArcGIS Pro** with the Add-In bridge running

5. **Test in Claude Desktop**
   - Open Claude Desktop
   - The ArcGIS tools should be available
   - Try: "What layers are in my active map?"

---

## Available MCP Tools

The following tools are exposed by the MCP server:

### 1. `GetActiveMapName`
**Description:** Returns the name of the active map in ArcGIS Pro

**Parameters:** None

**Example:**
```
What is the active map name?
```

**Returns:** String with map name

---

### 2. `ListLayers`
**Description:** Lists all layers in the active map

**Parameters:** None

**Example:**
```
List all layers in my map
```

**Returns:** Array of layer names

---

### 3. `CountFeatures`
**Description:** Counts features in a specific layer

**Parameters:**
- `layer` (string): Name of the layer

**Example:**
```
Count features in the Buildings layer
```

**Returns:** Integer count

---

### 4. `ZoomToLayer`
**Description:** Zooms the map view to a layer's extent

**Parameters:**
- `layer` (string): Name of the layer

**Example:**
```
Zoom to the Roads layer
```

**Returns:** Boolean success

---

### 5. `Ping` (Test Tool)
**Description:** Simple ping test that doesn't require ArcGIS Pro

**Parameters:** None

**Example:**
```
Ping the MCP server
```

**Returns:** "pong" with timestamp

---

### 6. `Echo` (Test Tool)
**Description:** Echoes back the provided text

**Parameters:**
- `text` (string): Text to echo

**Example:**
```
Echo "Hello MCP"
```

**Returns:** "echo: Hello MCP"

---

## Troubleshooting

### Issue: Add-In doesn't appear in ArcGIS Pro

**Solutions:**
1. Check if the Add-In is installed:
   - Open ArcGIS Pro
   - Go to: Project → Add-In Manager
   - Look for "APBridgeAddIn" in the list
   - If not present, reinstall the `.esriAddinX` file

2. Verify compatibility:
   - Check Config.daml: `desktopVersion="3.5.57366"`
   - Ensure your ArcGIS Pro version is compatible

---

### Issue: MCP Server can't connect to Named Pipe

**Error:** `IOException: bridge no response` or connection timeout

**Solutions:**
1. Verify the bridge service is running:
   - In ArcGIS Pro, click "Start server mcp" button
   - Check for confirmation message

2. Check Named Pipe name matches:
   - Add-In uses: `"ArcGisProBridgePipe"`
   - MCP Server uses: `"ArcGisProBridgePipe"`
   - These must match exactly

3. Restart services:
   - Close ArcGIS Pro
   - Stop MCP Server
   - Start ArcGIS Pro first
   - Click "Start server mcp"
   - Then start MCP Server

---

### Issue: NuGet Package Restore Fails

**Error:** `NU1301: Unable to load the service index`

**Solutions:**
1. Check internet connection
2. Clear NuGet cache:
   ```powershell
   dotnet nuget locals all --clear
   ```
3. Restore in Visual Studio:
   - Tools → NuGet Package Manager → Package Manager Console
   - Run: `Update-Package -reinstall`

---

### Issue: Build Errors with ArcGIS References

**Error:** `Could not find ArcGIS.Desktop.Framework.dll`

**Solutions:**
1. Verify ArcGIS Pro installation path:
   - Default: `C:\Program Files\ArcGIS\Pro\bin\`
   - Check if files exist at this location

2. Update project references:
   - Open `APBridgeAddIn.csproj` in text editor
   - Update `HintPath` elements if ArcGIS is installed elsewhere

3. Reinstall ArcGIS Pro SDK for .NET

---

### Issue: Visual Studio doesn't detect MCP server

**Solutions:**
1. Verify VS version is 17.14 or later:
   - Help → About Microsoft Visual Studio
   - Version should be >= 17.14

2. Check `.mcp.json` location:
   - Must be in solution root directory
   - Must be valid JSON (use a JSON validator)

3. Reload solution:
   - Close solution
   - Delete `.vs` hidden folder
   - Reopen solution

---

## Performance Considerations

### Named Pipe Timeout
- Default timeout: 2000ms (2 seconds)
- Location: `BridgeClient.cs`, line 15
- Increase if experiencing timeout issues:
  ```csharp
  await client.ConnectAsync(5000, ct); // 5 seconds
  ```

### Connection Limits
- Named Pipe allows 1 concurrent connection
- Multiple MCP clients will queue requests
- Consider implementing connection pooling for high-traffic scenarios

---

## Security Notes

### Named Pipe Security
- Current implementation: Local machine only
- Pipe name: `ArcGisProBridgePipe` (no authentication)
- **For production:** Add ACL (Access Control List) to Named Pipe

### Example: Adding ACL Security
```csharp
var ps = new PipeSecurity();
ps.AddAccessRule(new PipeAccessRule(
    WindowsIdentity.GetCurrent().User,
    PipeAccessRights.FullControl,
    AccessControlType.Allow));

var server = new NamedPipeServerStream(
    _pipeName,
    PipeDirection.InOut,
    1,
    PipeTransmissionMode.Message,
    PipeOptions.Asynchronous,
    4096, 4096,
    ps);
```

---

## Next Steps & Extensions

### Potential Enhancements

1. **Additional GIS Operations:**
   - `SelectByAttribute(layer, whereClause)`
   - `GetCurrentExtent()`
   - `ExportLayer(layer, outputPath)`
   - `CreateFeature(layer, geometry, attributes)`
   - `UpdateFeature(layer, objectId, attributes)`

2. **Error Handling:**
   - Retry logic for IPC communication
   - Better error messages for end users
   - Logging to file

3. **Configuration:**
   - User-configurable Named Pipe name
   - Timeout settings
   - Enable/disable specific tools

4. **Auto-Start:**
   - Modify `Module1.cs` to auto-start bridge on ArcGIS Pro launch
   - Remove button requirement

5. **Multi-Connection Support:**
   - Support multiple concurrent MCP clients
   - Connection pooling
   - Request queuing

### Example: Auto-Start Implementation

Modify `AddIn/APBridgeAddIn/Module1.cs`:

```csharp
internal class Module1 : Module
{
    private static Module1 _this = null;
    private ProBridgeService _service;

    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("APBridgeAddIn_Module");

    protected override bool Initialize()
    {
        // Auto-start the bridge service on module initialization
        _service = new ProBridgeService("ArcGisProBridgePipe");
        _service.Start();
        return base.Initialize();
    }

    protected override bool CanUnload()
    {
        _service?.Dispose();
        return true;
    }
}
```

Then update `Config.daml` to set `autoLoad="true"`:
```xml
<insertModule id="APBridgeAddIn_Module" className="Module1" autoLoad="true" caption="Module1">
```

---

## Support & Resources

### Official Documentation
- **ArcGIS Pro SDK:** https://pro.arcgis.com/en/pro-app/latest/sdk/
- **Model Context Protocol:** https://modelcontextprotocol.io/
- **.NET Named Pipes:** https://learn.microsoft.com/en-us/dotnet/standard/io/pipe-operations

### Community
- **ArcGIS Pro SDK Community:** https://community.esri.com/
- **MCP Discord:** https://discord.gg/mcp

### Repository
- **GitHub Issues:** Report bugs and request features
- **Pull Requests:** Contributions welcome!

---

## License

(Add your license information here)

---

## Credits

Developed by: nicogis
Last Updated: December 2025
