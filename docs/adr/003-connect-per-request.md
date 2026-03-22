# ADR-003: Connect-Per-Request Named Pipe Pattern

## Status
Accepted (with known debt - see [Architecture Debt D1](../architecture/07-architecture-debt.md#d1-connect-per-request-ipc-pattern))

## Date
2025-12-18

## Context
`BridgeClient` needs to communicate with the Add-In's Named Pipe server. We need to decide whether to keep the pipe connection open (persistent) or create a new connection for each operation.

## Decision
Use connect-per-request: each `SendAsync` call creates a new `NamedPipeClientStream`, connects, sends the request, reads the response, and disposes the connection.

```csharp
public async Task<IpcResponse> SendAsync(IpcRequest req, CancellationToken ct)
{
    using var client = new NamedPipeClientStream(".", _pipeName, ...);
    await client.ConnectAsync(2000, ct);
    // write request, read response
    // client disposed on scope exit
}
```

## Consequences

### Positive
- Simplest implementation: no connection state to manage
- No reconnection logic needed
- No risk of stale/broken connections
- Each request is fully isolated

### Negative
- **30-50ms overhead per call** from pipe handshake
- **"Pipe Busy" risk** if server has maxInstances=1 and a request is in flight
- Poor performance under chatty MCP conversations
- Server must recreate pipe after each client disconnects

### Neutral
- The server-side already supports multiple requests per connection (inner while loop), so migration to persistent client is straightforward

## Alternatives Considered

1. **Persistent connection with reconnect**: Keep pipe open, serialize requests with SemaphoreSlim, reconnect on broken pipe. Better performance, but adds connection lifecycle complexity. **Planned as future improvement (D1).**

2. **Connection pool**: Multiple pipe instances. Rejected for now because the server only allows 1 instance.

## Future Direction
This decision should be revisited once IPC stability is proven. The persistent connection approach is recommended for production use. See debt item D1.
