# Shrink to Shoot — Codex AI-agent setup

This folder is an **overlay** for an existing Unity project. It contains no Unity assets, scripts, or package migrations.

## Install
1. Back up or commit your current Unity project. Do not replace an existing `AGENTS.md` or `.codex` without reviewing the diff.
2. Copy the contents of this overlay into the **Unity project root** (the folder with `Assets/`, `Packages/`, `ProjectSettings/`). Keep hidden folders `.codex` and `.agents`.
3. Open that project root in VS Code with the Codex extension. Mark the project as trusted when appropriate; untrusted project-scoped `.codex/config.toml` is ignored.
4. Start a **new Codex chat/session**, so it picks up project configuration. Leave your current model selection in the extension; this overlay does not pin a model.
5. Paste the first prompt from `docs/ai/EXAMPLE_PROMPTS.md` to run a read-only audit.
6. Check subagent activity in Codex IDE, or use Codex CLI `/subagents`. The archive does not create long-running background processes.

## Files
- `AGENTS.md` — small always-on project rules.
- `.codex/config.toml` — project-scoped Codex agent concurrency (max 4).
- `.codex/agents/*.toml` — 6 named agents: explorer, gameplay, architecture, visuals, unity_editor, qa.
- `.agents/skills/*/SKILL.md` — on-demand workflows: gameplay feature, scene changes, URP audit, Android checks.
- `docs/ai/PROJECT_CONTEXT.md` — test assignment and approved decisions.
- `docs/ai/WORKFLOW.md` — how to delegate work without competing writes.
- `docs/ai/EXAMPLE_PROMPTS.md` — concrete prompts for VS Code.
- `docs/ai/UNITY_MCP.md` — optional access to the running Unity Editor.

## Important boundaries
- Agents are **Codex subagents**, not a runtime agent framework inside Unity. VContainer remains the game's application DI container.
- Unity MCP is **not preconfigured** because server installation, access and endpoint depend on your Editor. See the MCP guide.
- Neither Unity Editor nor Android builds can be verified by this overlay alone. Run tests inside your actual project.
- Nothing in this package changes your global `~/.codex/config.toml`, existing Unity scenes, packages or Unity meta GUIDs.

## Useful documentation
- https://learn.chatgpt.com/docs/agent-configuration/subagents
- https://learn.chatgpt.com/docs/agent-configuration/agents-md
- https://learn.chatgpt.com/docs/build-skills
- https://learn.chatgpt.com/docs/config-file/config-advanced
- https://unity.com/blog/unity-ai-mcp-how-to-get-started
