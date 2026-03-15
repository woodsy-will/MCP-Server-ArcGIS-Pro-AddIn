# AGENTS.md: MCP Server Steering

> [!IMPORTANT]
> **Context**: You are working in the `MCP-Server-ArcGIS-Pro-AddIn` repository. Your goal is to simplify and robustify the Named Pipe implementation between the C# Add-In and the Python/Node MCP Server.

## Directives
1.  **Simplify**: Prefer robust, simple IPC over complex multi-threading if possible.
2.  **Testing**: Use `test_bridge.py` as the ground truth for IPC stability.
3.  **Logs**: Ensure verbose logging in `C:\Users\Public\Documents\ArcGIS_MCP_Log.txt`.

## Tooling
*   **Fixer**: Work in `AddIn/` (C#) and `McpServer/` (Python/Node).
*   **Validator**: Use `powershell ./test_mcp.ps1` to verify.
