# ADR-005: Manual Bridge Service Start via Button

## Status
Accepted

## Date
2025-12-18

## Context
The Add-In's `ProBridgeService` creates a Named Pipe server. We need to decide when to start it: automatically when ArcGIS Pro loads the Add-In, or manually when the user clicks a button.

## Decision
Start the bridge service manually via `Button1` in the ArcGIS Pro ribbon. The button calls `Module1.Current.StartBridgeService()`.

```csharp
// Button1.cs
protected override void OnClick()
{
    Module1.Current.StartBridgeService();
    MessageBox.Show("Bridge service started!\nNamed Pipe: ArcGisProBridgePipe");
}
```

## Consequences

### Positive
- User has explicit control over when the pipe server runs
- No background resource usage when bridge isn't needed
- Clear feedback via MessageBox on success/failure
- Simpler debugging: user knows when the service started

### Negative
- Extra manual step before using MCP tools
- Easy to forget (common support issue: "pipe not found")
- Not suitable for unattended/automated workflows

### Neutral
- `Module1.StartBridgeService()` is idempotent (checks `_service == null`)

## Alternatives Considered

1. **Auto-start on module load**: Override `Initialize()` in `Module1`, set `autoLoad="true"` in `Config.daml`. Better UX but risks starting the pipe when not needed, and errors during startup could block ArcGIS Pro loading. **Recommended as future enhancement once stability is proven.**

2. **Start on first MCP client connection attempt**: Reactive pattern. Rejected because the pipe must exist before a client can connect to it.

## Future Direction
Once the bridge is stable and tested, migrate to auto-start. This requires:
1. Moving `new ProBridgeService().Start()` into `Module1.Initialize()`
2. Setting `autoLoad="true"` in `Config.daml`
3. Adding robust error handling to prevent blocking Pro startup
