---
name: android-check
description: Validate Android readiness, Unity build settings, package compatibility and runtime risks for the Shrink to Shoot take-home project.
---

1. Verify actual Unity version and package manifests before selecting any build command.
2. Inspect Android player settings, scenes in build, architecture, graphics API, scripting backend and permissions without rewriting them.
3. If Unity Editor is running, do not start a parallel batchmode project open.
4. Only run a build with a verified Unity Editor executable and user-provided or existing build workflow.
5. Separate compile results, Edit/Play mode test results, Editor Play Mode observations, device FPS and signed APK verification.
6. Report remaining device tests (touch, pause/resume, different aspect ratios, performance, crashes).
