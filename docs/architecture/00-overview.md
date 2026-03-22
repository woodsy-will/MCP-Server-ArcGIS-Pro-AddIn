# Architecture Overview

## System Identity

| Property | Value |
|----------|-------|
| **System Name** | MCP Server ArcGIS Pro Add-In |
| **Purpose** | Bridge AI agents to ArcGIS Pro via the Model Context Protocol |
| **Architecture Style** | Two-process IPC bridge (stdio + Named Pipes) |
| **Primary Language** | C# (.NET 8) |
| **Platform** | Windows only (ArcGIS Pro constraint) |

## Problem Statement

ArcGIS Pro is a Windows desktop GIS application with no native AI agent integration. AI agents (GitHub Copilot, Claude) communicate via the Model Context Protocol (MCP), which expects stdio-based JSON-RPC servers. ArcGIS Pro's SDK only runs in-process. This system bridges the gap: an MCP server translates agent requests into Named Pipe IPC calls to an ArcGIS Pro Add-In running inside the GIS application.

## Architecture Principles

1. **Simplicity over complexity** - Prefer robust, simple IPC over complex multi-threading (per AGENTS.md)
2. **In-process safety** - All ArcGIS SDK calls must run on `QueuedTask.Run()` for thread safety
3. **Loose coupling** - The MCP server and Add-In communicate only via a JSON-over-pipe protocol
4. **Fail-safe** - Unknown operations return explicit errors; the pipe server loop self-recovers on client disconnect

## Key Architectural Decisions

| Decision | Rationale | See ADR |
|----------|-----------|---------|
| Named Pipes for IPC | Local-only, fast, no network stack, native .NET support | [ADR-001](../adr/001-named-pipes-ipc.md) |
| Separate MCP server process | MCP requires stdio transport; ArcGIS Pro Add-Ins run in-process | [ADR-002](../adr/002-separate-mcp-process.md) |
| Connect-per-request pattern | Simplicity; avoids connection lifecycle management | [ADR-003](../adr/003-connect-per-request.md) |
| Static tool configuration | `ModelContextProtocol` NuGet uses static tool discovery | [ADR-004](../adr/004-static-tool-config.md) |
| Manual bridge start (Button) | Safer than auto-start; user controls when pipe is created | [ADR-005](../adr/005-manual-bridge-start.md) |

## Documentation Map

| Document | Purpose |
|----------|---------|
| [C4 Context](01-c4-context.md) | System context and external actors |
| [C4 Container](02-c4-container.md) | Container/process architecture |
| [C4 Component](03-c4-component.md) | Internal component structure |
| [Data Flow](04-data-flow.md) | Request lifecycle and data formats |
| [Security Architecture](05-security.md) | Threat model and security boundaries |
| [Quality Attributes](06-quality-attributes.md) | NFRs, performance, reliability |
| [Architecture Debt](07-architecture-debt.md) | Known debts and remediation roadmap |
| [ADRs](../adr/) | Architecture Decision Records |
