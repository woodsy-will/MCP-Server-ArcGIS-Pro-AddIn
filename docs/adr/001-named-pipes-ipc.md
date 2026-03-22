# ADR-001: Use Windows Named Pipes for IPC

## Status
Accepted

## Date
2025-12-18

## Context
The MCP server runs as a separate process from ArcGIS Pro. We need an IPC mechanism to bridge requests from the MCP server to the Add-In running inside ArcGIS Pro. Requirements:
- Local machine only (both processes on same Windows box)
- Low latency (interactive AI agent conversations)
- Bidirectional request/response
- .NET native support on both sides

## Decision
Use Windows Named Pipes (`System.IO.Pipes.NamedPipeServerStream` / `NamedPipeClientStream`) with JSON-encoded messages, newline-delimited.

Pipe name: `ArcGisProBridgePipe`

## Consequences

### Positive
- Zero configuration: no ports, no firewall rules
- Native .NET support with async APIs
- Message-mode transmission simplifies framing
- Local-only by default (security benefit)
- Low overhead for local IPC (~1-10ms)

### Negative
- Windows-only (acceptable since ArcGIS Pro is Windows-only)
- No built-in authentication (must add PipeSecurity ACL)
- Single instance limit in current implementation
- Debugging harder than HTTP-based IPC

### Neutral
- Pipe name is a well-known string that both sides must agree on

## Alternatives Considered

1. **TCP/IP Loopback (localhost:port)**: Cross-platform, more tooling available. Rejected because it requires port management, firewall rules, and is overkill for local-only communication.

2. **gRPC**: High-performance RPC with strong typing. Rejected because it adds protobuf complexity, requires code generation, and the protocol is already defined by the JSON-based IPC schema.

3. **Memory-Mapped Files**: Shared memory IPC. Rejected because it requires manual synchronization primitives and doesn't support message framing natively.

4. **Windows Messages (WM_COPYDATA)**: Windows messaging. Rejected because it requires window handles and is tied to the UI thread, conflicting with ArcGIS Pro's threading model.
