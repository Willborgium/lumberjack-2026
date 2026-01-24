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
    private List<Matrix> _baseWorlds = new List<Matrix>();
    private Matrix _view;
    private Matrix _projection;
    private float _angle;

    public Driver()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {

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

        // position them in the scene and record base transforms
        var cubeBase = Matrix.CreateTranslation(new Vector3(-2.2f, 0f, 0f));
        var sphereBase = Matrix.CreateTranslation(new Vector3(2.2f, 0f, 0f));
        var pyramidBase = Matrix.CreateTranslation(new Vector3(0f, 1.6f, 0f));
        var prismBase = Matrix.CreateTranslation(new Vector3(0f, -1.6f, 0f));

        cube.World = cubeBase;
        sphere.World = sphereBase;
        pyramid.World = pyramidBase;
        prism.World = prismBase;

        _renderables.Add(cube);
        _renderables.Add(sphere);
        _renderables.Add(pyramid);
        _renderables.Add(prism);

        _baseWorlds.Add(cubeBase);
        _baseWorlds.Add(sphereBase);
        _baseWorlds.Add(pyramidBase);
        _baseWorlds.Add(prismBase);

        // precompute view/projection matrices (not recalculated every frame)
        _view = Matrix.CreateLookAt(new Vector3(0, 0, 6f), Vector3.Zero, Vector3.Up);
        _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // simple rotation
        _angle += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.8f;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        if (_renderables != null && _renderables.Count > 0)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };

            var view = _view;
            var proj = _projection;

            // draw each with a small rotation
            for (int i = 0; i < _renderables.Count; i++)
            {
                var r = _renderables[i];
                var baseWorld = _baseWorlds[i];
                var rot = Matrix.CreateRotationY(_angle * (1f + i * 0.15f)) * Matrix.CreateRotationX(_angle * 0.5f);
                r.World = rot * baseWorld;
                r.Draw(GraphicsDevice, view, proj);
            }
        }

        base.Draw(gameTime);
    }
}
