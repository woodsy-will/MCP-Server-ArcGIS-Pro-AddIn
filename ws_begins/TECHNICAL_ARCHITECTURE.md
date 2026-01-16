# MCP Server ArcGIS Pro Add-In - Technical Architecture

## System Overview

This document provides a detailed technical overview of the MCP Server integration with ArcGIS Pro Add-In, including architecture, data flow, and implementation details.

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        AI Agent / User                          │
│                  (GitHub Copilot, Claude, etc.)                 │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ Natural Language Query
                             │
┌────────────────────────────▼────────────────────────────────────┐
│                      MCP Client Layer                           │
│                                                                  │
│  • Visual Studio 2022 (with Copilot Agent Mode)                │
│  • Claude Desktop App                                           │
│  • Any MCP-compatible client                                    │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ stdio (Standard Input/Output)
                             │ JSON-RPC 2.0 Protocol
                             │
┌────────────────────────────▼────────────────────────────────────┐
│                    MCP Server (.NET 8)                          │
│                                                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  Program.cs                                             │   │
│  │  • Host Builder                                         │   │
│  │  • Service Configuration                                │   │
│  │  • MCP Server Registration                              │   │
│  └─────────────────────────────────────────────────────────┘   │
│                             │                                    │
│  ┌─────────────────────────▼─────────────────────────────┐     │
│  │  Tools/ProTools.cs                                     │     │
│  │  • GetActiveMapName()                                  │     │
│  │  • ListLayers()                                        │     │
│  │  • CountFeatures(layer)                                │     │
│  │  • ZoomToLayer(layer)                                  │     │
│  │  • Ping() / Echo(text)                                 │     │
│  └─────────────────────────┬─────────────────────────────┘     │
│                             │                                    │
│  ┌─────────────────────────▼─────────────────────────────┐     │
│  │  Ipc/BridgeClient.cs                                   │     │
│  │  • Named Pipe Client                                   │     │
│  │  • JSON Serialization                                  │     │
│  │  • Request/Response Handling                           │     │
│  └─────────────────────────┬─────────────────────────────┘     │
└────────────────────────────┼────────────────────────────────────┘
                             │
                             │ Named Pipe: "ArcGisProBridgePipe"
                             │ Local Machine Only
                             │ JSON-based IPC
                             │
┌────────────────────────────▼────────────────────────────────────┐
│              ArcGIS Pro Add-In (.NET 8 - Windows)               │
│                                                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  Button1.cs                                             │   │
│  │  • UI Button: "Start server mcp"                       │   │
│  │  • Initializes ProBridgeService                        │   │
│  └─────────────────────────┬─────────────────────────────┘     │
│                             │                                    │
│  ┌─────────────────────────▼─────────────────────────────┐     │
│  │  ProBridgeService.cs                                   │     │
│  │  • Named Pipe Server                                   │     │
│  │  • Request Parser                                      │     │
│  │  • Operation Router                                    │     │
│  │  • Response Formatter                                  │     │
│  └─────────────────────────┬─────────────────────────────┘     │
│                             │                                    │
│  ┌─────────────────────────▼─────────────────────────────┐     │
│  │  Operation Handlers                                    │     │
│  │  • pro.getActiveMapName                                │     │
│  │  • pro.listLayers                                      │     │
│  │  • pro.countFeatures                                   │     │
│  │  • pro.zoomToLayer                                     │     │
│  │  • pro.selectByAttribute                               │     │
│  └─────────────────────────┬─────────────────────────────┘     │
└────────────────────────────┼────────────────────────────────────┘
                             │
                             │ ArcGIS Pro SDK API
                             │
