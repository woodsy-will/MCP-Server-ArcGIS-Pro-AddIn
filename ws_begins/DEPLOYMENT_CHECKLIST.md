# MCP Server ArcGIS Pro Add-In - Deployment Checklist

## Pre-Deployment Verification

### Software Requirements
- [ ] Windows 10/11 (64-bit) installed
- [ ] Visual Studio 2022 (version 17.14+) installed
- [ ] .NET 8.0 SDK installed (verify with `dotnet --version`)
- [ ] ArcGIS Pro installed (version 3.5+)
- [ ] ArcGIS Pro SDK for .NET installed
- [ ] GitHub Copilot subscription (optional, for VS Agent Mode)

### File Structure Verification
- [ ] Solution file exists: `McpServer.sln`
- [ ] MCP config exists: `.mcp.json`
- [ ] Add-In project exists: `AddIn/APBridgeAddIn/`
- [ ] MCP Server project exists: `McpServer/ArcGisMcpServer/`
- [ ] README.md present and reviewed

---

## Build & Compilation

### NuGet Package Restoration
- [ ] Open solution in Visual Studio 2022
- [ ] Right-click solution → Restore NuGet Packages
- [ ] Verify packages restored:
  - [ ] Microsoft.Extensions.Hosting (v8.0.0)
  - [ ] ModelContextProtocol (v0.3.0-preview.2)
- [ ] No NuGet errors in Error List window

### Project References
- [ ] Open APBridgeAddIn project properties
- [ ] Verify all ArcGIS references show correct path:
  - [ ] ArcGIS.Desktop.Framework
  - [ ] ArcGIS.Core
  - [ ] ArcGIS.Desktop.Core
  - [ ] ArcGIS.Desktop.Mapping
  - [ ] (and others listed in .csproj)
- [ ] No yellow warning triangles on references

### Build Process
- [ ] Set build configuration: **Debug** or **Release**
- [ ] Build → Build Solution (Ctrl+Shift+B)
- [ ] Build succeeds with 0 errors
- [ ] Verify output:
  - [ ] `AddIn/APBridgeAddIn/bin/{Debug|Release}/APBridgeAddIn.esriAddinX` exists
  - [ ] `McpServer/ArcGisMcpServer/bin/{Debug|Release}/net8.0/` has compiled files

---

## ArcGIS Pro Add-In Installation

### Install Add-In
- [ ] Locate the .esriAddinX file: `AddIn/APBridgeAddIn/bin/{Debug|Release}/APBridgeAddIn.esriAddinX`
- [ ] Double-click the .esriAddinX file
- [ ] ArcGIS Pro Add-In Installation Utility opens
- [ ] Click "Install Add-In" button
- [ ] Installation completes successfully
- [ ] Close ArcGIS Pro if it was running

### Verify Add-In Installation
- [ ] Start ArcGIS Pro
- [ ] Open or create a project
- [ ] Check for "Add-In" tab in the ribbon
- [ ] Verify "Start server mcp" button is visible in "Group 1"
- [ ] Alternative: Project → Add-In Manager → verify "APBridgeAddIn" is listed

---

## Testing

### Test 1: Bridge Service Startup
- [ ] In ArcGIS Pro, create or open a project with a map
- [ ] Ensure the map has at least one layer
- [ ] Click the "Start server mcp" button
- [ ] Message box appears: "Hello, I start ArcGisProBridgePipe!"
- [ ] No error messages

### Test 2: MCP Server Compilation
- [ ] Open PowerShell
- [ ] Navigate to solution root directory
- [ ] Run: `cd McpServer\ArcGisMcpServer`
- [ ] Run: `dotnet build`
- [ ] Build succeeds with 0 errors
- [ ] Run: `dotnet run` (will wait for stdio input)
- [ ] Press Ctrl+C to stop
- [ ] No errors during startup

### Test 3: Named Pipe Communication (Optional)
This requires writing a small test client or using PowerShell scripts.
- [ ] ArcGIS Pro is running with bridge service started
- [ ] Test client can connect to pipe "ArcGisProBridgePipe"
- [ ] Test client can send/receive JSON messages
- [ ] Operations return expected results

---

## Visual Studio Copilot Integration

### Prerequisites
- [ ] Visual Studio 2022 version is 17.14 or later (Help → About)
- [ ] GitHub Copilot extension is installed
- [ ] GitHub Copilot subscription is active

### Configuration
- [ ] `.mcp.json` exists in solution root
- [ ] `.mcp.json` contains valid JSON configuration
- [ ] Project path in `.mcp.json` is correct:
  ```
  "McpServer/ArcGisMcpServer/ArcGisMcpServer.csproj"
  ```

