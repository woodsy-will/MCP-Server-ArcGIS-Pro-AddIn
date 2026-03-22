# Architecture Debt Register

## Debt Summary

| ID | Debt Item | Severity | Effort | Priority |
|----|-----------|----------|--------|----------|
| D1 | Connect-per-request IPC | High | Medium | P1 |
| D2 | No logging/observability | High | Low | P1 |
| D3 | Hardcoded 2s timeout | Medium | Low | P1 |
| D4 | No Named Pipe ACL | Medium | Low | P2 |
| D5 | Duplicated IpcModels | Low | Low | P2 |
| D6 | Static ProTools._client | Medium | Medium | P3 |
| D7 | selectByAttribute not exposed | Low | Low | P1 |
| D8 | No retry/backoff in BridgeClient | High | Medium | P1 |
| D9 | No input sanitization for SQL where | Medium | Low | P2 |
| D10 | Hardcoded paths in test scripts | Low | Low | P3 |

## Detailed Debt Items

### D1: Connect-Per-Request IPC Pattern

**Location**: `McpServer/ArcGisMcpServer/Ipc/BridgeClient.cs`

**Problem**: Each `SendAsync` creates a new `NamedPipeClientStream`, connects, sends one request, reads one response, and disposes. This adds 30-50ms overhead per call and causes "Pipe Busy" errors under rapid sequential calls.

**Impact**: Latency tax on every operation; unreliable under chatty MCP conversations.

**Remediation**: Implement `PersistentBridgeClient` that connects once and reuses the connection. The server-side already supports multiple requests per connection (inner while loop in `RunAsync`).

```csharp
// Conceptual: Persistent client
public class PersistentBridgeClient : IDisposable
{
    private NamedPipeClientStream? _pipe;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private readonly SemaphoreSlim _lock = new(1);

    public async Task<IpcResponse> SendAsync(IpcRequest req, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            await EnsureConnectedAsync(ct);
            await _writer!.WriteLineAsync(JsonSerializer.Serialize(req));
            var line = await _reader!.ReadLineAsync();
            return JsonSerializer.Deserialize<IpcResponse>(line)!;
        }
        finally { _lock.Release(); }
    }
}
```

**Effort**: Medium (half day)
**Risk**: Must handle reconnection on broken pipe

---

### D2: No Logging / Observability

**Location**: Both projects

**Problem**: Neither the MCP server nor the Add-In logs operations, errors, or diagnostics. AGENTS.md requires logging to `C:\Users\Public\Documents\ArcGIS_MCP_Log.txt`.

**Impact**: Debugging IPC issues requires guesswork. No audit trail for operations.

**Remediation**: Add file-based logging on both sides.

**Effort**: Low (2-3 hours)

---

### D3: Hardcoded 2s Timeout

**Location**: `BridgeClient.cs:15` - `await client.ConnectAsync(2000, ct)`

**Problem**: 2 seconds is insufficient when ArcGIS Pro is busy with heavy geoprocessing. Results in `TimeoutException` while Pro is just temporarily blocked.

**Remediation**: Increase to 10s or make configurable; implement exponential backoff.

**Effort**: Low (30 minutes)

---

### D4: No Named Pipe ACL

**Location**: `ProBridgeService.cs:45-47`

**Problem**: The Named Pipe is created without `PipeSecurity`, allowing any local process to connect.

**Remediation**: Add `PipeSecurity` ACL restricting to current Windows user. See [Security Architecture](05-security.md).

**Effort**: Low (1 hour)

---

### D5: Duplicated IpcModels

**Locations**: `AddIn/APBridgeAddIn/IpcModels.cs` and `McpServer/ArcGisMcpServer/Ipc/IpcModels.cs`

**Problem**: Same records defined in two namespaces. Schema drift requires manual sync.

**Remediation options**:
1. Shared project (`.sharedproj`) referenced by both
2. Accept duplication for now (2 simple records, low drift risk)

**Effort**: Low (1 hour for shared project)

---

### D6: Static ProTools._client Coupling

**Location**: `McpServer/ArcGisMcpServer/Tools/ProTools.cs:11-12`

**Problem**: `ProTools.Configure(client)` sets a static field. Prevents multiple MCP server instances or parallel test execution.

**Root cause**: `ModelContextProtocol` NuGet discovers tools via `[McpServerToolType]` on static classes.

**Remediation**: Investigate if the MCP NuGet supports instance-based tool registration in newer versions.

**Effort**: Medium (depends on NuGet evolution)

---

### D7: selectByAttribute Not Exposed as MCP Tool

**Location**: `ProBridgeService.cs:145-168` (handler exists), `ProTools.cs` (tool missing)

**Problem**: The Add-In handles `pro.selectByAttribute` but no MCP tool is defined.

**Remediation**: Add `[McpServerTool]` method to `ProTools.cs`.

**Effort**: Low (15 minutes)

---

### D8: No Retry/Backoff in BridgeClient

**Location**: `BridgeClient.cs`

**Problem**: Single connection attempt with hard timeout. No retry on "Pipe Busy" or transient failures.

**Remediation**: Implement retry with exponential backoff (3 attempts, 500ms/1s/2s delays).

**Effort**: Medium (2-3 hours including tests)

---

### D9: No Input Sanitization for SQL Where Clauses

**Location**: `ProBridgeService.cs:162`

**Problem**: `where` argument is passed directly to `QueryFilter.WhereClause`. While ArcGIS uses its own query engine, malformed SQL could cause errors or unexpected behavior.

**Remediation**: Add basic validation (reject `--`, `;`, length limits).

**Effort**: Low (1 hour)

---

### D10: Hardcoded Absolute Paths in Test Scripts

**Locations**: `test_bridge.py:91`, `test_mcp.ps1:2`

**Problem**: Both reference old absolute path `C:\Users\wsteinley\AAA_CODE_ROOT_FOLDERS\...` which doesn't match the current repo location.

**Remediation**: Use relative paths or detect repo root dynamically.

**Effort**: Low (30 minutes)

## Remediation Roadmap

### Sprint 1 (Immediate)
- D3: Increase timeout to 10s
- D7: Expose selectByAttribute as MCP tool
- D10: Fix hardcoded test paths
- D2: Add basic file logging

### Sprint 2 (Short-term)
- D8: Add retry logic with backoff
- D4: Add Named Pipe ACL
- D9: Input validation for where clauses

### Sprint 3 (Medium-term)
- D1: Implement persistent BridgeClient
- D5: Evaluate shared IpcModels project
- D6: Monitor MCP NuGet for instance-based tools