┌────────────────────────────▼────────────────────────────────────┐
│                         ArcGIS Pro                              │
│                                                                  │
│  • MapView.Active                                               │
│  • Map.Layers                                                   │
│  • FeatureLayer operations                                      │
│  • QueuedTask.Run()                                             │
└─────────────────────────────────────────────────────────────────┘
```

---

## Component Details

### 1. MCP Server (.NET 8 Console Application)

**Location:** `McpServer/ArcGisMcpServer/`

**Purpose:** Acts as a bridge between MCP clients and the ArcGIS Pro Add-In

**Key Files:**
- `Program.cs` - Entry point, host configuration
- `Tools/ProTools.cs` - MCP tool definitions
- `Ipc/BridgeClient.cs` - Named Pipe client implementation
- `Ipc/IpcModels.cs` - Data transfer objects

**Dependencies:**
- `Microsoft.Extensions.Hosting` (v8.0.0) - For hosted service pattern
- `ModelContextProtocol` (v0.3.0-preview.2) - MCP protocol implementation

**Communication:**
- **Input:** stdio (JSON-RPC 2.0)
- **Output:** stdio (JSON-RPC 2.0)
- **IPC:** Named Pipes to ArcGIS Add-In

---

### 2. ArcGIS Pro Add-In (Windows Desktop)

**Location:** `AddIn/APBridgeAddIn/`

**Purpose:** Runs inside ArcGIS Pro, exposes GIS operations via IPC

**Key Files:**
- `Config.daml` - Add-In configuration and UI definition
- `Module1.cs` - Add-In module (entry point)
- `Button1.cs` - UI button to start bridge service
- `ProBridgeService.cs` - Named Pipe server and operation handler
- `IpcModels.cs` - Data transfer objects

**Dependencies:**
- ArcGIS Pro SDK for .NET (multiple assemblies)
- .NET 8.0 Windows target

**Communication:**
- **IPC:** Named Pipes server
- **Internal:** ArcGIS Pro SDK API calls

---

## Data Flow

### Example: Counting Features in a Layer

```
1. User Query (AI Agent):
   "How many features are in the Buildings layer?"

2. MCP Client → MCP Server (stdio):
   {
     "jsonrpc": "2.0",
     "id": 1,
     "method": "tools/call",
     "params": {
       "name": "CountFeatures",
       "arguments": {
         "layer": "Buildings"
       }
     }
   }

3. MCP Server → BridgeClient:
   ProTools.CountFeatures("Buildings") called
   
4. BridgeClient → Named Pipe → Add-In:
   {
     "op": "pro.countFeatures",
     "args": {
       "layer": "Buildings"
     }
   }

5. ProBridgeService.HandleAsync():
   - Parses request
   - Calls QueuedTask.Run()
   - Finds FeatureLayer "Buildings"
   - Gets FeatureClass
   - Calls GetCount()

6. Add-In → Named Pipe → BridgeClient:
   {
     "ok": true,
     "error": null,
     "data": {
       "count": 1523
     }
   }

7. MCP Server → MCP Client (stdio):
   {
     "jsonrpc": "2.0",
     "id": 1,
     "result": 1523
   }

8. MCP Client → User:
   "The Buildings layer contains 1,523 features."
