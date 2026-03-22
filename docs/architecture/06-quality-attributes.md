# Quality Attributes

## Performance

### Latency Budget (per operation)

| Phase | Typical | Worst Case | Bottleneck |
|-------|---------|------------|------------|
| MCP Client → Server (stdio) | ~5ms | ~10ms | Process I/O |
| Tool dispatch | ~1ms | ~2ms | Method call |
| Pipe connect (per-request) | ~10-50ms | ~2000ms (timeout) | Handshake overhead |
| JSON serialization (both ways) | ~2ms | ~5ms | Payload size |
| Pipe transfer | ~2ms | ~5ms | Payload size |
| ArcGIS SDK operation | ~5ms | ~30s+ | Data volume, geoprocessing |
| **Total (simple op)** | **~30ms** | **~2s+** | - |

### Throughput

| Metric | Current | Notes |
|--------|---------|-------|
| Max concurrent connections | 1 | Single pipe instance |
| Theoretical max requests/sec | ~20-30 | Limited by connect-per-request overhead |
| With persistent connection | ~200+ | If BridgeClient kept connection open |

### Performance Risks

1. **Connect-per-request**: 30-50ms overhead per call (see [ADR-003](../adr/003-connect-per-request.md))
2. **Pipe Busy errors**: Second request during processing gets rejected
3. **ArcGIS UI thread blocking**: Heavy operations block the pipe accept loop
4. **No connection pooling**: Cannot parallelize independent operations

## Reliability

### Failure Modes

| Failure | Probability | Impact | Recovery |
|---------|-------------|--------|----------|
| Pipe not found (bridge not started) | Common | Complete failure | User clicks button |
| Pipe busy (concurrent request) | Medium | Single request fails | Retry (not implemented) |
| Connection timeout (2s) | Medium under load | Single request fails | Client retries |
| ArcGIS Pro crash | Low | Complete failure | Restart Pro |
| MCP server crash | Low | Agent loses tools | MCP client restarts server |
| Invalid JSON in pipe | Very Low | Single request fails | Server continues loop |

### Recovery Characteristics

| Scenario | Recovery Time | Mechanism |
|----------|--------------|-----------|
| Client disconnects mid-request | Immediate | Server pipe loop restarts |
| Bridge service not started | Manual | User clicks Button1 |
| ArcGIS Pro restart needed | 30-60s | Manual restart + button click |
| MCP server crash | ~5s | MCP client auto-restarts |

### Missing Reliability Features
- No retry logic in `BridgeClient`
- No health check / heartbeat
- No connection keepalive
- No request queuing on pipe busy
- No graceful degradation (all-or-nothing)

## Scalability

| Dimension | Current Limit | Scaling Strategy |
|-----------|--------------|------------------|
| Concurrent users | 1 | Would need multi-pipe instances |
| Operations/sec | ~20-30 | Persistent connection + queuing |
| Data volume | Limited by pipe buffer | Chunked responses (not implemented) |
| Tool count | ~6 | Add to ProTools + HandleAsync switch |

## Maintainability

### Code Complexity

| Component | Lines of Code | Cyclomatic Complexity | Notes |
|-----------|--------------|----------------------|-------|
| ProBridgeService.cs | 174 | Medium (switch) | Will grow with each new op |
| ProTools.cs | 67 | Low | Repetitive pattern per tool |
| BridgeClient.cs | 31 | Low | Simple send/receive |
| Program.cs | 27 | Low | DI configuration |
| Module1.cs | 60 | Low | Startup lifecycle |

### Extension Cost
Adding a new GIS operation requires changes in **3 locations**:
1. `ProBridgeService.cs` - Add case to `HandleAsync` switch
2. `ProTools.cs` - Add `[McpServerTool]` method
3. Build and reinstall both projects

### Coupling Assessment
- **IpcModels**: Duplicated between projects (no shared project reference). Changes require manual sync.
- **Static ProTools._client**: Tight coupling to single BridgeClient instance
- **Operation strings**: Implicit contract (`"pro.countFeatures"`) with no compile-time validation

## Observability

### Current State: Minimal

| Capability | Status |
|-----------|--------|
| Structured logging | Not implemented |
| Error reporting | MessageBox on fatal errors only |
| Performance metrics | Not implemented |
| Health check | Ping tool (MCP-only, doesn't test pipe) |
| Distributed tracing | Not applicable (local) |

### Target State

| Capability | Implementation |
|-----------|---------------|
| File logging | Both sides log to `C:\Users\Public\Documents\ArcGIS_MCP_Log.txt` |
| Operation audit | Timestamp + op + args + duration + result |
| Error details | Exception type + message + stack trace |
| Pipe diagnostics | Connection count, busy rejections, timeouts |

## Availability

| Metric | Target | Current |
|--------|--------|---------|
| Uptime | While ArcGIS Pro is open + button clicked | Same |
| MTTR (mean time to recover) | < 1 minute | ~1-2 minutes (manual restart) |
| Auto-recovery | Desired | Not implemented |

## Testability

| Test Type | Coverage | Tooling |
|-----------|----------|---------|
| Unit tests | None | Could mock BridgeClient for ProTools |
| Integration tests | `test_bridge.py` (Python/pywin32) | Requires running ArcGIS Pro |
| Smoke tests | `TestBridge/Program.cs` | Requires running ArcGIS Pro |
| MCP protocol tests | `test_mcp.ps1` (PowerShell) | Tests MCP server standalone |
| Pipe connectivity | `check_pipes.ps1` | Lists active pipes |

### Test Gap Analysis
- No automated unit tests
- All integration tests require ArcGIS Pro running
- No CI/CD pipeline
- No mock/stub for ArcGIS SDK calls
