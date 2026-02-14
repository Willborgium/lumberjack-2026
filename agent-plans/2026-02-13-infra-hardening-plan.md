# Infrastructure Hardening Plan (2026-02-13)

## Scope

Implement early-stage engine hardening tasks with minimal disruption:

1. IDisposable coverage where ownership is clear.
2. Render/effect state stack hygiene per renderable (not gameplay state stack).
3. Action mapping layer for keyboard-driven actions.
4. ResourceManager cache synchronization (thread-safety for cache operations only).

## Execution Order

1. **Render state hygiene + disposal foundation**
   - Add per-renderable graphics state push/pop in `BaseState.Render`.
   - Ensure `Renderable3DBase.SetState` is actually used.
   - Add dispose ownership for renderables/effects and state-managed entities.
2. **Action mapping**
   - Add action enum + default key bindings in `InputService`.
   - Migrate camera movement/run, debug toggle, and escape handling to action queries.
3. **ResourceManager thread-safety**
   - Synchronize dictionary access and add disposed guards.
   - Preserve current API behavior with minimal signature changes.
4. **Documentation updates**
   - Update `system-overview.md` with new design decisions.
   - Update `tasks.md` as tasks are completed with commit hashes.

## Validation

- Build with `dotnet build` after each implementation slice.
- Smoke check: skybox and opaque meshes render together; no render state leaks between renderables.
- Smoke check: action mappings still drive camera/debug/exit behavior.
- Smoke check: resource cache methods remain functionally identical under single-threaded usage.
