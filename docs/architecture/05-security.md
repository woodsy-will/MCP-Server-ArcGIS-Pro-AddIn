# Security Architecture

## Threat Model

### System Context

```
┌─────────────────────────────────────────────────────────────────┐
│                    Single Windows Machine                        │
│                                                                  │
│  ┌────────┐  stdio  ┌────────────┐  Named Pipe  ┌───────────┐ │
│  │  MCP   │◄───────►│  MCP       │◄────────────►│  ArcGIS   │ │
│  │ Client │         │  Server    │              │  Pro +    │ │
│  │        │         │            │              │  Add-In   │ │
│  └────────┘         └────────────┘              └───────────┘ │
│                                                                  │
│  Trust Boundary: Windows user session                           │
└─────────────────────────────────────────────────────────────────┘
```

### STRIDE Analysis

| Threat | Category | Current Risk | Mitigation Status |
|--------|----------|-------------|-------------------|
| Unauthorized pipe access | **Spoofing** | Medium | No ACL on pipe |
| Malicious op strings | **Tampering** | Low | Switch-based allowlist |
| No audit trail | **Repudiation** | Medium | No logging implemented |
| Sensitive GIS data in pipe | **Info Disclosure** | Low | Local-only, no encryption |
| Pipe flooding / DoS | **Denial of Service** | Medium | Single instance, 2s timeout |
| Arbitrary code via op string | **Elevation of Privilege** | Low | No reflection; switch dispatch |

## Security Boundaries

### Boundary 1: Process Isolation
- MCP Server and ArcGIS Pro run as separate Windows processes
- Named Pipe is the only bridge between them
- No shared memory or direct API calls

### Boundary 2: Named Pipe Access
- **Current**: No ACL configured; any local process can connect
- **Recommended**: Restrict to current Windows user

### Boundary 3: Operation Allowlist
- Operations are dispatched via a switch statement in `ProBridgeService.HandleAsync`
- Unknown operations return `"op not found"` error
- No reflection-based dispatch (mitigates RCE risk)

## Current Security Controls

| Control | Status | Location |
|---------|--------|----------|
| Local-only Named Pipe (server `.`) | Implemented | `BridgeClient.cs:13`, `ProBridgeService.cs:45` |
| Operation allowlist (switch) | Implemented | `ProBridgeService.cs:91-171` |
| Input validation (null/empty checks) | Partial | `ProBridgeService.cs` per-case |
| Pipe ACL | **Not implemented** | - |
| Logging / Audit | **Not implemented** | - |
| Input sanitization (SQL in `where`) | **Not implemented** | `selectByAttribute` |

## Recommended Security Enhancements

### Priority 1: Named Pipe ACL

Add access control to restrict pipe to current user only.

```csharp
// In ProBridgeService.RunAsync(), before creating pipe:
var ps = new PipeSecurity();
ps.AddAccessRule(new PipeAccessRule(
    WindowsIdentity.GetCurrent().User,
    PipeAccessRights.FullControl,
    AccessControlType.Allow));

using var server = NamedPipeServerStream.Create(
    _pipeName, PipeDirection.InOut,
    maxNumberOfServerInstances: 1,
    PipeTransmissionMode.Message,
    PipeOptions.Asynchronous,
    pipeSecurity: ps);
```

### Priority 2: Input Validation

The `where` parameter in `pro.selectByAttribute` is passed directly to `QueryFilter.WhereClause`. While ArcGIS Pro uses its own query engine (not raw SQL), the parameter should still be validated.

```csharp
// Reject obviously dangerous patterns
if (where.Contains("--") || where.Contains(";"))
    return new(false, "invalid where clause characters", null);
```

### Priority 3: Audit Logging

Log all operations to `C:\Users\Public\Documents\ArcGIS_MCP_Log.txt` (per AGENTS.md directive).

```csharp
// Both sides should log:
// - Timestamp
// - Operation name
// - Arguments (sanitized)
// - Success/failure
// - Duration
```

### Priority 4: Rate Limiting

Defend against pipe flooding:
- Limit requests per second (e.g., 10 req/s)
- Reject connections when backlog exceeds threshold

## Data Sensitivity

| Data Type | Sensitivity | Exposure Path |
|-----------|-------------|---------------|
| Map names | Low | IPC response |
| Layer names | Low | IPC response |
| Feature counts | Low | IPC response |
| SQL where clauses | Medium | IPC request args |
| GIS coordinates (future) | Medium-High | IPC response if extent tools added |
| Attribute data (future) | Variable | Depends on data content |

## Authentication & Authorization

**Current state**: None. All local processes can access the pipe.

**Recommended approach**: Since this is a local single-user tool:
1. Named Pipe ACL (Priority 1 above)
2. Optional shared secret in pipe handshake (future, if needed)
3. No need for full auth framework in current scope
