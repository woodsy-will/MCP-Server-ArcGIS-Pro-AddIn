# BUG REPORT: MCP Server & ArcGIS Pro Bridge

## Executive Summary
**Severity**: 🟠 **Major Performance & Stability Risk**

The Hexagents team (Scout, Deep Diver, Ralph) has reviewed the codebase and identified a critical inefficiency in the IPC (Inter-Process Communication) architecture. The current implementation creates a new Named Pipe connection for *every single request*. Combined with the C# server's single-threaded handling, this will lead to significant latency and "Pipe Busy" errors under load.

## Detailed Findings

### 1. IPC Concurrency Bottleneck (Deep Diver)
*   **Location**: `McpServer/.../Ipc/BridgeClient.cs`
*   **Issue**: `SendAsync` instantiates `NamedPipeClientStream`, connects, writes, reads, and disconnects for every call.
*   **Metric**: Connection handshake overhead is ~30-50ms. For chatty MCP protocols, this adds up fast.
*   **Risk**: If `ProBridgeService` is busy processing a request, a rapid second request from the Client will receive "Pipe Busy" or timeout immediately because of the `2000ms` hard timeout.

### 2. Static State Coupling (Fixer)
*   **Location**: `McpServer/.../Program.cs` -> `StartupConfigurator`
*   **Issue**: `ProTools.Configure(_client)` uses a static field `_client`.
*   **Risk**: Prevents running multiple managed server instances or parallel tests within the same process.

### 3. Hardcoded Timeouts (Validator)
*   **Location**: `BridgeClient.cs` (Line 15)
*   **Issue**: `await client.ConnectAsync(2000, ct);`
*   **Risk**: 2 seconds is insufficient for ArcGIS Pro if it blocks the UI thread (e.g., during a heavy geoprocessing tool run). The client will crash with `TimeoutException` while Pro is just busy.

### 4. Input Validation (Ralph)
*   **Location**: `ProBridgeService.cs` (presumed based on architecture)
*   **Issue**: Ensure `op` strings are strictly allowed-listed. If reflection is used to dispatch arbitrary methods, it allows RCE. (Verification pending `ProBridgeService.cs` read).

## Recommendations
1.  **Refactor BridgeClient**: Implement `PersistentBridgeClient` that connects once and keeps the pipe open, or uses a `Mutex` to serialize requests safely without full reconnection.
2.  **Increase Timeout**: Bump connection timeout to 10s or implemented exponential backoff.
3.  **Implement Queueing**: The Add-In should accept requests and queue them to `QueuedTask.Run` to avoid blocking the pipe accept loop.
