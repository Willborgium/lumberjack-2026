# Tooling, Namespace, Shading, and Collision Plan (2026-02-16)

## Goals

1. Configure VS Code format-on-save.
2. Introduce a basic linter in a pre-commit hook.
3. Align namespaces with folder structure (`Core`, `Services.Systems`, `States`).
4. Add reusable cel-shading effect support for renderables.
5. Add collision-shape creation from `Renderable3D` vertex data.
6. Add collision listener registrations for object↔object, object↔type, type↔type with collision details.

## Execution Order

1. Add editor/lint workflow files (`.vscode/settings.json`, `.editorconfig`, `.githooks/pre-commit`).
2. Refactor namespaces to folder-based namespaces and add global usings.
3. Add cel-shading effect (`Content/Effects/CelShadingEffect.fx`) + MGCB registration + renderable technique selection.
4. Add mesh-derived bounds helpers in `Renderable3D`/`Renderable3DBase` and shape factory methods in collision shapes.
5. Add collision listener APIs + payloads and wire notifications in `CollisionSystem`.
6. Validate with `dotnet build` and update `system-overview.md` + `tasks.md` with completed items and commit hashes.
