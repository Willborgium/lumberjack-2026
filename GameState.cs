using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Lumberjack;

public class GameState : IState
{
    private List<Renderable3DBase> _renderables = new List<Renderable3DBase>();
    private Matrix _view;
    private Matrix _projection;
    private BasicEffect? _effect;
    private GraphicsDevice? _graphicsDevice;
    private ContentManager? _content;
    private bool _exitRequested = false;
    private Renderable3DBase? _cube;
    private Renderable3DBase? _floor;
    private Camera? _camera;
    private SpriteBatch? _spriteBatch;
    private DebugPanel? _debugPanel;

    public bool IsExitRequested => _exitRequested;

    public void Load(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _content = content;
        _graphicsDevice = graphicsDevice;

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
        var floorTexture = ResourceLoader.LoadTexture(content, graphicsDevice, "grass");
        var floor = new Renderable3D<VertexPositionNormalTextureColor>(floorVerts, floorInds, floorTexture);

        cube.Position = new Vector3(-2.2f, 0f, 0f);
        sphere.Position = new Vector3(2.2f, 0f, 0f);
        pyramid.Position = new Vector3(0f, 1.6f, 0f);
        prism.Position = new Vector3(0f, -1.6f, 0f);
        floor.Position = new Vector3(0f, -2.2f, 0f);

        _cube = cube;
        _floor = floor;
        _renderables.Add(cube);
        _renderables.Add(sphere);
        _renderables.Add(pyramid);
        _renderables.Add(prism);
        _renderables.Add(floor);

        // configure graphics states for 3D
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };

        _camera = new Camera(new Vector3(0, 0, 6f), Vector3.Zero);
        _view = _camera.GetViewMatrix();
        _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), graphicsDevice.Viewport.AspectRatio, 0.1f, 100f);

        _effect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = true,
            SpecularPower = 16f,
            View = _view,
            Projection = _projection
        };
        _effect.EnableDefaultLighting();
        _effect.DirectionalLight0.Enabled = true;
        _effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-0.5f, -1f, -0.3f));
        _effect.DirectionalLight0.DiffuseColor = new Vector3(1f, 1f, 1f);
        _effect.DirectionalLight0.SpecularColor = new Vector3(0.3f, 0.3f, 0.3f);
        _effect.AmbientLightColor = new Vector3(0.18f, 0.18f, 0.18f);

        _spriteBatch = new SpriteBatch(graphicsDevice);
        _debugPanel = new DebugPanel();
        _debugPanel.Load(content, graphicsDevice, _spriteBatch, "DebugFont");
    }

    public void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            _exitRequested = true;
            return;
        }

        if (_camera != null)
        {
            _camera.Update(gameTime);
            _view = _camera.GetViewMatrix();
            if (_effect != null) _effect.View = _view;
        }

        if (_debugPanel != null)
        {
            _debugPanel.SetStat("Camera", _camera != null ? $"{_camera.Position.X:F2}, {_camera.Position.Y:F2}, {_camera.Position.Z:F2}" : "N/A");
            _debugPanel.SetStat("Renderables", _renderables.Count.ToString());
            _debugPanel.Update(gameTime);
        }

        // rotate cube if present
        if (_cube != null) _cube.Rotation += new Vector3(0.01f, 0.02f, 0.03f);

        foreach (var r in _renderables)
        {
            if (r == _floor)
            {
                continue;
            }
            r.Rotation += new Vector3(0.01f, 0.02f, 0.03f);
        }
    }

    public void Render(GameTime gameTime, GraphicsDevice graphicsDevice)
    {
        graphicsDevice.Clear(Color.CornflowerBlue);

        if (_effect == null) return;

        if (_renderables != null && _renderables.Count > 0)
        {
            foreach (var r in _renderables)
            {
                r.Draw(_effect, graphicsDevice);
            }
        }

        _debugPanel?.Draw(graphicsDevice);
    }
}
