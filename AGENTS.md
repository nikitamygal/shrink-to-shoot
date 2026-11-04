# Shrink to Shoot — Codex instructions

## Project
- Unity 6.6, URP, Android. Hypercasual prototype: a sphere spends its size to charge shots, clears obstacles and jumps toward the goal.
- Existing stack: C#, VContainer, UniTask, DOTween, More Mountains Feel, Cinemachine, New Input System, UIParticle (`com.coffee.ui-particle`). Use only packages actually installed.

## Code and architecture
- Before editing, inspect the **relevant existing** code and imitate its style, namespaces, layout and APIs. Do not impose a new architecture.
- Match established C# conventions: `PascalCase` types/methods/properties, `_camelCase` private fields, explicit `[SerializeField]` for Inspector fields, and existing brace/spacing style.
- Keep changes focused. Extend existing components/services instead of creating duplicate managers, wrappers or speculative abstractions.
- Use VContainer for scene-level/service dependencies. A reusable player prefab must not serialize references to scene-only objects such as `LevelPath`; resolve them from the level scope/composition root.
- Prefab-local component and asset references may use `[SerializeField]` and/or `GetComponent` as appropriate. `FindAnyObjectByType`, `GameObject.Find` and similar scene searches are not substitutes for proper DI.
- Keep input devices in the input layer, mass logic separate from movement/shooting, and gameplay values in the existing `GameplayConfig`.

## Unity scenes, prefabs and assets
- Inspect **and directly edit** `.unity`, `.prefab`, `.asset`, materials and their serialized references when the available tools permit it. Do not require Unity MCP.
- Wire required Inspector references and integrate changes into the actual scene/prefabs; code-only changes are not a completed gameplay feature.
- Preserve Unity GUIDs, `.meta` files, prefab connections and overrides. Avoid unrelated changes.
- Do not generate an Editor utility for routine asset edits. Use an Editor script only if direct editing is technically impractical or materially less safe; explain why.
- Do not claim to have inspected a running Unity Editor unless you actually accessed it. If integration cannot be verified, identify the exact remaining checks.

## Execution
- Work as one agent. For clear tasks, implement directly; inspect only relevant files, avoid broad audits or lengthy plans unless requested.
- Respect the current scope; do not refactor unrelated code, upgrade packages or change project settings unnecessarily.
- Check compilation/errors with available tools; never claim Play Mode, Android builds, or visual validation without evidence.
- Finish with a concise summary of modified files, what was verified, and any unavoidable manual steps.