### Testing with Copilot
- [ ] Open solution in Visual Studio 2022
- [ ] Start ArcGIS Pro and click "Start server mcp"
- [ ] Open Copilot Chat window
- [ ] Enable Agent Mode
- [ ] Verify "arcgis" server is detected
- [ ] Test command: "What is the active map name?"
- [ ] Response is received from ArcGIS Pro
- [ ] Test command: "List all layers"
- [ ] Layer list is returned correctly

---

## Claude Desktop Integration (Optional)

### Installation
- [ ] Claude Desktop app installed
- [ ] Configuration file located: `%APPDATA%\Claude\claude_desktop_config.json`

### Configuration
- [ ] Backup existing `claude_desktop_config.json`
- [ ] Update config with absolute paths to your project
- [ ] Configuration follows this format:
  ```json
  {
    "mcpServers": {
      "arcgis": {
        "type": "stdio",
        "command": "dotnet",
        "args": [
          "run",
          "--project",
          "C:\\Full\\Path\\To\\McpServer\\ArcGisMcpServer\\ArcGisMcpServer.csproj"
        ]
      }
    }
  }
  ```
- [ ] JSON is valid (test with JSON validator)

### Testing with Claude Desktop
- [ ] Start ArcGIS Pro with bridge service running
- [ ] Start/Restart Claude Desktop
- [ ] In chat, try: "What layers are in my active map?"
- [ ] Response includes actual layer data from ArcGIS Pro
- [ ] Test other tools: CountFeatures, ZoomToLayer

---

## Production Readiness

### Code Review
- [ ] Review all TODO comments in code
- [ ] Error handling is adequate
- [ ] Logging is configured (if needed)
- [ ] Security considerations addressed (Named Pipe ACL)

### Documentation
- [ ] README.md is complete and accurate
- [ ] SETUP_GUIDE_WINDOWS.md reviewed
- [ ] All example commands tested
- [ ] Troubleshooting section covers common issues

### Performance
- [ ] Test with multiple layers (10+)
- [ ] Test with large feature counts (10,000+)
- [ ] Verify timeouts are appropriate:
  - [ ] Named Pipe connection timeout (default: 2s)
  - [ ] Operation timeouts acceptable

### User Training
- [ ] End users trained on:
  - [ ] Starting ArcGIS Pro
  - [ ] Clicking "Start server mcp" button
  - [ ] Using MCP commands
  - [ ] Common troubleshooting steps

---

## Post-Deployment

### Monitoring
- [ ] Monitor for Named Pipe connection errors
- [ ] Check for Add-In crashes in ArcGIS Pro
- [ ] Review MCP server logs (if logging implemented)

### Feedback Collection
- [ ] Gather user feedback on:
  - [ ] Tool usefulness
  - [ ] Missing features
  - [ ] Performance issues
  - [ ] Error messages clarity

### Maintenance Plan
- [ ] Schedule for updating dependencies
- [ ] Plan for ArcGIS Pro version upgrades
- [ ] MCP protocol updates
- [ ] .NET version updates

---

## Common Issues Quick Reference

### Issue: Add-In not visible in ArcGIS Pro
**Quick Fix:**
1. Go to Project → Add-In Manager
2. Check if APBridgeAddIn is listed
3. If not, reinstall .esriAddinX file
4. Restart ArcGIS Pro

### Issue: "Unable to connect" errors
**Quick Fix:**
1. Restart ArcGIS Pro
2. Click "Start server mcp" button first
3. Then start MCP client
4. Verify both use same pipe name

### Issue: MCP tools not appearing
**Quick Fix:**
1. Check Visual Studio version >= 17.14
2. Verify .mcp.json is in solution root
3. Reload solution in VS
4. Restart Claude Desktop (if using)

### Issue: Build errors with ArcGIS references
**Quick Fix:**
1. Verify ArcGIS Pro installed at: `C:\Program Files\ArcGIS\Pro\`
2. Reinstall ArcGIS Pro SDK for .NET
3. Clean and rebuild solution

---

## Sign-Off

### Deployed By
- Name: ______________________
- Date: ______________________
- Signature: __________________

### Verified By
- Name: ______________________
- Date: ______________________
- Signature: __________________

### Notes
```
(Add any deployment-specific notes, configurations, or customizations here)





```

---

## Version History

| Version | Date | Changes | Deployed By |
|---------|------|---------|-------------|
| 1.0 | 2025-12-18 | Initial deployment | |
| | | | |
| | | | |

---

**Remember:** Always test in a development environment before deploying to production!
