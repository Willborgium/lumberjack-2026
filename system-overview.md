# Lumberjack System Overview

## Runtime Architecture

- `Driver` owns global services (`InputService`, `ResourceManager`, `StateManager`) and delegates update/render.
- `StateManager` hosts the active `IState`, updates input first, then state logic, then debug UI.
- `BaseState` provides shared world rendering flow and common service access for concrete states.
- Current runtime uses `DemoState` as the active gameplay/demo state.
- Source is organized into folder groups:
  - `States` for state contracts/implementations
  - `Services/Systems` for runtime service and simulation systems
  - `Core` for shared engine primitives, geometry, and abstractions

## Rendering Pipeline

- Renderables inherit from `Renderable3DBase` and implement `Draw(GraphicsDevice, view, projection)`.
- `BaseState.Render` now uses a per-renderable render-state stack (`DepthStencilState`, `RasterizerState`, `BlendState`, `SamplerState[0]`) to prevent state leakage.
- `Renderable3DBase.SetState` is used as an optional per-object state override hook; default culling is `CullCounterClockwiseFace`.
- `BaseState.Render` includes a basic behind-camera skip (`EnableBehindCameraCulling`) that omits draw calls for objects whose positions are behind the camera forward vector.
- Effects are parameter-driven (`World`, `View`, `Projection`, optional `Texture`) and are generally cloned per renderable.

## Resource and Ownership Model

- `ResourceManager` caches content assets by string key and now synchronizes cache access with a private lock.
- Thread-safety scope is cache synchronization only; GPU/content-loading thread affinity rules still apply.
- `Renderable3DBase` implements `IDisposable` and disposes owned effects by default (`OwnsEffect = true`).
- `BaseState` disposes tracked `Renderables` and disposable `Updatables` on reload/disposal.
- `StateManager` disposes replaced states, `DebugPanel` resources, and owned `SpriteBatch`.

## Input Model

- `InputService` exposes both raw key APIs and a single action query (`IsAction`).
- Action bindings carry trigger semantics (`Down`, `Pressed`, `Released`) so call sites stay simple.
- Default action bindings include `Exit`, `ToggleDebugPanel`, movement (`WASD`), and `Run` (`Shift`).

## Movement Pipeline

- Translation targets use `ITranslatable` (`Position` + `Translate(delta)`).
- Movement is decomposed into composable pieces:
  - `IMovementActionEmitter` (action source)
  - `GroundMovementTranslator` (actions -> translation vector)
  - `TranslationApplier` (translation -> translatable target)
- Camera look/orientation is handled by the active camera implementation update (`POVCamera` or `ThirdPersonCamera`), while target translation comes from the movement pipeline.
- Camera implementations now split into:
  - `POVCamera` (preserved first-person implementation)
  - `ThirdPersonCamera` (active gameplay camera with smoothing)
- Demo wiring in `DemoState`:
  - player cube movement uses `InputMovementActionEmitter`
  - NPC prism movement uses `PatternMovementActionEmitter` for scripted motion
  - `ThirdPersonCamera` follows the player cube and provides movement frame vectors

## Content Pipeline and Textures

- `Content/textures` now includes normalized naming:
  - lowercase directories/files
  - kebab-case naming
  - `512x512` directory/file segments normalized to `512`
- All textures under `Content/textures/512` are registered in `Content.mgcb`.
- `DemoState` now uses multiple imported texture assets for different demo models (cube/sphere/pyramid/prism/floor) to exercise texture variety.

## Click Selection System

- Clickability uses a registered target/receiver model:
  - `IClickTarget` performs hit tests
  - `IClickReceiver` handles click responses
  - `ClickSelectionSystem` is an updatable that casts a ray on click actions
- Input uses `InputAction.PrimaryClick` bound to mouse left button with `Pressed` trigger.
- v1 hit testing uses bounding spheres (`SphereClickTarget`) and logs selected object ids to `DebugLog`.

## Collision System (v1)

- `CollisionSystem` supports primitive shapes:
  - sphere (`SphereCollisionShape`)
  - axis-aligned box (`BoxCollisionShape`)
  - capsule (`CapsuleCollisionShape`)
- Collision filtering supports:
  - object↔object rules
  - type↔type rules
  - object↔type rules
- Basic response currently reports collision count/last pair to debug stats and logs contacts to `DebugLog`.

## Debugging

- `DebugPanel` draws runtime stats and recent `DebugLog` lines.
- Visibility toggle is action-mapped (`ToggleDebugPanel`).

## Current Constraints / Notes

- Render state stack currently snapshots slot `SamplerStates[0]`; expand if multi-texturing is introduced.
- Resource cache is synchronized, but this does not make all asset creation safe on background threads.
- Collision shapes are axis-aligned/no-rotation in v1; oriented/mesh collisions are out of scope for now.
- Winding convention is now enforced consistently for generated cube/prism geometry to match project culling behavior.
