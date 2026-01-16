# Quick Start Guide - MCP Server ArcGIS Pro Add-In

Get up and running in 15 minutes!

---

## Prerequisites Check

Before starting, ensure you have:
- ✅ Windows 10/11 (64-bit)
- ✅ Visual Studio 2022 (v17.14+)
- ✅ .NET 8.0 SDK
- ✅ ArcGIS Pro installed
- ✅ ArcGIS Pro SDK for .NET

**Quick verify:**
```powershell
dotnet --version  # Should show 8.0.x
```

---

## Step 1: Build the Solution (5 minutes)

1. **Open the solution:**
   - Double-click `McpServer.sln`
   - Wait for Visual Studio to load

2. **Restore packages:**
   - Right-click solution → "Restore NuGet Packages"
   - Wait for completion

3. **Build:**
   - Press `Ctrl+Shift+B` (or Build → Build Solution)
   - Verify: **Build succeeded** in Output window

---

## Step 2: Install the ArcGIS Add-In (3 minutes)

1. **Locate the Add-In file:**
   ```
   AddIn\APBridgeAddIn\bin\Debug\APBridgeAddIn.esriAddinX
   ```

2. **Install:**
   - Double-click the `.esriAddinX` file
   - Click "Install Add-In" in the installer dialog
   - Close installer

3. **Verify:**
   - Open ArcGIS Pro
   - Look for "Add-In" tab → "Start server mcp" button

---

## Step 3: Test the System (5 minutes)

### Test 1: Start the Bridge Service

1. In ArcGIS Pro, open or create a project with a map
2. Click **"Start server mcp"** button
3. You should see: "Hello, I start ArcGisProBridgePipe!"

✅ **Success!** The bridge service is running.

---

### Test 2: Test MCP Server Standalone

1. Open PowerShell in your project directory
2. Run:
   ```powershell
   cd McpServer\ArcGisMcpServer
   dotnet run
   ```
3. Server starts and waits for input
4. Press `Ctrl+C` to stop

✅ **Success!** The MCP server compiles and runs.

---

### Test 3: Test with Visual Studio Copilot (Optional)

**Requirements:** VS 2022 v17.14+ with GitHub Copilot

1. Ensure ArcGIS Pro is running with bridge service started
2. Open the solution in Visual Studio 2022
3. Open Copilot Chat window
4. Try these commands:
   ```
   What is the active map name?
   List all layers
   Count features in the [LayerName] layer
   ```

✅ **Success!** You can now control ArcGIS Pro with AI!

---

## Common Issues & Quick Fixes

### Issue: Add-In not visible in ArcGIS Pro

**Fix:**
1. Go to: Project → Add-In Manager
2. Check if "APBridgeAddIn" is listed
3. If not, reinstall the .esriAddinX file
4. Restart ArcGIS Pro

---

### Issue: "Unable to connect to Named Pipe"

**Fix:**
1. Ensure ArcGIS Pro is running
2. Click "Start server mcp" button FIRST
3. Then start the MCP server/client
4. Verify pipe name matches: `"ArcGisProBridgePipe"`

---

### Issue: Build errors with ArcGIS references

**Fix:**
1. Verify ArcGIS Pro is installed at: `C:\Program Files\ArcGIS\Pro\`
2. Reinstall: ArcGIS Pro SDK for .NET
3. Close and reopen Visual Studio

---

### Issue: NuGet restore fails

**Fix:**
1. Clear cache:
   ```powershell
   dotnet nuget locals all --clear
   ```
2. In Visual Studio: Tools → Options → NuGet Package Manager → Clear All NuGet Caches
3. Restore again

---

## Available Commands

Once everything is running, try these commands with your MCP client:

### Basic Operations
- `What is the active map name?`
- `List all layers`
- `Ping the MCP server` (doesn't require ArcGIS)

### Layer Operations
- `Count features in the [LayerName] layer`
- `Zoom to the [LayerName] layer`

### Advanced (if implemented)
- `Select features where [condition]`
- `Get the current map extent`

---

## Next Steps

### Learn More
- 📖 Read `SETUP_GUIDE_WINDOWS.md` for complete documentation
- 🏗️ Read `TECHNICAL_ARCHITECTURE.md` for architecture details
- ✅ Use `DEPLOYMENT_CHECKLIST.md` for production deployment

### Extend Functionality
1. Add new operations in `ProTools.cs` (MCP Server)
2. Add corresponding handlers in `ProBridgeService.cs` (Add-In)
3. Rebuild and reinstall

### Configure for Production
1. Change build to **Release** mode
2. Update `.mcp.json` with absolute paths (for Claude Desktop)
3. Configure logging (optional)
4. Add security (ACL on Named Pipe)

---

## Useful File Locations

| What | Where |
|------|-------|
| Solution | `McpServer.sln` |
| MCP Config | `.mcp.json` |
| MCP Server Code | `McpServer/ArcGisMcpServer/` |
| Add-In Code | `AddIn/APBridgeAddIn/` |
| Built Add-In | `AddIn/APBridgeAddIn/bin/Debug/APBridgeAddIn.esriAddinX` |
| Documentation | `SETUP_GUIDE_WINDOWS.md` |

---

## Getting Help

### Self-Help
1. Check `SETUP_GUIDE_WINDOWS.md` troubleshooting section
2. Review error messages carefully
3. Verify all prerequisites are installed

### Additional Resources
- **ArcGIS Pro SDK:** https://pro.arcgis.com/en/pro-app/latest/sdk/
- **MCP Protocol:** https://modelcontextprotocol.io/
- **Repository Issues:** (Add your GitHub Issues URL)

---

## Success Checklist

After completing this guide, you should have:
- ✅ Solution builds successfully
- ✅ Add-In installed in ArcGIS Pro
- ✅ "Start server mcp" button visible
- ✅ Bridge service starts without errors
- ✅ MCP server compiles and runs
- ✅ (Optional) Commands work with Copilot

**Congratulations!** 🎉 You're ready to use MCP with ArcGIS Pro!

---

## Tips for Best Results

1. **Always start ArcGIS Pro first** before MCP clients
2. **Click "Start server mcp"** before testing commands
3. **Keep ArcGIS Pro visible** to see map operations happen
4. **Use specific layer names** in commands
5. **Test with simple commands first** (like Ping, ListLayers)

---

**Time to complete:** ~15 minutes  
**Difficulty:** Intermediate  
**Version:** 1.0  
**Last Updated:** December 18, 2025
