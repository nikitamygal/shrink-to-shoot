# Shrink to Shoot

**Shrink to Shoot** is a mobile 3D arcade prototype developed in **Unity 6.6** as a technical assignment for a Middle Unity Developer position.

The player controls an orange ball that spends its own mass to create projectiles. Charge a shot, clear obstacles from the path, and reach the goal before the ball becomes too small.

## Gameplay

1. **Hold** the screen to transfer mass from the player ball into a projectile. The longer you hold, the larger the projectile becomes and the smaller the player gets.
2. **Release** to fire toward the goal. The projectile hits the first obstacle in its path.
3. **Clear obstacles** using an infection / area-of-effect reaction. A larger projectile affects a wider radius; closely packed obstacles are easier to clear.
4. **Advance automatically** as the route opens. The player ball bounces along the path toward the finish.
5. **Reach the exit** without consuming all your mass. The finish doors open when the player approaches within 5 meters.

### Win and lose conditions

- **Win:** Reach the finish through the cleared path.
- **Lose:** The ball reaches its critical minimum size while charging a shot, or the remaining mass is insufficient to clear the path.

The level is intended to be solvable with approximately **20% mass reserve** when shots are used efficiently.

## Tech stack

| Technology | Purpose |
| --- | --- |
| **Unity 6.6** | Game engine and Android build |
| **C#** | Gameplay and application logic |
| **VContainer** | Dependency injection and system composition |
| **UniTask** | Async gameplay flows and cancellation |
| **DOTween** | Lightweight gameplay and UI animations |
| **More Mountains Feel** | Gameplay feedback and effects |
| **UIParticle (`com.coffee.ui-particle`)** | Particle effects integrated with the UI |

## Development goals

- Deliver a complete, readable one-level gameplay experience.
- Keep input, shooting, obstacle interactions, player movement, and game state logic modular.
- Make mass consumption and projectile impact clear through animation, VFX, and audio feedback.
- Balance obstacle layout and projectile power around meaningful shot-size decisions.
- Produce an Android build and a short gameplay video.

## Controls

**Touch and hold** to charge a projectile. **Release** to shoot.

## Running the project

1. Clone the repository.
2. Open the project in **Unity Hub** using the compatible **Unity 6.6** editor version recorded in `ProjectSettings/ProjectVersion.txt`.
3. Allow Unity to import and resolve project dependencies.
4. Ensure any separately licensed assets or packages required by the project are available locally.
5. Open the gameplay scene and press **Play**, or switch to **Android** in Build Profiles to create a mobile build.

> **Note:** Some third-party assets, such as More Mountains Feel, may be subject to separate licenses and may not be redistributed in this public repository.

## Project status

🚧 **In development** — gameplay prototype / technical assignment.

## Deliverables

- Android build
- Gameplay video (YouTube)
- Unity project source code

Links will be added when the deliverables are ready.
