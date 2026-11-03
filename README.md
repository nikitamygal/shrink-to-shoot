# Shrink to Shoot

**Shrink to Shoot** is a 3D mobile arcade prototype made with **Unity 6.6** as a technical assignment for a Middle Unity Developer position.

Control a ball that uses its own mass to fire projectiles. Clear obstacles, conserve your size, and reach the exit.

## Gameplay

- **Hold** to transfer mass into a projectile: the shot grows while the player shrinks.
- **Release** to fire toward the goal. On impact, nearby obstacles become infected and explode.
- Larger shots affect a wider area; smaller shots conserve mass.
- The ball automatically bounces forward when the path is clear. The exit doors open when it gets within 5 meters.

**Win:** Reach the exit.  
**Lose:** Shrink below the critical size or run out of enough mass to clear the remaining obstacles.

The level is balanced to allow roughly **20% mass reserve** with efficient shots.

## Tech Stack

| Technology | Purpose |
| --- | --- |
| **Unity 6.6 / C#** | Game engine and gameplay |
| **VContainer** | Dependency injection |
| **UniTask** | Asynchronous gameplay flows |
| **DOTween** | Gameplay and UI animations |
| **More Mountains Feel** | Visual, audio, and camera feedback |
| **UIParticle (`com.coffee.ui-particle`)** | UI particle effects |

## Getting Started

1. Clone the repository and open it in Unity Hub.
2. Use the Unity version specified in `ProjectSettings/ProjectVersion.txt`.
3. Let Unity import dependencies, then open the gameplay scene and press **Play**.
4. To build for mobile, switch the target platform to **Android** in Build Profiles.

> Some third-party assets may require separate licenses and are not necessarily included in the repository.
