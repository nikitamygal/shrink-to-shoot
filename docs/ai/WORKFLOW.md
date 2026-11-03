# Multi-agent workflows

## Ownership and coordination
The main Codex session owns decisions and integration. Subagents receive bounded tasks and an explicit set of files/directories they may edit. Avoid parallel mutations of shared assets, especially `.unity`, `.prefab`, `.asset`, `ProjectSettings/*`, and VContainer installers.

### Example: implement shooting
- `explorer` (read-only): find input, player, obstacles and DI entry points.
- `architecture` (read-only): define the simplest contract/ownership boundaries and lifecycle concerns.
- `gameplay` (single writer): implement feature in agreed C# files.
- `qa` (read-only, AFTER implementation): review gameplay edge cases and run safe checks.
- `unity_editor` (single writer, AFTER code integration): wire serialised references and prefab setup if requested and Unity Editor/MCP is available.

### Example: URP polish
- `explorer` maps render assets/materials and current scene.
- `visuals` proposes changes and then owns materials/URP assets.
- `unity_editor` modifies the relevant scene and camera after the visuals pass, not concurrently on the same files.
- `qa` reviews performance tradeoffs and confirms what was actually verified on Android.

### Example: release check
- `qa` investigates checkable correctness and Android conditions.
- `explorer` provides versions/manifest and scene build-list evidence.
- Main agent consolidates, then asks before destructive changes or upload/publish.

## Task contract (give every subagent)
- Objective and explicit acceptance criteria.
- Scope: folders/files it owns vs read-only.
- Dependencies that must be inspected.
- Test/validation expected and what tools exist.
- Required output: changed paths, evidence, risks, blockers.

## Do not over-delegate
Small single-class changes stay with main Codex. Parallelize investigation and review; serialize writes to shared files. Do not use multiple Unity Editor instances on the same project directory.
