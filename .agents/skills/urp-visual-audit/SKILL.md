---
name: urp-visual-audit
description: Diagnose or improve URP rendering, lighting, shaders, materials, shadows, volume effects and Android visual performance in Shrink to Shoot.
---

1. Inspect URP/renderer assets, camera, lights, color space, material shaders and relevant scene context.
2. Compare concrete differences with target screenshot, avoiding generic bloom/saturation guesses.
3. Check whether unlit materials or missing shadows are the root cause.
4. Propose the smallest reversible set of changes; prioritize lit materials, lighting and shadow setup before post-processing.
5. Use Feel and UIParticle only where they belong; UI particle system does not replace world-space VFX.
6. Treat mobile SSAO/bloom as optional budgeted effects. Report GPU/Android verification separately.
7. If Editor/MCP unavailable, implement a safe opt-in Editor script or precise manual instructions.
