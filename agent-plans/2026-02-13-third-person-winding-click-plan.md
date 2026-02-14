# Third-Person, Winding, and Click Plan (2026-02-13)

## Decisions Applied

1. Third-person camera uses smoothing.
2. Click ray hit-testing starts with bounding volumes.
3. Enforce one winding convention project-wide aligned with current render/culling configuration.

## Implementation Steps

1. Preserve first-person implementation as `POVCamera` and add smoothed `ThirdPersonCamera`.
2. Make player a cube and wire player movement pipeline to target cube translation instead of moving the camera directly.
3. Reuse camera orientation as movement frame provider for third-person-relative movement.
4. Correct cube/prism index winding to match project culling convention consistently.
5. Add click system with explicit target/receiver registration:
   - `IClickTarget`
   - `IClickReceiver`
   - `ClickSelectionSystem` (updatable)
   - ray from mouse cursor
6. Demonstrate clicking by logging clicked object ids to `DebugLog`.

## Validation

- `dotnet build`
- Runtime smoke check:
  - player cube moves, third-person camera follows with smoothing
  - cube/prism render correctly (not inside out)
  - clicking renderables logs hit ids
