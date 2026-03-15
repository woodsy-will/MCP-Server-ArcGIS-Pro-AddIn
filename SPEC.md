# SPEC: MCP Server & ArcGIS Pro Bridge

## Goal
Complete the implementation of the Model Context Protocol (MCP) server that interfaces with ArcGIS Pro via Named Pipes. The primary objective is to achieve **Week 1 capabilities**: defining critical GIS operations and ensuring reliable, error-tolerant communication.

## Milestones (Week 1)

### 1. GIS Operations (Tool Definitions)
The following tools must be exposed by the MCP Server and handled by the Add-In:
*   **`get_layer_list`**: Return list of layers in active map.
*   **`get_feature_count(layer_name)`**: Return feature count.
*   **`select_layer_by_attribute(layer_name, query)`**: Select features.
*   **`zoom_to_layer(layer_name)`**: Zoom map view.

### 2. IPC Reliability (Bridge Hardening)
*   **Retry Logic**: The Python client must retry connection if the pipe is busy.
*   **Timeouts**: Operations should timeout after 30 seconds gracefully.
*   **Logging**:
    *   Python side: Python `logging` to console/file.
    *   C# side: `ArcGIS.Desktop.Framework.Dialogs.MessageBox` (for errors) or internal log.

### 3. Architecture
*   **MCP Server (Python)**: Uses `fastmcp` or similar standard library.
*   **Add-In (C#)**: Background thread runs `NamedPipeServerStream`.
*   **Protocol**: JSON-RPC over Named Pipes.

## Verification
*   **Test Script**: `test_bridge.py` must pass 5 consecutive runs without "Pipe Busy" errors.
*   **Manual**: Validated by user in Pro (using the existing "Test Connection" button).
