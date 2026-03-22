# MCP Server ArcGIS Pro Add-In

## Purpose
Bridges AI agents (GitHub Copilot, Claude, etc.) to ArcGIS Pro via the Model Context Protocol.
Two .NET 8 projects communicate over Windows Named Pipes (`ArcGisProBridgePipe`).

## Architecture

```
AI Agent -> MCP Client (VS/Claude) -> [stdio/JSON-RPC] -> MCP Server (.NET 8)
   -> [Named Pipe / JSON] -> ArcGIS Pro Add-In (in-process) -> ArcGIS Pro SDK
```

### Projects in McpServer.sln
| Project | Path | Target | Role |
|---------|------|--------|------|
| ArcGisMcpServer | `McpServer/ArcGisMcpServer/` | net8.0 | MCP server (stdio transport) |
| APBridgeAddIn | `AddIn/APBridgeAddIn/` | net8.0-windows8.0 | ArcGIS Pro in-process add-in |
| TestBridge | `TestBridge/` | net8.0 | C# pipe test client |

### Key Dependencies
- `ModelContextProtocol` v0.3.0-preview.2
- `Microsoft.Extensions.Hosting` v8.0.0
- ArcGIS Pro SDK assemblies (referenced from `C:\Program Files\ArcGIS\Pro\bin\`)

## IPC Protocol
- **Transport**: Windows Named Pipe `ArcGisProBridgePipe`
- **Direction**: Bidirectional, message mode
- **Encoding**: UTF-8 JSON, newline-delimited
- **Request**: `{ "op": "string", "args": { "key": "value" } }`
- **Response**: `{ "ok": bool, "error": "string|null", "data": object|null }`

### Implemented Operations
| Op String | MCP Tool | Args | Add-In Handler |
|-----------|----------|------|----------------|
| `pro.getActiveMapName` | GetActiveMapName | none | ProBridgeService:93 |
| `pro.listLayers` | ListLayers | none | ProBridgeService:96 |
| `pro.countFeatures` | CountFeatures | `layer` | ProBridgeService:101 |
| `pro.zoomToLayer` | ZoomToLayer | `layer` | ProBridgeService:125 |
| `pro.selectByAttribute` | *not exposed yet* | `layer`, `where` | ProBridgeService:145 |
| *(n/a)* | Ping | none | MCP-only, no IPC |
| *(n/a)* | Echo | `text` | MCP-only, no IPC |

## File Map (source files only)

### MCP Server
- `McpServer/ArcGisMcpServer/Program.cs` - Host builder, DI, `StartupConfigurator`
- `McpServer/ArcGisMcpServer/Tools/ProTools.cs` - `[McpServerToolType]` static class; tools call `BridgeClient`
- `McpServer/ArcGisMcpServer/Ipc/BridgeClient.cs` - Named pipe client; connects per-request
- `McpServer/ArcGisMcpServer/Ipc/IpcModels.cs` - `IpcRequest` / `IpcResponse` records

### Add-In
- `AddIn/APBridgeAddIn/Module1.cs` - Module singleton; holds `ProBridgeService`
- `AddIn/APBridgeAddIn/Button1.cs` - Ribbon button to call `Module1.Current.StartBridgeService()`
- `AddIn/APBridgeAddIn/ProBridgeService.cs` - Named pipe server loop + `HandleAsync` switch
- `AddIn/APBridgeAddIn/IpcModels.cs` - Duplicate of IPC DTOs (same schema, different namespace)

### Test Scripts
- `test_bridge.py` - Python pipe test using `pywin32` (ground truth per AGENTS.md)
- `test_mcp.ps1` - PowerShell MCP server stdio test
- `check_pipes.ps1` - Lists active named pipes to verify bridge is running
- `TestBridge/Program.cs` - C# console pipe test client

## Known Issues / Design Debts

1. **Connect-per-request**: `BridgeClient.SendAsync` opens a new pipe connection for every call (30-50ms overhead). Causes "Pipe Busy" under rapid calls.
2. **Hard 2s timeout**: `BridgeClient.cs:15` - insufficient for heavy geoprocessing.
3. **Static `_client`**: `ProTools.Configure(client)` uses static field, preventing parallel test instances.
4. **Single pipe instance**: `NamedPipeServerStream` maxNumberOfServerInstances=1, sequential only.
5. **No logging**: Neither side logs to `C:\Users\Public\Documents\ArcGIS_MCP_Log.txt` yet (required by AGENTS.md).
6. **`selectByAttribute` not exposed**: Handler exists in Add-In but no MCP tool in `ProTools.cs`.
7. **Hardcoded paths**: `test_bridge.py` and `test_mcp.ps1` reference old absolute paths.

## Build & Run

```powershell
# Build entire solution
dotnet build McpServer.sln

# Run MCP server standalone (stdio)
dotnet run --project McpServer/ArcGisMcpServer/ArcGisMcpServer.csproj

# Run pipe test (requires ArcGIS Pro bridge running)
python test_bridge.py

# Check if bridge pipe exists
powershell ./check_pipes.ps1

# Validate MCP server tools via PowerShell
powershell ./test_mcp.ps1
```

## Development Workflow
1. Start ArcGIS Pro, open a project with map data
2. Click "Start server mcp" button in ribbon (starts pipe server)
3. Run MCP server or test scripts
4. Validate with `test_bridge.py` (5 consecutive clean runs = stable)

## Conventions
- All ArcGIS SDK calls MUST go through `QueuedTask.Run()` for thread safety
- IPC DTOs are duplicated in both projects (no shared project)
- Op strings follow `pro.<camelCase>` convention
- MCP tools are static methods on `ProTools` with `[McpServerTool]` attribute
- `.mcp.json` at solution root configures VS Copilot Agent Mode

## Gotchas
- Add-In requires manual button click to start; no auto-start currently
- `ProBridgeService` pipe loop: if the client disconnects mid-request, the server recreates the pipe and continues
- `BridgeClient` does NOT keep the connection alive; each `OpAsync` is a full connect/send/read/disconnect cycle
- `IpcModels` records are defined in both projects independently; changes must be synced manually
- The `TestBridge` project uses raw byte writes vs `StreamWriter` in `BridgeClient`; behavior may differ
