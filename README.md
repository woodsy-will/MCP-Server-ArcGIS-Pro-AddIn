# MCP Server with ArcGIS Pro Add-In (C# .NET 8)

This repository demonstrates how to integrate a **Model Context Protocol (MCP) server** with an **ArcGIS Pro Add-In**. The goal is to expose ArcGIS Pro functionality as MCP tools so that GitHub Copilot (in Agent mode) or any MCP client can interact with your GIS environment.

---

## Overview
- **ArcGIS Pro Add-In** (C# with ArcGIS Pro SDK): runs *in-process* with ArcGIS Pro and exposes GIS operations through a local IPC channel (Named Pipes).
- **MCP Server** (.NET 8 console app): defines MCP tools, communicates with the Add-In via Named Pipes, and is configured as an MCP server in Visual Studio through `.mcp.json`.

This allows Copilot (Agent Mode) to query maps, list layers, count features, zoom to layers, and more — directly in ArcGIS Pro.

---

## Prerequisites
- Visual Studio 2022 **17.14 or later** (for MCP Agent Mode support)
- ArcGIS Pro SDK for .NET
- ArcGIS Pro installed (same machine)
- .NET 8 SDK

---

## Solution Structure
```
ArcGisProMcpSample/
+- ArcGisProBridgeAddIn/           # ArcGIS Pro Add-In project (in-process)
¦  +- Config.daml
¦  +- Module.cs
¦  +- ProBridgeService.cs          # Named Pipe server + command handler
¦  +- IpcModels.cs                 # IPC request/response DTOs
+- ArcGisMcpServer/                # MCP server project (.NET 8)
¦  +- Program.cs
¦  +- Tools/ProTools.cs            # MCP tool definitions (bridge client)
¦  +- Tools/HealthTools.cs         # health.ping tool for testing
¦  +- Ipc/BridgeClient.cs          # Named Pipe client
¦  +- Ipc/IpcModels.cs             # Shared IPC DTOs
+- .mcp.json                       # MCP server manifest for VS Copilot
```

---

## ArcGIS Pro Add-In
The Add-In starts a **Named Pipe server** on ArcGIS Pro launch. It handles operations like:
- `pro.getActiveMapName`
- `pro.listLayers`
- `pro.countFeatures`
- `pro.zoomToLayer`

### Example: `Module.cs`
```csharp
protected override bool Initialize()
{
    _service = new ProBridgeService("ArcGisProBridgePipe");
    _service.Start();
    return true; // initialization successful
}

protected override bool CanUnload()
{
    _service?.Dispose();
    return true;
}
```

### Example: `ProBridgeService` handler
```csharp
case "pro.countFeatures":
{
    if (req.Args == null ||
        !req.Args.TryGetValue("layer", out string? layerName) ||
        string.IsNullOrWhiteSpace(layerName))
        return new(false, "arg 'layer' required", null);

    int count = await QueuedTask.Run(() =>
    {
        var fl = MapView.Active?.Map?.Layers
            .OfType<FeatureLayer>()
            .FirstOrDefault(l => l.Name.Equals(layerName, StringComparison.OrdinalIgnoreCase));
        if (fl == null) return 0;
        using var fc = fl.GetFeatureClass();
        return (int)fc.GetCount();
    });

    return new(true, null, new { count });
}
```

---

## MCP Server (.NET 8)
The MCP server uses the official `ModelContextProtocol` NuGet package.

### `Program.cs`
```csharp
await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(new BridgeClient("ArcGisProBridgePipe"));
        services.AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly(typeof(ProTools).Assembly);
    })
    .RunConsoleAsync();
```

### Example tool
```csharp
[McpServerToolType]
public static class ProTools
{
    private static BridgeClient _client;
    public static void Configure(BridgeClient client) => _client = client;

    [McpServerTool(Title = "Count features in a layer", Name = "pro.countFeatures")]
    public static async Task<object> CountFeatures(string layer)
    {
        var r = await _client.OpAsync("pro.countFeatures", new() { ["layer"] = layer });
        if (!r.Ok) throw new Exception(r.Error);
        var count = ((System.Text.Json.JsonElement)r.Data).GetProperty("count").GetInt32();
        return new { layer, count };
    }
}
```

### Health check tool
```csharp
[McpServerToolType]
public static class HealthTools
{
    [McpServerTool(Title = "Ping", Name = "health.ping")]
    public static object Ping() => new { ok = true, ts = DateTimeOffset.UtcNow };
}
```

---

## `.mcp.json` Manifest
Place in solution root (`.mcp.json`):
```json
{
  "servers": {
    "arcgis": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "ArcGisMcpServer/ArcGisMcpServer.csproj"
      ]
    }
  }
}
```
---

## Running in Visual Studio
1. Open the solution in **Visual Studio 2022 (=17.14)**.
2. Ensure ArcGIS Pro is running with the Add-In loaded (so the Named Pipe exists).
3. In VS, open **Copilot Chat ? Agent Mode**.
4. Copilot reads `.mcp.json` and starts the MCP server.
5. Type in chat:
   - `health.ping` ? returns `{ ok: true, ts: ... }`
   - `pro.listLayers` ? returns the layers in the active map
   - `pro.countFeatures layer=Buildings` ? returns the feature count

---

## Best Practices
- Always wrap ArcGIS Pro API calls in **`QueuedTask.Run`**.
- Never use `.Result` or `.Wait()` on async tasks (causes deadlocks).
- Send logs to **`Console.Error`** (leave `Console.Out` for MCP protocol messages only).
- Keep tools small and composable (`pro.*`, `data.*`, `gp.*`).

---

## Next Steps
- Extend tools with operations like `pro.selectByAttribute`, `pro.getCurrentExtent`, `pro.exportLayer`.
- Add retry/timeout logic for IPC communication.
- Package the Add-In as VSIX for easier distribution.
- Containerize the MCP server for deployment.

---
