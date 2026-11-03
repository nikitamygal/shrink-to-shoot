# Unity Editor and MCP integration

## Why
Codex normally sees source files and can edit text, but cannot reliably assert live scene hierarchy, inspector values, package errors, screenshots, or actual Play Mode state without access to the running Unity Editor. A verified Unity MCP server can close that gap.

## Official option
Unity provides its own AI MCP bridge as part of its Unity AI tooling. Its availability, package requirements and subscription/trial restrictions may change. See: https://unity.com/blog/unity-ai-mcp-how-to-get-started

According to Unity's setup guidance (May 2026), you generally need Unity 6+, Unity AI tooling installed/configured, an eligible Unity Cloud connection and active access. In Unity Editor: Edit > Project Settings > AI > Unity MCP, check that bridge is Running, then use the integration's Configure option for a supported client. When connecting to Codex, configure a standard compatible MCP server in the Codex IDE/CLI, verifying the exact generated command or URL from your Editor installation; do NOT copy a guessed endpoint.

## Safe rollout
1. First run without MCP: Codex edits C#, AGENTS and tools only; the user handles manual Inspector changes.
2. Activate Unity MCP and confirm server discovery through Codex MCP UI or `codex mcp list`.
3. Ask Codex for read-only scene hierarchy and console checks; confirm they match the open Editor scene.
4. Enable controlled editor operations one at a time (create objects, configure prefabs, apply materials).
5. Keep version control changes inspectable. Save scene/prefabs deliberately, reviewing YAML diffs and `.meta` preservation.

## Alternatives
If the official Unity MCP isn't available, use a maintained, compatible community Unity MCP after reviewing its permissions, source, Unity/URP support and installation instructions. Do not run unknown MCP packages via `npx -y` or shell without verification. A project-local `Assets/**/Editor` utility is a transparent alternative for repetitive scene modifications.

## Safety
An instruction to an agent not to edit is not a security sandbox. Set the parent Codex permission mode carefully; subagents inherit it. Require approval for broad scene edits, package installs, builds, asset removal or publishing.
