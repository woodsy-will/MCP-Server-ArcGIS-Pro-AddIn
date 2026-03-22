# C4 Model: Level 3 - Component Diagram

## MCP Server Components

```mermaid
graph TD
    subgraph "ArcGisMcpServer (.NET 8 Console)"
        direction TB
        PROG["Program.cs<br/>Host Builder + DI"]
        SC["StartupConfigurator<br/>IHostedService"]
        PT["ProTools<br/>[McpServerToolType]<br/>Static MCP tool class"]
        BC["BridgeClient<br/>Named Pipe client"]
        IM1["IpcModels<br/>IpcRequest / IpcResponse"]

        PROG --> SC
        SC -->|"Configure(_client)"| PT
        PROG -->|"AddSingleton"| BC
        PT -->|"OpAsync()"| BC
        BC --> IM1
    end

    style PT fill:#4CAF50,color:white
    style BC fill:#2196F3,color:white
```

### Component Responsibilities

| Component | File | Responsibility |
|-----------|------|----------------|
| **Program** | `Program.cs` | Configures DI container, registers MCP server with stdio transport, starts hosted service |
| **StartupConfigurator** | `Program.cs` | `IHostedService` that injects `BridgeClient` into static `ProTools` on startup |
| **ProTools** | `Tools/ProTools.cs` | Static class with `[McpServerTool]` methods; each method calls `BridgeClient.OpAsync()` |
| **BridgeClient** | `Ipc/BridgeClient.cs` | Creates per-request Named Pipe client connections, serializes/deserializes JSON |
| **IpcModels** | `Ipc/IpcModels.cs` | `IpcRequest(Op, Args)` and `IpcResponse(Ok, Error, Data)` record types |

### MCP Tool Registry

```
[McpServerToolType] ProTools
├── GetActiveMapName()  → "pro.getActiveMapName"
├── ListLayers()        → "pro.listLayers"
├── CountFeatures(layer) → "pro.countFeatures"
├── ZoomToLayer(layer)   → "pro.zoomToLayer"
├── Ping()              → (local, no IPC)
└── Echo(text)          → (local, no IPC)
```

## Add-In Components

```mermaid
graph TD
    subgraph "APBridgeAddIn (ArcGIS Pro In-Process)"
        direction TB
        MOD["Module1<br/>ArcGIS Module singleton"]
        BTN["Button1<br/>Ribbon UI button"]
        PBS["ProBridgeService<br/>Named Pipe server + handler"]
        IM2["IpcModels<br/>IpcRequest / IpcResponse"]
        HANDLERS["HandleAsync switch<br/>Operation dispatch"]

        BTN -->|"StartBridgeService()"| MOD
        MOD -->|"new + Start()"| PBS
        PBS -->|"Deserialize"| IM2
        PBS --> HANDLERS

        HANDLERS -->|"QueuedTask.Run()"| SDK["ArcGIS Pro SDK<br/>MapView, FeatureLayer, etc."]
    end

    style PBS fill:#FF9800,color:white
    style HANDLERS fill:#9C27B0,color:white
```

### Component Responsibilities

| Component | File | Responsibility |
|-----------|------|----------------|
| **Module1** | `Module1.cs` | Add-In module singleton; holds `ProBridgeService` instance; disposes on Pro shutdown |
| **Button1** | `Button1.cs` | Ribbon button; calls `Module1.Current.StartBridgeService()` on click |
| **ProBridgeService** | `ProBridgeService.cs` | Creates pipe server, runs accept loop on background task, dispatches to `HandleAsync` |
| **HandleAsync** | `ProBridgeService.cs` | Switch statement routing `req.Op` to ArcGIS SDK calls wrapped in `QueuedTask.Run()` |
| **IpcModels** | `IpcModels.cs` | Same schema as MCP server side (independently defined) |

### Operation Handler Map

```
HandleAsync(req) switch on req.Op:
├── "pro.getActiveMapName" → MapView.Active?.Map?.Name
├── "pro.listLayers"       → Map.Layers.Select(l => l.Name)
├── "pro.countFeatures"    → FeatureLayer.GetFeatureClass().GetCount()
├── "pro.zoomToLayer"      → MapView.Active.ZoomToAsync(layer)
├── "pro.selectByAttribute"→ FeatureLayer.Select(QueryFilter)
└── default                → error: "op not found"
```

## Interaction Between Components

```mermaid
sequenceDiagram
    participant Client as MCP Client (stdio)
    participant Host as Host Builder
    participant Tools as ProTools
    participant Bridge as BridgeClient
    participant Pipe as Named Pipe
    participant Service as ProBridgeService
    participant SDK as ArcGIS SDK

    Client->>Host: tools/call { name: "CountFeatures", args: { layer: "Buildings" } }
    Host->>Tools: CountFeatures("Buildings")
    Tools->>Bridge: OpAsync("pro.countFeatures", { layer: "Buildings" })
    Bridge->>Bridge: new NamedPipeClientStream()
    Bridge->>Pipe: ConnectAsync(2000ms)
    Bridge->>Pipe: WriteLineAsync(JSON)
    Pipe->>Service: ReadLineAsync()
    Service->>Service: Deserialize → HandleAsync
    Service->>SDK: QueuedTask.Run(() => GetCount())
    SDK-->>Service: count = 1523
    Service->>Pipe: WriteLineAsync({ ok: true, data: { count: 1523 } })
    Pipe-->>Bridge: ReadLineAsync()
    Bridge->>Bridge: Deserialize IpcResponse
    Bridge-->>Tools: IpcResponse
    Tools-->>Host: 1523
    Host-->>Client: { result: 1523 }
```
