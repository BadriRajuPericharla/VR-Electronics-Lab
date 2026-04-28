---
name: project-indexzero-technical
description: Technical architecture and state of the VR Electronics Lab project
type: project
originSessionId: d2fef399-f4aa-4844-a7c8-d4963c3c3ac9
---
Project: VR Electronics Lab (Project-IndexZero)
Goal: Educational tool where students learn via consequence and authentic simulation.

**Core Tech Stack:**
- Unity (Built-in RP), C#, XR Interaction Toolkit.
- Circuit Simulation: Custom Modified Nodal Analysis (MNA) solver using Gaussian elimination.
- Data: ScriptableObjects (`ComponentItemSO`) for component data.

**Key Architectural Patterns:**
- **Simulation Loop**: Two-phase loop (Stamp $\rightarrow$ UpdateState) running at ~50Hz.
- **MNA Implementation**: Components stamp into conductance matrix G and current vector I via abstract `Stamp()` methods.
- **VR Interaction**: XR grab, snap-to-breadboard, and rotation snapping.
- **Component Design**: Polymorphic `CanSnap` property; separate controller pattern (e.g., `CapacitorVisualController` for visuals vs. physics/logic scripts).
- **Authenticity**: Per-instance materials for LEDs and spatial audio sources.

**Current State:**
- MNA solver is functional.
- Capacitor support partially implemented using equivalent conductance ($G_{eq} = C/\Delta t$).
- BFS-based circuit validation in place.
