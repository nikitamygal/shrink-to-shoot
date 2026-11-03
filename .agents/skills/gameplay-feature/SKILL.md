---
name: gameplay-feature
description: Implement or refactor a Shrink to Shoot gameplay feature involving ball charging, shooting, obstacles, movement, level progress, or win/lose states.
---

1. Read AGENTS.md and docs/ai/PROJECT_CONTEXT.md.
2. Inspect existing scripts/DI registrations and identify one owner for each affected file.
3. State a small design contract with input, state transitions, configurable values, and edge cases.
4. Implement only the requested mechanism; avoid scope creep.
5. Review lifetime handling for UniTask and DOTween; no unhandled fire-and-forget in gameplay.
6. Add focused tests if practical, or exact manual Unity validation steps.
7. Report touched files and what was actually verified.
