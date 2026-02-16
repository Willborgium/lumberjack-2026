# To Do

- (none)

# Suggestions

1. Add debug panel visualization for active movement actions and translated velocity per target.
2. Add optional one-frame collision event callbacks to avoid repeated per-frame collision spam.
3. Add oriented box support (OBB) once rotation-aware collisions become necessary.
4. Add click-hit feedback (outline/highlight) so picks are visible without opening logs.
5. Add occlusion-aware third-person camera collision handling against environment bounds.
6. Replace position-only behind-camera culling with bounds-aware frustum culling to reduce pop-in on large meshes.
7. Raise pre-commit lint severity to include warnings after the existing analyzer backlog is addressed.
8. Add configurable cel-shading band count and rim-light parameters per material/effect instance.
9. Add optional cooldown/debounce settings for collision listeners that only need discrete enter events.

# Completed

1. input action semantics are weird. action down, action pressed, action released are strange terms. i expect just "check action" and the binding defines whether we're looking for key down, button down, etc. action checking should just be an action type and return a bool. — `3102885`
2. I don't like that camera movement is defined in the camera. I want the idea that objects that have a position are translatable, and we have an updatable that can take input and apply it to a target translatable. i also want that "input" to be an updatable that either is reading input from the input service or maybe a computer NPC preprogrammed set of inputs. So the flow is something like:
   1. input emitter: reads nothing, outputs actions
   2. action translator: reads actions, outputs translation
   3. translator: reads translation, applies to translatable
   4. Implement a little demo of this by making the camera dependent on user actions coming from the input service, but a cube that moves in a pattern based on some pre-programmed NPC action emitter.
   5. feel free to choose better updatable/component/emitter names for these things. — `3102885`
3. introduce basic collision detection system. using basic bounding shapes like box, sphere, and capsule.
   1. allow multiple ways of defining which things collide:
      1. object X can/cannot collide with object Y
      2. objects of type A can/cannot collide with objects of type B
      3. object X can/cannot collide with objects of type A — `3102885`

4. Make sure IDisposable is properly implemented across all classes that warrant it — `2d92393`
5. Implement a simple effect/render state stack to manage renderable-specific GPU state changes — `2d92393`
6. Implement basic action mapping system that translates inputs (like keyboard buttons or mouse input) to actions — `2d92393`
7. Make resource manager thread safe (cache synchronization scope) — `64e0c48`
8. Switch to a third-person style camera
   1. make player a cube
   2. preserve the current camera implementation in a POV camera class, in case we want to use it again later — `3096181`
9. review cube and prism vertices, as they appear to be rendering inside out. make sure they are defined in the correct order such that they render properly and the same way as the sphere and pyramid. — `3096181`
10. implement ray tracing so that objects can be clicked by the mouse. demonstrate this by having clicked objects log to the debug console.
11. clickability should be defined by a clickable updatable being registered with a target that can be clicked, and a receiver that does something when it is clicked. this system will look similar to the camera movement system. — `3096181`
12. Add basic rendering optimization to skip rendering objects behind the camera — `cab2c67`
13. Organize files into basic folders

- States
- Services/Systems
- Core — `cab2c67`

14. Add all of the textures in the Content/textures subdirectories to the content pipeline

- lowercase and kebab-case all of the folder and file names
- replace occurances of 512x512 with just 512 (since we assume textures are square)
- example: Content/textures/512x512/Brick/Brick_01-512x512.png should become Content/textures/512/brick/brick-01-512.png — `cab2c67`

15. Update all of the demo models to use a different texture from the new textures we've added — `cab2c67`
16. Place all of the demo models on the ground instead of floating in the sky at different positions — `cab2c67`
17. Make vs code apply formatting to a file on save — `d7a0813`
18. Introduce basic linter as pre-commit hook — `d7a0813`
19. Update namespaces to match folder structure — `ddf97b8`
20. Introduce basic cell-shading effect that can be applied to anything — `ddf97b8`
21. Allow passing in a `Renderable3D` to a collision shape and have it determine the volume for that collision shape based on the vertices — `ddf97b8`
22. Update collision system to register listeners
   1. listeners can listen to object-to-object, object-to-type, or type-to-type collisions
   2. listeners receive details about what collided — `ddf97b8`
