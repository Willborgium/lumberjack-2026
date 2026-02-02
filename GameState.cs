using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public class GameState : BaseState
{
    protected override Color ClearColor => Color.CornflowerBlue;

    protected override void OnLoad(ContentManager content, GraphicsDevice graphicsDevice)
    {
        var colorEffect = Resources.Get<Effect>("color-effect");
        var cube = TestFunctions.CreateCube(colorEffect.Clone());
        var sphere = TestFunctions.CreateSphere(colorEffect.Clone(), 10, 14, 0.9f);
        var pyramid = TestFunctions.CreatePyramid(colorEffect.Clone(), 0.9f, 1.4f);
        var prism = TestFunctions.CreateRectangularPrism(colorEffect.Clone(), 1.6f, 0.8f, 0.6f);

        var textureEffect = Resources.Get<Effect>("texture-effect");
        var floorTexture = Resources.Get("grass", () => ResourceLoader.LoadTexture(content, graphicsDevice, "grass"));
        var floor = TestFunctions.CreateTexturedPlane(textureEffect.Clone(), floorTexture, 120f, 120f, 24);

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

        _camera = new Camera(new Vector3(0, 0, 6f), Vector3.Zero, Input);
        _camera.SetViewport(graphicsDevice.Viewport);
        Updatables.Add(_camera);
        var projection = GetProjection(graphicsDevice);

        var skyTexture = Resources.Get("sky", () => ResourceLoader.LoadTexture(content, graphicsDevice, "sky"));
        var skybox = new Skybox(textureEffect.Clone(), graphicsDevice, _camera, skyTexture, 80f);
        Renderables.Add(skybox);
    }

    protected override Matrix GetView(GraphicsDevice graphicsDevice) => _camera != null ? _camera.GetViewMatrix() : Matrix.Identity;

    protected override Camera? GetActiveCamera() => _camera;

    private Camera? _camera;
}
