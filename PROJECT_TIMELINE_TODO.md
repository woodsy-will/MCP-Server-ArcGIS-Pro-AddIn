# Project Timeline TODO (Optimized)

## Purpose
Accelerate delivery of the MCP Server + ArcGIS Pro Add-In by sequencing high-impact tasks first, parallelizing workstreams, and reducing blockers identified in the current documentation and architecture notes.【F:README.md†L1-L155】【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L1-L493】

---

## Timeline Overview

### Week 0 (Immediate Setup & Risk Reduction)
**Goal:** Ensure every developer can build, run, and test without friction.
- [ ] **Validate prerequisites** on at least one reference machine (VS 2022 17.14+, .NET 8, ArcGIS Pro, SDK).【F:README.md†L15-L21】【F:ws_begins/QUICK_START.md†L7-L18】
- [ ] **Restore/build both projects** and confirm expected outputs (Add-In `.esriAddinX`, MCP server binaries).【F:ws_begins/DEPLOYMENT_CHECKLIST.md†L35-L61】
- [ ] **Smoke test Named Pipe bridge** (ArcGIS Pro button → MCP server `dotnet run`).【F:ws_begins/DEPLOYMENT_CHECKLIST.md†L86-L123】

### Week 1 (Core Functionality & Reliability)
**Goal:** Expand critical tools and harden IPC reliability.
- [ ] **Add missing GIS operations** (e.g., select by attribute, map extent, export layer) to the MCP server and Add-In handler switch.【F:README.md†L119-L155】【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L372-L443】
- [ ] **Implement retry/timeout logic** for Named Pipe client connections and long-running operations.【F:README.md†L149-L152】【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L259-L286】
- [ ] **Add basic logging** for tool calls and IPC failures in MCP server; log key bridge events in Add-In.【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L444-L468】

### Week 2 (Quality & Testing)
**Goal:** Make behavior predictable and verifiable.
- [ ] **Create integration tests** for pipe communication (test client + sample requests).【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L394-L418】
- [ ] **Define a minimal test dataset** to validate operations (layer list, feature counts, zoom).【F:ws_begins/DEPLOYMENT_CHECKLIST.md†L86-L123】
- [ ] **Document troubleshooting runbook** for common failures (pipe not found, Add-In missing).【F:ws_begins/DEPLOYMENT_CHECKLIST.md†L222-L262】

### Week 3 (Performance & Security Baseline)
**Goal:** Prepare for broader rollout with acceptable safety and responsiveness.
- [ ] **Add caching or connection reuse** to reduce repeated IPC overhead (single pipe connection, queue if needed).【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L299-L321】
- [ ] **Apply Named Pipe ACL** and validate input to reduce accidental misuse.【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L323-L370】
- [ ] **Document performance expectations** and validate timeouts for larger datasets.【F:ws_begins/DEPLOYMENT_CHECKLIST.md†L189-L208】

### Week 4 (Release Readiness)
**Goal:** Enable operational rollout and maintenance.
- [ ] **Finalize deployment checklist** and verify production steps on a clean machine.【F:ws_begins/DEPLOYMENT_CHECKLIST.md†L1-L218】
- [ ] **Lock versioning strategy** in documentation and update Add-In version fields as needed.【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L486-L503】
- [ ] **Collect user feedback** and prioritize next release features.【F:ws_begins/DEPLOYMENT_CHECKLIST.md†L274-L303】

---

## Parallel Workstreams (To Reduce Critical Path)

1. **Operations Expansion (MCP Server + Add-In)**
   - Add tool definitions + handler implementations together to avoid mismatch errors.【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L372-L443】

2. **Testing & Diagnostics**
   - Stand up integration tests while feature work continues (shared test data, automated checks).【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L394-L418】

3. **Documentation & Deployment**
   - Keep docs aligned with actual behaviors (setup, troubleshooting, deployment).【F:ws_begins/DEPLOYMENT_CHECKLIST.md†L1-L262】

---

## Milestone Acceptance Criteria
- **M1 (Week 1):** At least 2 new GIS operations added and successfully invoked via MCP tools.【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L372-L443】
- **M2 (Week 2):** Integration test script can run end-to-end with stable outputs.【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L394-L418】
- **M3 (Week 3):** Named Pipe ACL and input validation documented and implemented.【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L323-L370】
- **M4 (Week 4):** Deployment checklist verified on a clean environment and signed off.【F:ws_begins/DEPLOYMENT_CHECKLIST.md†L1-L303】

---

## Risks & Mitigations
- **Risk:** ArcGIS Pro SDK version mismatch causes build errors.
  - **Mitigation:** Pin SDK prerequisites and add a pre-flight checklist step.【F:ws_begins/DEPLOYMENT_CHECKLIST.md†L6-L33】
- **Risk:** Named Pipe connection failures during demo.
  - **Mitigation:** Add connection retry/backoff and a health check tool for liveness.【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L259-L286】【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L468-L478】
- **Risk:** Limited throughput due to sequential IPC.
  - **Mitigation:** Queue requests and consider connection pooling in the bridge client.【F:ws_begins/TECHNICAL_ARCHITECTURE.md†L299-L321】

---

## Owners & Sequencing
- **Core Integration:** Add-In + MCP server changes (Week 1).
- **Quality/Tests:** Integration tests + dataset (Week 2).
- **Ops & Security:** ACLs, logging, performance (Week 3).
- **Release:** Deployment validation + feedback loop (Week 4).

> Update this document weekly as tasks complete or new requirements emerge.
