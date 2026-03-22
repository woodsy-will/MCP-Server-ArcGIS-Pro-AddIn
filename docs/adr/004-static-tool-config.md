# ADR-004: Static Tool Configuration via ProTools.Configure()

## Status
Accepted (with known debt - see [Architecture Debt D6](../architecture/07-architecture-debt.md))

## Date
2025-12-18

## Context
The `ModelContextProtocol` NuGet package (v0.3.0-preview.2) discovers MCP tools via `[McpServerToolType]` attributes on static classes, registered via `WithToolsFromAssembly()`. Tool methods are static. The `BridgeClient` dependency cannot be injected via constructor.

## Decision
Use a static `Configure` method called from an `IHostedService` to inject the `BridgeClient` into `ProTools`.

```csharp
[McpServerToolType]
public static class ProTools
{
    private static BridgeClient? _client;
    public static void Configure(BridgeClient client) => _client = client;

    [McpServerTool, Description("...")]
    public static async Task<string> GetActiveMapName()
    {
        var r = await _client!.OpAsync("pro.getActiveMapName");
        // ...
    }
}
```

## Consequences

### Positive
- Works with the current MCP NuGet's static tool discovery
- Simple to understand
- Only one BridgeClient instance needed (singleton)

### Negative
- Static mutable state prevents parallel test instances
- No dependency injection for tools (harder to mock)
- Tool methods use `_client!` (null-forgiving) which could NPE if called before Configure

### Neutral
- `StartupConfigurator` as `IHostedService` ensures Configure runs before any tool calls

## Alternatives Considered

1. **Wait for MCP NuGet to support instance-based tools**: May happen in future versions. Not available in v0.3.0-preview.2.

2. **Service locator pattern**: Resolve BridgeClient from `IServiceProvider` inside each tool method. Rejected because it requires passing the provider to a static class, which has the same problem.
