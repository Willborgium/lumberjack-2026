using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public class StateManager(ResourceManager resources, InputService inputService)
{
    public bool IsExitRequested => _current?.IsExitRequested ?? false;

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        var spriteBatch = new SpriteBatch(graphicsDevice);
        var font = resources.GetContent<SpriteFont>(ContentPaths.DebugFont);
        _debugPanel.Initialize(graphicsDevice, spriteBatch, font);
    }

    public void SetState(IState state, ContentManager content, GraphicsDevice graphicsDevice)
    {
        _current = state;
        _current.SetDebugger(_debugPanel);
        _current.Load(content, graphicsDevice, resources, inputService);
    }

    public void Update(GameTime gameTime)
    {
        inputService.Update(gameTime);
        _current?.Update(gameTime);
        _debugPanel.Update(gameTime);
    }

    public void Render(GameTime gameTime, GraphicsDevice graphicsDevice)
    {
        _current?.Render(gameTime, graphicsDevice);

        if (_debugPanel.Visible)
        {
            _debugPanel.Draw(graphicsDevice);
        }
    }

    protected readonly DebugPanel _debugPanel = new(inputService);
    private IState? _current;
}
