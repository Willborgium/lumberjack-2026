using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Lumberjack;

public class Driver : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private List<Renderable3D> _renderables = new List<Renderable3D>();
    private Matrix _view;
    private Matrix _projection;

    public Driver()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };


        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // create four renderables: cube, sphere, pyramid, rectangular prism
        var (cubeVerts, cubeInds) = TestFunctions.CreateCube();
        var cube = new Renderable3D(GraphicsDevice, cubeVerts, cubeInds);

        var (sphereVerts, sphereInds) = TestFunctions.CreateSphere(stacks: 10, slices: 14, radius: 0.9f);
        var sphere = new Renderable3D(GraphicsDevice, sphereVerts, sphereInds);

        var (pyramidVerts, pyramidInds) = TestFunctions.CreatePyramid(size: 0.9f, height: 1.4f);
        var pyramid = new Renderable3D(GraphicsDevice, pyramidVerts, pyramidInds);

        var (prismVerts, prismInds) = TestFunctions.CreateRectangularPrism(width: 1.6f, height: 0.8f, depth: 0.6f);
        var prism = new Renderable3D(GraphicsDevice, prismVerts, prismInds);

        // position them in the scene using Position property
        cube.Position = new Vector3(-2.2f, 0f, 0f);
        sphere.Position = new Vector3(2.2f, 0f, 0f);
        pyramid.Position = new Vector3(0f, 1.6f, 0f);
        prism.Position = new Vector3(0f, -1.6f, 0f);

        _renderables.Add(cube);
        _renderables.Add(sphere);
        _renderables.Add(pyramid);
        _renderables.Add(prism);

        // precompute view/projection matrices (not recalculated every frame)
        _view = Matrix.CreateLookAt(new Vector3(0, 0, 6f), Vector3.Zero, Vector3.Up);
        _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        if (_renderables != null && _renderables.Count > 0)
        {
            foreach (var r in _renderables)
            {
                r.Rotation += new Vector3(0.01f, 0.02f, 0.03f);
                r.Draw(GraphicsDevice, _view, _projection);
            }
        }

        base.Draw(gameTime);
    }
}
