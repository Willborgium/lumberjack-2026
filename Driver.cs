using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public class Driver : Game
{
    private readonly InputService _input;
    private readonly ResourceManager _resources;
    private readonly StateManager _stateManager;

    public Driver()
    {
        var _ = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1920,
            PreferredBackBufferHeight = 1080,
            IsFullScreen = false,
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = false;

        _input = new InputService();
        _resources = new ResourceManager(Content);
        _stateManager = new StateManager(_resources, _input);
    }

    protected override void Initialize()
    {
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };

        _stateManager.Initialize(GraphicsDevice);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        var gameState = new GameState();
        _stateManager.SetState(gameState, Content, GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        _stateManager.Update(gameTime);

        if (_stateManager.IsExitRequested == true)
        {
            Exit();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _stateManager.Render(gameTime, GraphicsDevice);

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stateManager.Dispose();
            _resources.Dispose();
        }

        base.Dispose(disposing);
    }
}
