# Example prompts for Codex IDE chat

## First prompt — project audit
Read AGENTS.md, docs/ai/PROJECT_CONTEXT.md and inspect this repository. Spawn explorer to map the current Unity scenes, code structure, packages and settings, architecture to assess DI/lifecycles, and qa to identify blockers against the gameplay test assignment. All subagents must be read-only. Collect evidence with file paths and report a prioritized implementation plan. Do not edit files yet.

## Implement charge and shot
Implement the hold-to-charge / release-to-shoot feature. Have explorer first locate the real input/player/DI code. Delegate the gameplay code to gameplay as the only writing agent. Let architecture assess registration/lifetime boundaries in read-only mode. After implementation, have qa review edge cases: critical radius, release before charging, cancellation on game over, repeated taps, and transfer of size. Do not change Unity scenes or prefabs; list exact Inspector connections I must make.

## Unity scene integration
Using unity_editor, inspect the active scene via a VERIFIED Unity MCP connection (if available), then configure the player ball, shot prefab, obstacle references and goal only as needed for the already implemented C#. Do not change render pipeline settings or unrelated assets. If Unity MCP is unavailable, create a guarded Editor utility and explain how to run it; never claim a scene has been modified without evidence.

## Make graphics match reference
Use visuals to compare the current URP environment against references, and explorer to inspect renderer assets, material shaders, lighting and scene hierarchy (read-only). Make a small, reversible plan before modifying materials or volumes. Let visuals own rendering assets and unity_editor own scene changes sequentially. Have qa identify Android performance risks. No project-wide material conversion.

## QA pass
Run a read-only QA audit against docs/ai/PROJECT_CONTEXT.md. Look specifically at charge/size math, obstacle infection and first-hit collision, safe jumping path, 5m door opening, inability-to-clear loss, and 20% starting reserve. Distinguish static review from verified Unity Play Mode and Android runs. Return actionable findings ordered by severity.