```

---

## IPC Protocol Specification

### Transport
- **Mechanism:** Windows Named Pipes
- **Pipe Name:** `"ArcGisProBridgePipe"`
- **Direction:** Bidirectional (InOut)
- **Mode:** Message-based
- **Scope:** Local machine only (`.` server)

### Message Format

**Request:**
```json
{
  "op": "string",           // Operation name (e.g., "pro.listLayers")
  "args": {                 // Optional arguments
    "key": "value"
  }
}
```

**Response:**
```json
{
  "ok": boolean,           // Success/failure flag
  "error": "string",       // Error message (null if ok=true)
  "data": object           // Result data (null if ok=false)
}
```

### Supported Operations

| Operation | Arguments | Returns | Description |
|-----------|-----------|---------|-------------|
| `pro.getActiveMapName` | None | `{ name: string }` | Active map name |
| `pro.listLayers` | None | `string[]` | Layer names array |
| `pro.countFeatures` | `layer: string` | `{ count: number }` | Feature count |
| `pro.zoomToLayer` | `layer: string` | `{ done: boolean }` | Zoom result |
| `pro.selectByAttribute` | `layer: string`<br>`where: string` | `{ done: boolean }` | Selection result |

---

## Threading Model

### MCP Server
- **Main Thread:** Runs Host, handles stdio
- **Background Thread:** BridgeClient creates connections on-demand
- **Async/Await:** All operations are async

### ArcGIS Pro Add-In
- **UI Thread:** Button click handler
- **Background Thread:** ProBridgeService.RunAsync()
- **Named Pipe Thread:** WaitForConnectionAsync(), read/write operations
- **ArcGIS Thread:** QueuedTask.Run() for all GIS operations

**Important:** All ArcGIS SDK calls must run on `QueuedTask.Run()` to ensure thread safety.

---

## Error Handling

### Error Types

1. **Connection Errors**
   - Named Pipe connection timeout (2 seconds)
   - Named Pipe not found (bridge service not started)
   
2. **Serialization Errors**
   - Invalid JSON in request
   - Deserialization failures

3. **Operation Errors**
   - Layer not found
   - Invalid arguments
   - ArcGIS API exceptions

### Error Response Format

```json
{
  "ok": false,
  "error": "Layer 'NonExistent' not found in active map",
  "data": null
}
```

### Retry Strategy
- **MCP Server:** No automatic retry (client handles)
- **Named Pipe:** Single connection attempt with timeout
- **Recommendation:** Implement exponential backoff in production

---

## Performance Characteristics

### Latency Breakdown

Typical operation (e.g., ListLayers):
```
1. MCP Client → Server:        ~5ms   (stdio, local)
2. Server processing:           ~1ms   (tool dispatch)
3. Named Pipe connect:          ~10ms  (first connection)
                                ~1ms   (subsequent)
4. IPC serialization:           ~1ms
5. Named Pipe transfer:         ~2ms
6. Add-In deserialization:      ~1ms
7. ArcGIS operation:            ~5-100ms (depends on data size)
8. Return path:                 ~5ms   (reverse of steps 1-6)

Total: ~30-120ms (typical)
```

### Scalability Considerations

**Current Limitations:**
- Single Named Pipe connection at a time
- Sequential request processing
- No connection pooling

**Recommendations for High Traffic:**
1. Implement connection pooling
2. Support multiple concurrent pipes
3. Add request queuing
4. Cache frequently accessed data

---

## Security Considerations

### Current Implementation
- **Authentication:** None
- **Authorization:** None
- **Encryption:** None (local machine IPC)
- **Access Control:** Windows process isolation only

### Recommended Enhancements

1. **Named Pipe ACL:**
```csharp
var ps = new PipeSecurity();
ps.AddAccessRule(new PipeAccessRule(
    WindowsIdentity.GetCurrent().User,
    PipeAccessRights.FullControl,
    AccessControlType.Allow));
```

2. **Input Validation:**
```csharp
if (!IsValidLayerName(layerName))
    throw new ArgumentException("Invalid layer name");
```

3. **Operation Whitelist:**
```csharp
private static readonly HashSet<string> AllowedOps = new()
{
    "pro.getActiveMapName",
    "pro.listLayers",
    // ... approved operations only
};
```

4. **Logging & Auditing:**
```csharp
logger.LogInformation("Operation {Op} executed by {User}", 
    req.Op, Environment.UserName);
