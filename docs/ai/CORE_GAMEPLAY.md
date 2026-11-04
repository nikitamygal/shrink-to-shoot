# Core gameplay — current implementation

## Input and shooting

There is one input producer, registered in GameLifetimeScope as a singleton with IInputService, IInitializable, ITickable and IDisposable. VContainer initializes it and calls Tick during Update. No player component needs to poll input, no separate permission state exists, and GameplayService has no input dependency.

The path is:

`VContainer Update → InputService.Tick → charge/release/cancel event → PlayerShootingController → PlayerMassController / Projectile`

InputService reads New Input System primary-touch press on Android and left mouse in the Editor. Focus loss, cancelled touch or device removal cancels the current hold. A cancelled hold must be released before the next press. The current focus value is read each tick so an old focus event cannot leave the service stuck. The project-wide InputSystem_Actions asset is not used by this polling path.

PlayerShootingController alone decides whether a shot is valid: active player, Gameplay state Play, valid dependencies, enough mass and no existing shot. It refunds undersized/cancelled previews and commits a valid shot on release. Its state-change and disable handlers cancel active work. Win/Lose continue to stop gameplay; raw input does not need to be switched off.

## Responsibilities

| Type | Responsibility |
|---|---|
| GameLifetimeScope | Registers services and the scene LevelPath |
| GameplayService | Start/Play/Win/Lose and delayed GameplayEnded event |
| InputService | Mouse/touch edges and cancellation, through ITickable |
| PlayerShootingController | Charge/launch coordination and one active projectile |
| PlayerMassController | Conserved sphere-volume resource and visual size |
| PlayerMovementController | Sequential DOTween hops, path clearance, stop/resume and goal arrival |
| Projectile | Arcing flight, swept first-hit detection, radial damage and cleanup |
| Obstacle | Health, immediate collider disabling, Feel destruction and cleanup |
| CameraTarget | Empty marker whose transform is the Cinemachine Follow target |

Player movement receives LevelPath through DI. Projectile receives config explicitly from the shooter. Scene and spawned-player injection use the existing SceneInjector and InjectAndInitialize helpers. PlayerSpawnPoint implements IStartable, avoiding an async-save-load race with Unity Start.

## Scene integration already applied

The obsolete player input adapter and its serialized component have been removed. Three small asset changes make the current authored loop reachable without an extra setup command:

- PlayerSphere prefab: removed only the obsolete input component.
- 01 Obstacle prefab: added a root CapsuleCollider on existing layer 6, radius .35, height 4.5, center Y=2.25. GameplayConfig already includes layer 6 in its obstacle mask. All three placed obstacles inherit that collider.
- GamePlay scene: extended only the final waypoint from local Z=-4 to Z=10.07, placing it at world `(-5.17, .746, -2.5)`. Earlier waypoints and final height are preserved. All three gameplay trees now lie before the goal.

Previously, the final waypoint was at world Z=-16.57 and trees were at Z=-10.91/-9.91/-8.66; the player could finish the short path in ~2.91 seconds and stop before reaching any tree. Those trees also had no colliders. Both saved-asset issues are corrected.

Obstacle assets with no authored destruction feedback get one child MMF_Player at runtime, using a .35-second linear shrink. Assigned Feel players are preserved. Collider disabling happens immediately; removal waits for Feel completion with cancellation and a safety timeout.

Projectile visuals may reference Sphere or its Model parent. Charging scales against the actual SphereCollider world radius and centers that sphere on the flight root, including authored child offsets. A miss terminates at destination/lifetime. Explosion radius is base radius plus charged radius times multiplier; obstacle components are deduplicated across colliders.

The camera's existing controller installs CinemachineFixedHeight once, preserving authored world Y while native Follow tracks X/Z. Uniform damping prevents jump height feeding horizontal damping through a tilted offset frame. CameraTarget remains passive. This assumes the current flat level; intentional camera noise/Feel impulses remain available after the Body stage.

## Run and verify in Unity

