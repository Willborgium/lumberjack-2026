using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Lumberjack;

public class Driver : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private StateManager _stateManager;

    public SpriteBatch SpriteBatch => _spriteBatch ?? throw new System.InvalidOperationException("SpriteBatch is not initialized. Call LoadContent() before accessing this property.");

    public Driver()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _stateManager = new StateManager();
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

        // initialize the game state and load its content
        var gameState = new GameState();
        _stateManager.SetState(gameState, Content, GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        // allow the state to handle input/logic
        _stateManager.Update(gameTime);

        // if the state requested exit, close the game
        if (_stateManager.IsExitRequested)
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _stateManager.Render(gameTime, GraphicsDevice);

        base.Draw(gameTime);
    }
}
