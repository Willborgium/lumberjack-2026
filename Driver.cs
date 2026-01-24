using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lumberjack;

public class Driver : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private BasicEffect _effect;
    private VertexPositionColor[] _vertices;
    private short[] _indices;
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

    private void CreateCube()
    {
        _vertices = new VertexPositionColor[8];
        float s = 1f;
        _vertices[0] = new VertexPositionColor(new Vector3(-s, -s, -s), Color.Red);
        _vertices[1] = new VertexPositionColor(new Vector3(s, -s, -s), Color.Green);
        _vertices[2] = new VertexPositionColor(new Vector3(s, s, -s), Color.Blue);
        _vertices[3] = new VertexPositionColor(new Vector3(-s, s, -s), Color.Yellow);
        _vertices[4] = new VertexPositionColor(new Vector3(-s, -s, s), Color.Cyan);
        _vertices[5] = new VertexPositionColor(new Vector3(s, -s, s), Color.Magenta);
        _vertices[6] = new VertexPositionColor(new Vector3(s, s, s), Color.White);
        _vertices[7] = new VertexPositionColor(new Vector3(-s, s, s), Color.Orange);

        _indices = new short[]
        {
            // back face
            0,1,2, 0,2,3,
            // front face
            4,6,5, 4,7,6,
            // left face
            4,3,7, 4,0,3,
            // right face
            1,5,6, 1,6,2,
            // top face
            3,2,6, 3,6,7,
            // bottom face
            4,5,1, 4,1,0
        };
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // create effect
        _effect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false
        };

        CreateCube();
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

        if (_effect != null && _vertices != null && _indices != null)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            var raster = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };
            GraphicsDevice.RasterizerState = raster;

            _effect.World = Matrix.CreateRotationY(_angle) * Matrix.CreateRotationX(_angle * 0.6f);
            _effect.View = Matrix.CreateLookAt(new Vector3(0, 0, 5f), Vector3.Zero, Vector3.Up);
            _effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleList,
                    _vertices, 0, _vertices.Length,
                    _indices, 0, _indices.Length / 3);
            }
        }

        base.Draw(gameTime);
    }
}
