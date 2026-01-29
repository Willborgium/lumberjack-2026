using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Lumberjack;

public class Driver : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private StateManager? _stateManager;
    private ResourceManager? _resources;

    public SpriteBatch SpriteBatch => _spriteBatch ?? throw new System.InvalidOperationException("SpriteBatch is not initialized. Call LoadContent() before accessing this property.");

    public Driver()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1920,
            PreferredBackBufferHeight = 1080,
            IsFullScreen = true,
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize()
    {
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };

        _resources = new ResourceManager(Content, GraphicsDevice);
        _stateManager = new StateManager(_resources);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // initialize the game state and load its content
        var gameState = new GameState();
        _stateManager?.SetState(gameState, Content, GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        // allow the state to handle input/logic
        _stateManager?.Update(gameTime);

        // if the state requested exit, close the game
        if (_stateManager?.IsExitRequested == true)
            Exit();

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
            _resources?.Dispose();
        }

        base.Dispose(disposing);
    }
}
