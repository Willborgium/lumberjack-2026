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
    private Renderable3DBase? _texturedCube;

    public bool IsExitRequested => _exitRequested;

    public void Load(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _content = content;
        _graphicsDevice = graphicsDevice;

        var (cubeVerts, cubeInds, _) = TestFunctions.CreateTexturedCube();
        var cubeTexture = ResourceLoader.LoadTexture(content, graphicsDevice, "grass");
        var cube = new Renderable3D<VertexPositionNormalTextureColor>(cubeVerts, cubeInds, cubeTexture);

        var (sphereVerts, sphereInds) = TestFunctions.CreateSphere(stacks: 10, slices: 14, radius: 0.9f);
        var sphere = new Renderable3D<VertexPositionNormalColor>(sphereVerts, sphereInds);

        var (pyramidVerts, pyramidInds) = TestFunctions.CreatePyramid(size: 0.9f, height: 1.4f);
        var pyramid = new Renderable3D<VertexPositionNormalColor>(pyramidVerts, pyramidInds);

        var (prismVerts, prismInds) = TestFunctions.CreateRectangularPrism(width: 1.6f, height: 0.8f, depth: 0.6f);
        var prism = new Renderable3D<VertexPositionNormalColor>(prismVerts, prismInds);

        cube.Position = new Vector3(-2.2f, 0f, 0f);
        sphere.Position = new Vector3(2.2f, 0f, 0f);
        pyramid.Position = new Vector3(0f, 1.6f, 0f);
        prism.Position = new Vector3(0f, -1.6f, 0f);

        _texturedCube = cube;
        _renderables.Add(sphere);
        _renderables.Add(pyramid);
        _renderables.Add(prism);

        // configure graphics states for 3D
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };

        _view = Matrix.CreateLookAt(new Vector3(0, 0, 6f), Vector3.Zero, Vector3.Up);
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
    }

    public void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            _exitRequested = true;
            return;
        }

        // rotate textured cube if present
        if (_texturedCube != null) _texturedCube.Rotation += new Vector3(0.01f, 0.02f, 0.03f);

        foreach (var r in _renderables)
        {
            r.Rotation += new Vector3(0.01f, 0.02f, 0.03f);
        }
    }

    public void Render(GameTime gameTime, GraphicsDevice graphicsDevice)
    {
        graphicsDevice.Clear(Color.CornflowerBlue);

        if (_effect == null) return;

        // draw textured cube first (enables texture on effect)
        if (_texturedCube != null)
        {
            _texturedCube.Draw(_effect, graphicsDevice);
        }

        // disable texture for non-textured renderables
        _effect.TextureEnabled = false;

        if (_renderables != null && _renderables.Count > 0)
        {
            foreach (var r in _renderables)
            {
                r.Draw(_effect, graphicsDevice);
            }
        }
    }
}
