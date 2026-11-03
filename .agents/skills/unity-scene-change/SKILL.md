---
name: unity-scene-change
description: Create or edit Unity scenes, prefabs, inspector connections or Editor tools for Shrink to Shoot, using verified Unity MCP when available.
---

1. Inspect the existing scene/prefab structure through a verified Editor/MCP connection if present.
2. Write a scoped mutation plan, including scene names, prefabs and serialized fields.
3. Save a clean git state/checkpoint before risky asset edits when approved.
4. Change assets sequentially, not in parallel; prefer Editor API or official Unity tooling to raw YAML.
5. Preserve all .meta GUIDs and existing references.
6. Verify compile and inspector connections through real Editor tooling or report that manual checks are required.
7. Provide rollback instructions if relevant.
