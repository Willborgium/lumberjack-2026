using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Lumberjack;

public class GameState : IState
{
    private List<Renderable3DBase> _renderables = new List<Renderable3DBase>();
    private readonly List<IUpdatable> _updatables = new List<IUpdatable>();
    private bool _exitRequested = false;
    private Camera? _camera;
    private Skybox? _skybox;
    private DebugPanel? _debugPanel;
    private InputService _input = new InputService();
    private ResourceManager? _resources;

    public bool IsExitRequested => _exitRequested;

    public void Load(ContentManager content, GraphicsDevice graphicsDevice, ResourceManager resources)
    {
        _resources = resources;

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
        var floorTexture = _resources.Get<Texture2D>("grass", (c, g) => ResourceLoader.LoadTexture(c, g, "grass"));
        var floor = new Renderable3D<VertexPositionNormalTextureColor>(floorVerts, floorInds, floorTexture);

        cube.Position = new Vector3(-2.2f, 0f, 0f);
        sphere.Position = new Vector3(2.2f, 0f, 0f);
        pyramid.Position = new Vector3(0f, 1.6f, 0f);
        prism.Position = new Vector3(0f, -1.6f, 0f);
        floor.Position = new Vector3(0f, -2.2f, 0f);

        _renderables.Add(cube);
        _renderables.Add(sphere);
        _renderables.Add(pyramid);
        _renderables.Add(prism);
        _renderables.Add(floor);

        floor.CullMode = CullMode.None;
        floor.EnableAutoRotation = false;

        // configure graphics states for 3D
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };

        _camera = new Camera(new Vector3(0, 0, 6f), Vector3.Zero, _input);
        _camera.SetViewport(graphicsDevice.Viewport);
        _updatables.Add(_camera);
        var view = _camera.GetViewMatrix();
        var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), graphicsDevice.Viewport.AspectRatio, 0.1f, 200f);

        var skyTexture = _resources.Get<Texture2D>("sky", (c, g) => ResourceLoader.LoadTexture(c, g, "sky"));
        _skybox = new Skybox(graphicsDevice, skyTexture, size: 80f);

        _resources.Get<BasicEffect>("basic-effect", (c, g) =>
        {
            var fx = new BasicEffect(g)
            {
                VertexColorEnabled = true,
                LightingEnabled = true,
                SpecularPower = 16f,
                View = view,
                Projection = projection
            };
            fx.EnableDefaultLighting();
            fx.DirectionalLight0.Enabled = true;
            fx.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-0.5f, -1f, -0.3f));
            fx.DirectionalLight0.DiffuseColor = new Vector3(1f, 1f, 1f);
            fx.DirectionalLight0.SpecularColor = new Vector3(0.3f, 0.3f, 0.3f);
            fx.AmbientLightColor = new Vector3(0.18f, 0.18f, 0.18f);
            return fx;
        });

        var spriteBatch = new SpriteBatch(graphicsDevice);
        _debugPanel = new DebugPanel();
        _debugPanel.Load(content, graphicsDevice, spriteBatch, "DebugFont");
        _debugPanel.ConfigureStatProviders(() => _camera, () => _renderables.Count);
        _debugPanel.ConfigureOverlay(graphicsDevice, projection, () => _renderables);
        _updatables.Add(_debugPanel);
    }

    public void Update(GameTime gameTime)
    {
        _input.Update(gameTime);

        if (_input.IsKeyDown(Keys.Escape))
        {
            _exitRequested = true;
            return;
        }

        foreach (var updatable in _updatables)
        {
            updatable.Update(gameTime);
        }

        foreach (var r in _renderables)
        {
            if (!r.EnableAutoRotation) continue;
            r.Rotation += new Vector3(0.01f, 0.02f, 0.03f);
        }
    }

    public void Render(GameTime gameTime, GraphicsDevice graphicsDevice)
    {
        graphicsDevice.Clear(Color.CornflowerBlue);

        // ensure 3D pipeline state is restored before drawing meshes (SpriteBatch changes these)
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };
        graphicsDevice.BlendState = BlendState.Opaque;
        graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

        Matrix view = _camera != null ? _camera.GetViewMatrix() : Matrix.Identity;
        if (_resources == null) return;
        var effect = _resources.Get<BasicEffect>("basic-effect");
        effect.View = view;
        var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), graphicsDevice.Viewport.AspectRatio, 0.1f, 200f);
        effect.Projection = projection;
        _debugPanel?.UpdateOverlayProjection(projection);

        if (_skybox != null && _camera != null)
        {
            _skybox.Draw(graphicsDevice, view, effect.Projection, _camera.Position);
        }

        foreach (var r in _renderables)
        {
            r.Draw(effect, graphicsDevice);
        }

        _debugPanel?.DrawOverlay(graphicsDevice, view);

        _debugPanel?.Draw(graphicsDevice);
    }
}
