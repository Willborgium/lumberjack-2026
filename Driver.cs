using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lumberjack;

public class Driver : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Renderable3D _cubeRenderable;
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

        // create a simple cube renderable from test data
        TestFunctions.CreateCube(out var verts, out var inds);
        _cubeRenderable = new Renderable3D(GraphicsDevice, verts, inds);
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

        if (_cubeRenderable != null)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };

            _cubeRenderable.World = Matrix.CreateRotationY(_angle) * Matrix.CreateRotationX(_angle * 0.6f);
            var view = Matrix.CreateLookAt(new Vector3(0, 0, 5f), Vector3.Zero, Vector3.Up);
            var proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);
            _cubeRenderable.Draw(GraphicsDevice, view, proj);
        }

        base.Draw(gameTime);
    }
}
