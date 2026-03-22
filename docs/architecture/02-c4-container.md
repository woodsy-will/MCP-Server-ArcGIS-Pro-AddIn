# C4 Model: Level 2 - Container Diagram

## Container Architecture

```mermaid
C4Container
    title Container Diagram - MCP ArcGIS Pro Bridge

    Person(user, "User", "GIS Analyst / Developer")

    System_Boundary(bridge, "MCP ArcGIS Pro Bridge") {
        Container(mcpServer, "ArcGisMcpServer", ".NET 8 Console App", "MCP server process; receives JSON-RPC via stdio, forwards to Add-In via Named Pipe")
        Container(addIn, "APBridgeAddIn", "ArcGIS Pro Add-In (.NET 8 Windows)", "Runs in-process with ArcGIS Pro; handles Named Pipe requests, executes GIS operations")
        ContainerDb(pipe, "Named Pipe", "ArcGisProBridgePipe", "Bidirectional JSON IPC channel")
    }

    System_Ext(mcpClient, "MCP Client", "VS Copilot / Claude Desktop")
    System_Ext(arcgis, "ArcGIS Pro Runtime", "Map views, layers, feature classes")

    Rel(user, mcpClient, "Natural language")
    Rel(mcpClient, mcpServer, "stdio JSON-RPC 2.0")
    Rel(mcpServer, pipe, "JSON request (connect-per-call)")
    Rel(pipe, addIn, "JSON request")
    Rel(addIn, arcgis, "QueuedTask.Run() SDK calls")
```

## Container Details

### ArcGisMcpServer (MCP Server)

| Property | Value |
|----------|-------|
| **Type** | .NET 8 Console Application |
| **Entry Point** | `McpServer/ArcGisMcpServer/Program.cs` |
| **Transport** | stdio (stdin/stdout) |
| **Protocol** | JSON-RPC 2.0 (MCP specification) |
| **NuGet Packages** | `ModelContextProtocol` 0.3.0-preview.2, `Microsoft.Extensions.Hosting` 8.0.0 |
| **Lifetime** | Started by MCP client, runs until client terminates |
| **Configuration** | `.mcp.json` (solution root) |

**Responsibilities:**
- Register MCP tools via `[McpServerToolType]` attributes
- Receive JSON-RPC tool calls over stdio
- Translate MCP calls to IPC requests via `BridgeClient`
- Return results to MCP client

### APBridgeAddIn (ArcGIS Pro Add-In)

| Property | Value |
|----------|-------|
| **Type** | ArcGIS Pro SDK Add-In (WPF) |
| **Entry Point** | `AddIn/APBridgeAddIn/Module1.cs` |
| **Transport** | Named Pipe server |
| **Target Framework** | net8.0-windows8.0 |
| **Runtime** | In-process with ArcGIS Pro |
| **Activation** | Manual (Button1 click) |
| **Packaging** | `.esriAddinX` file |

**Responsibilities:**
- Host Named Pipe server on background thread
- Parse JSON IPC requests
- Route operations to `HandleAsync` switch
- Execute ArcGIS SDK calls via `QueuedTask.Run()`
- Return JSON responses

### Named Pipe Channel

| Property | Value |
|----------|-------|
| **Name** | `ArcGisProBridgePipe` |
| **Direction** | Bidirectional (InOut) |
| **Mode** | PipeTransmissionMode.Message |
| **Max Instances** | 1 (sequential) |
| **Scope** | Local machine only (`.`) |
| **Options** | PipeOptions.Asynchronous |

## Process Topology

```
┌─────────────────────────────────────────────┐
│                 Windows OS                   │
│                                              │
│  ┌─────────────────────┐                    │
│  │    ArcGIS Pro.exe   │                    │
│  │  ┌───────────────┐  │                    │
│  │  │ APBridgeAddIn │  │  Named Pipe       │
│  │  │ (in-process)  │◄─┼──────────────┐    │
│  │  │               │  │              │    │
│  │  └───────────────┘  │              │    │
│  └─────────────────────┘              │    │
│                                        │    │
│  ┌─────────────────────┐              │    │
│  │ ArcGisMcpServer.exe │              │    │
│  │  (console app)      │──────────────┘    │
│  │                      │                   │
│  │  stdin ◄── MCP Client                   │
│  │  stdout ──► MCP Client                  │
│  └─────────────────────┘                    │
└─────────────────────────────────────────────┘
```

## Deployment View

```mermaid
graph TD
    subgraph "Developer Machine"
        subgraph "ArcGIS Pro Process"
            M[Module1.cs] --> PBS[ProBridgeService]
            B1[Button1.cs] --> M
        end

        subgraph "MCP Server Process"
            P[Program.cs] --> PT[ProTools]
            PT --> BC[BridgeClient]
        end

        PBS <-->|"\\.\pipe\ArcGisProBridgePipe"| BC

        subgraph "MCP Client"
            VS[VS 2022 Copilot]
            CD[Claude Desktop]
        end

        VS <-->|stdio| P
        CD <-->|stdio| P
    end
```
