# ADR-002: Run MCP Server as Separate Process from ArcGIS Pro

## Status
Accepted

## Date
2025-12-18

## Context
The MCP protocol requires a server that communicates via stdio (stdin/stdout) JSON-RPC. ArcGIS Pro Add-Ins run in-process with the desktop application and cannot own their process's stdio streams. We need to decide where the MCP server runs.

## Decision
Run the MCP server as a standalone .NET 8 console application (`ArcGisMcpServer.exe`) that communicates with ArcGIS Pro via Named Pipes IPC. The MCP client (VS Copilot, Claude Desktop) starts the MCP server process and communicates over stdio.

```
MCP Client ←→ [stdio] ←→ ArcGisMcpServer.exe ←→ [Named Pipe] ←→ ArcGIS Pro Add-In
```

## Consequences

### Positive
- Clean separation of concerns: MCP protocol handling vs GIS operations
- MCP server can be tested independently (Ping/Echo tools work without ArcGIS)
- Add-In stays focused on ArcGIS SDK interaction
- Each process can fail independently without crashing the other
- Standard MCP server lifecycle (started/stopped by MCP client)

### Negative
- IPC overhead between the two processes
- Two processes to manage and deploy
- IPC models must be kept in sync between projects
- Additional failure mode (pipe disconnection)

### Neutral
- `.mcp.json` at solution root tells MCP clients how to start the server

## Alternatives Considered

1. **Embed MCP server inside the Add-In**: Would avoid IPC entirely, but impossible because Add-Ins cannot own stdio streams of the ArcGIS Pro process.

2. **Use ArcGIS Pro as MCP server with HTTP transport**: MCP supports HTTP/SSE transport, but the ModelContextProtocol NuGet only supports stdio in v0.3.0-preview.2, and embedding an HTTP server in an Add-In adds complexity.
