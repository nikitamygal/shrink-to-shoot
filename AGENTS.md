# Shrink to Shoot — Codex project instructions

## Mission
Build a small, polished Android hypercasual Unity prototype. Implement the supplied test assignment faithfully; do not invent a bigger game.
Read `docs/ai/PROJECT_CONTEXT.md` before gameplay changes and `docs/ai/WORKFLOW.md` before multi-agent work.

## Technology
- Unity 6.6 (verify exact Editor and URP package versions from `ProjectSettings/ProjectVersion.txt` and `Packages/manifest.json`).
- C#, URP, DOTween, More Mountains Feel, VContainer, UniTask, `com.coffee.ui-particle`.
- Use the versions and APIs actually installed. Do not install packages or change manifests without approval.

## Working agreements
- Inspect actual files, types, package manifests, and scene assets before assuming APIs or folder layout.
- Prefer the project's existing naming/namespace/assembly conventions. Do not create speculative abstractions or a new architecture without a concrete need.
- One owner per modified file. Parallel agents may explore/review different areas, but must not write the same files, scene, prefab, or project settings simultaneously.
- For multi-file gameplay features: ask `gameplay` for implementation, `architecture` for DI/API review, `qa` for validation; delegate only independent tasks.
- Use `visuals` for URP/material/Feel work and `unity_editor` for scene/prefab work. Editor and visuals agents must coordinate before changing the same asset.
- Never claim Unity compilation, playmode tests, screenshots, Android builds, or profiler results were verified unless tools actually produced evidence.
- Never rewrite or regenerate Unity `.meta` files or GUIDs; keep YAML scene/prefab edits minimal. Prefer Unity Editor / verified Unity MCP / an Editor script for structured asset mutations.
- Do not run Unity batchmode on the same project while its Editor instance is open; coordinate editor access.
- Avoid destructive actions, package upgrades, mass asset conversion, `git reset --hard`, force pushes, or overwriting user work without explicit approval.
- Keep runtime code independent of `UnityEditor`; editor tooling belongs under `Editor` folders or editor-only assemblies.
- Favor clear ownership, testability, cancellation/lifetime handling for UniTask, proper tween cleanup, and explicit VContainer registrations.
- For new performance-sensitive code avoid per-frame allocations, accidental LINQ in hot paths, and unnecessary scene searches.

## Done means
Explain what changed and why; list affected files; distinguish automated checks passed from manual checks needed in Unity/Android. Mention assumptions or blockers.

## Unity Editor Integration

Unity MCP is not configured.

- Do not attempt to install or configure Unity MCP.
- Do not assume direct access to the running Unity Editor.
- Prefer Unity Editor scripts for scene, prefab, material,
  and GameObject modifications.
- Place Editor scripts inside an Editor folder.
- Expose operations through MenuItem commands.
- Make Editor operations safe to run multiple times.
- Never overwrite existing scene objects without checking.
- Clearly describe any manual Unity Editor steps.
