# Render, Content, and Organization Plan (2026-02-16)

## Goals

1. Add a basic behind-camera render skip optimization.
2. Organize source files into `States`, `Services/Systems`, and `Core` folders.
3. Import all textures under `Content/textures` into MGCB using normalized lowercase/kebab-case names and `512` directory naming.
4. Update the current demo state (`DemoState`) to use newly-added textures and keep models grounded.

## Execution Steps

1. Add optional per-renderable behind-camera culling control in the render path and enable the optimization by default.
2. Move files into folder groups while preserving namespaces and behavior.
3. Normalize texture tree names on disk (`512x512` -> `512`, lowercase/kebab-case file/folder naming).
4. Append MGCB entries for all normalized textures.
5. Load new textures in `DemoState` and apply different textures across demo objects where supported; keep all objects positioned on/above the floor plane baseline.
6. Build, then update `tasks.md` and `system-overview.md` with completed work and design notes.
