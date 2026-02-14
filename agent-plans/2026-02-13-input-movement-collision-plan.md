# Input/Movement/Collision Plan (2026-02-13)

## User Decisions Applied

1. Action API should expose a single bool check (`IsAction`) and binding decides trigger mode.
2. Use `ITranslatable` abstraction for movement targets.
3. Collision can start with basic response via debug output/stat reporting.

## Implementation Steps

1. Refactor `InputService` action semantics:
   - add trigger-aware bindings
   - keep one action query (`IsAction(InputAction)`)
2. Introduce movement pipeline components:
   - action emitters (`InputMovementActionEmitter`, `PatternMovementActionEmitter`)
   - movement translator (`GroundMovementTranslator`)
   - target applier (`TranslationApplier`)
   - movement frame providers (`IMovementFrameProvider`)
3. Apply `ITranslatable` target abstraction:
   - `Renderable3DBase` and `Camera` implement `ITranslatable`
4. Wire demo behavior in `GameState`:
   - camera translation driven by user action emitter
   - one cube moved by pre-programmed NPC emitter pattern
5. Add collision v1:
   - shape types: sphere, box, capsule
   - filter rules: object↔object, type↔type, object↔type
   - basic response: collision stats + debug log entries
6. Validate and document:
   - run `dotnet build`
   - update `system-overview.md`
   - update `tasks.md` completion and suggestions with commit hashes
