# Project context — Shrink to Shoot

## Product
A one-level, mobile 3D hypercasual prototype for a Middle Unity Developer test assignment. Android is the required target. Portrait, three-quarter/top-down composition inspired by the supplied image: player ball lower left, destination/door upper right, obstacles along a visible path.

## Required game flow (authoritative requirements from the test assignment)
1. The player is a spherical ball. Holding a tap separates/charges a shot ball while the player ball shrinks. Longer hold transfers more size to the shot.
2. On release the shot travels **towards the target**, not toward a freely tapped point unless later explicitly required.
3. The projectile impacts the first obstacle in its route, infecting obstacles within a radius that scales with shot size; affected obstacles explode. Closer obstacles should be easier to affect. Whether infection recursively chains is a DESIGN DECISION, not a stated requirement; implement a bounded radial effect first unless otherwise agreed.
4. After obstacles clear the main ball automatically hops forward along the cleared central path.
5. Its final size is smaller but it should fit comfortably through the remaining clear passage. The path visibly narrows together with ball size; the exact visual mechanism needs explicit design choice.
6. Doors open when the player ball is within 5 world meters.
7. Lose if holding transfers so much mass that the ball reaches a configurable critical minimum. Lose if there is insufficient shot resource to clear the remaining path.
8. Starting ball size must allow the intended solution with 20% surplus. The exact definition of '20%' (scale/radius/volume/resource budget) should be made explicit in balancing docs, not assumed from the prompt.

## Existing decisions
- Chosen name: Shrink to Shoot.
- Target Unity Editor: Unity 6.6. Verify version file before using APIs.
- Packages: DOTween, More Mountains Feel, VContainer, UniTask, com.coffee.ui-particle. Verify actual installed package IDs/API signatures before use.
- Stylized low-poly/toon forest. Prior asset environment was manually converted to URP. Never globally reconvert the asset library without inspection.

## Scope guardrails
Do not add shop, meta-progression, ads, new levels, or free player steering without request. Aim for one delightful and reliable vertical slice. Prioritize core loop correctness and visual clarity.

## Open choices to resolve in code/design with recorded rationale
- Fixed forward projectile trajectory vs target-anchor direction (both are toward goal; no free-aim by default).
- Infection immediate in radius vs delayed spread; avoid unbounded chain reactions.
- Resource conservation rule (visual radius vs sphere volume).
- Dynamic path geometry width vs progression-based taper.
- What constitutes a guaranteed winnable solution and a 20% budget reserve.
