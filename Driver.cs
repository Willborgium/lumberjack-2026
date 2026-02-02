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
        var _ = new GraphicsDeviceManager(this);

        Content.RootDirectory = "Content";
        IsMouseVisible = false;

        _input = new InputService();
        _resources = new ResourceManager();
        _stateManager = new StateManager(_resources, _input);
    }

    protected override void Initialize()
    {
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };
        GraphicsDevice.PresentationParameters.BackBufferWidth = 1920;
        GraphicsDevice.PresentationParameters.BackBufferHeight = 1080;
        GraphicsDevice.PresentationParameters.IsFullScreen = true;

        _stateManager.Initialize(Content, GraphicsDevice);

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
            _resources.Dispose();
        }

        base.Dispose(disposing);
    }
}
