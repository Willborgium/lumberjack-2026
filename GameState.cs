using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Lumberjack;

public class GameState : BaseState
{
    private Camera? _camera;
    private Skybox? _skybox;

    protected override void OnLoad(ContentManager content, GraphicsDevice graphicsDevice)
    {
        var cube = TestFunctions.CreateCube();
        var sphere = TestFunctions.CreateSphere(stacks: 10, slices: 14, radius: 0.9f);
        var pyramid = TestFunctions.CreatePyramid(size: 0.9f, height: 1.4f);
        var prism = TestFunctions.CreateRectangularPrism(width: 1.6f, height: 0.8f, depth: 0.6f);

        var floorTexture = Resources.Get("grass", () => ResourceLoader.LoadTexture(content, graphicsDevice, "grass"));
        var floor = TestFunctions.CreateTexturedPlane(floorTexture, 120f, 120f, 24);

        cube.Position = new Vector3(-2.2f, 0f, 0f);
        sphere.Position = new Vector3(2.2f, 0f, 0f);
        pyramid.Position = new Vector3(0f, 1.6f, 0f);
        prism.Position = new Vector3(0f, -1.6f, 0f);
        floor.Position = new Vector3(0f, -2.2f, 0f);

        Renderables.Add(cube);
        Renderables.Add(sphere);
        Renderables.Add(pyramid);
        Renderables.Add(prism);
        Renderables.Add(floor);

        floor.CullMode = CullMode.None;
        floor.EnableAutoRotation = false;

        // configure graphics states for 3D
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };

        _camera = new Camera(new Vector3(0, 0, 6f), Vector3.Zero, Input);
        _camera.SetViewport(graphicsDevice.Viewport);
        Updatables.Add(_camera);
        var projection = GetProjection(graphicsDevice);

        var skyTexture = Resources.Get("sky", () => ResourceLoader.LoadTexture(content, graphicsDevice, "sky"));
        _skybox = new Skybox(graphicsDevice, skyTexture, size: 80f);

    }

    protected override Matrix GetView(GraphicsDevice graphicsDevice) => _camera != null ? _camera.GetViewMatrix() : Matrix.Identity;

    protected override void DrawSkybox(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
    {
        if (_skybox != null && _camera != null)
        {
            _skybox.Draw(graphicsDevice, view, projection, _camera.Position);
        }
    }

    protected override Color ClearColor => Color.CornflowerBlue;

    protected override Camera? GetActiveCamera() => _camera;
}
