# Lumberjack System Overview

## Runtime Architecture

- `Driver` owns global services (`InputService`, `ResourceManager`, `StateManager`) and delegates update/render.
- `StateManager` hosts the active `IState`, updates input first, then state logic, then debug UI.
- `BaseState` provides shared world rendering flow and common service access for concrete states.

## Rendering Pipeline

- Renderables inherit from `Renderable3DBase` and implement `Draw(GraphicsDevice, view, projection)`.
- `BaseState.Render` now uses a per-renderable render-state stack (`DepthStencilState`, `RasterizerState`, `BlendState`, `SamplerState[0]`) to prevent state leakage.
- `Renderable3DBase.SetState` is used as an optional per-object state override hook; default culling is `CullCounterClockwiseFace`.
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
- Camera look/orientation is still handled by `Camera.Update`, while position movement now comes from the translation pipeline.
- Demo wiring in `GameState`:
  - player camera movement uses `InputMovementActionEmitter`
  - one cube uses `PatternMovementActionEmitter` for scripted NPC-style motion

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