```

---

## Extension Points

### Adding New Operations

**Step 1: Add MCP Tool (MCP Server)**

Edit `McpServer/ArcGisMcpServer/Tools/ProTools.cs`:

```csharp
[McpServerTool, Description("Get map extent as JSON")]
public static async Task<object> GetMapExtent()
{
    var r = await _client!.OpAsync("pro.getMapExtent");
    if (!r.Ok) throw new Exception(r.Error);
    return r.Data!;
}
```

**Step 2: Add Operation Handler (Add-In)**

Edit `AddIn/APBridgeAddIn/ProBridgeService.cs`, in `HandleAsync()` switch:

```csharp
case "pro.getMapExtent":
{
    var extent = await QueuedTask.Run(() =>
    {
        var env = MapView.Active?.Extent;
        return new
        {
            xmin = env?.XMin,
            ymin = env?.YMin,
            xmax = env?.XMax,
            ymax = env?.YMax,
            spatialReference = env?.SpatialReference?.Name
        };
    });
    return new(true, null, extent);
}
```

**Step 3: Rebuild and Deploy**
1. Build both projects
2. Reinstall .esriAddinX file
3. Restart ArcGIS Pro
4. Test new operation

---

## Testing Strategy

### Unit Testing

**MCP Server:**
- Mock BridgeClient for testing tools
- Test serialization/deserialization
- Test error handling

**Add-In:**
- Mock ArcGIS SDK calls (difficult)
- Test ProBridgeService request parsing
- Test IPC message formatting

### Integration Testing

1. **Named Pipe Communication:**
   - Write test client to connect to pipe
   - Send test requests
   - Verify responses

2. **End-to-End:**
   - ArcGIS Pro running with test data
   - Bridge service started
   - MCP Server running
   - Execute all operations
   - Verify results

### Load Testing

- Send N concurrent requests
- Measure response times
- Identify bottlenecks
- Test connection timeout scenarios

---

## Monitoring & Diagnostics

### Recommended Logging

**MCP Server:**
```csharp
_logger.LogInformation("Tool {Tool} called with args {Args}", 
    toolName, args);
_logger.LogError(ex, "Named Pipe connection failed");
```

**Add-In:**
```csharp
ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
    $"Bridge service started: {_pipeName}");
```

### Performance Metrics
- Operation execution time
- Named Pipe connection time
- Request rate
- Error rate

### Health Checks
- Ping tool for MCP server liveness
- Named Pipe connectivity check
- ArcGIS Pro process existence

---

## Deployment Considerations

### Development Environment
- Build configuration: **Debug**
- Detailed error messages
- Extra logging
- Hot reload enabled

### Production Environment
- Build configuration: **Release**
- Optimized binaries
- Error messages sanitized
- Logging to file
- Auto-restart on crash

### Versioning Strategy
- Semantic versioning (MAJOR.MINOR.PATCH)
- Update `Config.daml` version field
- Track compatibility with ArcGIS Pro versions
- Document breaking changes

---

## Known Limitations

1. **Windows Only:**
   - Named Pipes are Windows-specific
   - ArcGIS Pro is Windows-only

2. **Single Instance:**
   - One bridge service per ArcGIS Pro instance
   - No multi-user support

3. **No Persistence:**
   - State lost on restart
   - No operation history

4. **Limited Error Recovery:**
   - No automatic reconnection
   - Manual restart required

5. **Synchronous Processing:**
   - One request at a time
   - No request queuing

---

## Future Enhancements

### Short Term
- [ ] Add more GIS operations (SelectByAttribute, etc.)
- [ ] Implement logging to file
- [ ] Add configuration file support
- [ ] Auto-start bridge service on ArcGIS Pro launch

### Medium Term
- [ ] Support multiple concurrent connections
- [ ] Add request queuing and prioritization
- [ ] Implement caching for frequently accessed data
- [ ] Add health check endpoints

### Long Term
- [ ] Web-based administration UI
- [ ] Support for remote connections (with proper security)
- [ ] Plugin architecture for custom operations
- [ ] Integration with other ESRI products

---

## References

### Documentation
- **ArcGIS Pro SDK:** https://pro.arcgis.com/en/pro-app/latest/sdk/
- **Named Pipes:** https://learn.microsoft.com/en-us/dotnet/standard/io/pipe-operations
- **MCP Protocol:** https://modelcontextprotocol.io/
- **.NET 8:** https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8

### Sample Code
- **ArcGIS Pro SDK Samples:** https://github.com/Esri/arcgis-pro-sdk-community-samples
- **MCP Servers:** https://github.com/modelcontextprotocol/servers

---

## Contact & Support

For technical questions or issues:
- **Repository:** (Add your GitHub repository URL)
- **Issues:** (Add GitHub Issues URL)
- **Email:** (Add support email)

---

**Document Version:** 1.0  
**Last Updated:** December 18, 2025  
**Author:** Setup Assistant