After scripts/assets import, restart Play mode in `Assets/_Scenes/GamePlay.unity` so the scope rebuilds its tick registration and instantiates the updated prefabs. In focused Game view, hold left mouse for about .5–1 second and release. Expect player shrink, a growing shot, launch toward the final waypoint, a tree hit/destruction and automatic hop resumption. Primary touch is the Android equivalent. Check Win occurs at the extended final waypoint, after clearing the route.

No extra setup command is required for the three integration fixes above. The existing setup/repair/validation commands remain under Tools > Shrink to Shoot > Gameplay for future edits. Scene operations support Undo; prefab/config asset changes use version control for rollback. Diagnose Shooting and Trace Shooting Input are optional Editor-only observations; they do not inject services, change gameplay state or simulate shots.

## Validation actually performed

- Full runtime and Editor C# source compiled externally against the installed Unity/package assemblies, using the project's Bee response files. Both pass. Existing obsolete scene-search warnings remain in GUIService and SceneInjector.
- 23 input transition checks pass using the production input service with simulated device/focus APIs: press/hold/release, duplicate prevention, touch/focus/device cancellation, stale focus recovery, combined sources and disposal.
- 17 checks pass against the **actual installed VContainer assembly**: one shared service instance across initializer/ticker/disposable collections; registered Tick emits press/release; container disposal cancels active input and removes subscriptions. This explicitly invokes Tick outside Unity and does not exercise Unity's native PlayerLoop.
- 41 pure sphere-volume/mass checks pass: conservation, commit/refund, critical clamp, invalid values and 30/60/120-update equivalence.
- Offline asset checks confirm unique local YAML document IDs, collider ownership/layer-mask alignment, removed adapter references, preserved waypoint references and corrected world goal position.

Unity Editor Play mode, native input delivery, screenshots and Android execution have not been run by these tools. The checks above do not establish visual/runtime behavior on device.

Reproduce from the repository root:

```sh
python3 tools/validation/compile_gameplay.py
csc -nologo -define:UNITY_EDITOR -out:/tmp/shrink-input-checks.exe Assets/_Game/Core/Scripts/Input/Services/IInputService.cs Assets/_Game/Core/Scripts/Input/Services/InputService.cs Tests/CoreGameplay/InputServiceChecks.cs
mono /tmp/shrink-input-checks.exe
csc -nologo -define:UNITY_EDITOR,REAL_VCONTAINER -main:InputRegistrationChecks -r:Library/ScriptAssemblies/VContainer.dll -r:/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.5/Facades/netstandard.dll -out:/tmp/shrink-input-registration-checks.exe Assets/_Game/Core/Scripts/Input/Services/IInputService.cs Assets/_Game/Core/Scripts/Input/Services/InputService.cs Tests/CoreGameplay/InputServiceChecks.cs Tests/CoreGameplay/InputRegistrationChecks.cs
MONO_PATH='Library/ScriptAssemblies:/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/Resources/Scripting/Managed/UnityEngine' mono /tmp/shrink-input-registration-checks.exe
csc -nologo -out:/tmp/shrink-sphere-mass-checks.exe Assets/_Game/Core/Scripts/_Specifically/Player/SphereMass.cs Tests/CoreGameplay/SphereMassChecks.cs
mono /tmp/shrink-sphere-mass-checks.exe
```

These standalone tests live outside Assets; no Test Framework package was installed. The compiler helper needs local Bee artifacts and the inspected Editor installation.

## Files affected by the input refactor

- Input service and interface; deleted obsolete input adapter script/metadata.
- GameplayService, PlayerShootingController, Obstacle, GameLifetimeScope.
- PlayerSphere/01 Obstacle prefabs and GamePlay scene (three scoped integration changes above).
- CoreGameplaySetup and ShootingDiagnostics Editor utilities.
- InputServiceChecks, InputRegistrationChecks and this document.

No package/manifest upgrades or existing GUID rewrites were performed. Target inspected: Unity6000.6.0f1, URP17.6.0, Cinemachine6.6.0, InputSystem1.20.0, VContainer1.19.0.

Assignment-wide door opening, visible path taper and a quantified 20% resource reserve remain separate authored-scene/balancing work; current defaults do not prove that budget. Android build-scene list and portrait orientation still need review before device delivery.
