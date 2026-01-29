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
        var (cubeVerts, cubeInds) = TestFunctions.CreateCube();
        var cube = new Renderable3D<VertexPositionNormalColor>(cubeVerts, cubeInds);

        var (sphereVerts, sphereInds) = TestFunctions.CreateSphere(stacks: 10, slices: 14, radius: 0.9f);
        var sphere = new Renderable3D<VertexPositionNormalColor>(sphereVerts, sphereInds);

        var (pyramidVerts, pyramidInds) = TestFunctions.CreatePyramid(size: 0.9f, height: 1.4f);
        var pyramid = new Renderable3D<VertexPositionNormalColor>(pyramidVerts, pyramidInds);

        var (prismVerts, prismInds) = TestFunctions.CreateRectangularPrism(width: 1.6f, height: 0.8f, depth: 0.6f);
        var prism = new Renderable3D<VertexPositionNormalColor>(prismVerts, prismInds);

        // large floor plane (textured)
        var (floorVerts, floorInds) = TestFunctions.CreateTexturedPlane(width: 120f, depth: 120f, uvScale: 24f);
        var floorTexture = Resources.Get<Texture2D>("grass", (c, g) => ResourceLoader.LoadTexture(c, g, "grass"));
        var floor = new Renderable3D<VertexPositionNormalTextureColor>(floorVerts, floorInds, floorTexture);

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

        var skyTexture = Resources.Get<Texture2D>("sky", (c, g) => ResourceLoader.LoadTexture(c, g, "sky"));
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

    protected override void ConfigureEffectDefaults(GraphicsDevice graphicsDevice)
    {
        Resources.Get<BasicEffect>("basic-effect", (c, g) =>
        {
            var fx = new BasicEffect(g)
            {
                VertexColorEnabled = true,
                LightingEnabled = true,
                SpecularPower = 16f,
                View = Matrix.Identity,
                Projection = Matrix.Identity
            };
            fx.EnableDefaultLighting();
            fx.DirectionalLight0.Enabled = true;
            fx.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-0.5f, -1f, -0.3f));
            fx.DirectionalLight0.DiffuseColor = new Vector3(1f, 1f, 1f);
            fx.DirectionalLight0.SpecularColor = new Vector3(0.3f, 0.3f, 0.3f);
            fx.AmbientLightColor = new Vector3(0.18f, 0.18f, 0.18f);
            return fx;
        });
    }

    protected override Color ClearColor => Color.CornflowerBlue;

    protected override Camera? GetActiveCamera() => _camera;
}
