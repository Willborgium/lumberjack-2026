using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public class StateManager
{
    private IState? _current;
    private readonly ResourceManager _resources;

    public StateManager(ResourceManager resources)
    {
        _resources = resources;
    }

    public bool IsExitRequested => _current?.IsExitRequested ?? false;

    public void SetState(IState state, ContentManager content, GraphicsDevice graphicsDevice)
    {
        _current = state;
        _current.Load(content, graphicsDevice, _resources);
    }

    public void Update(GameTime gameTime) => _current?.Update(gameTime);

    public void Render(GameTime gameTime, GraphicsDevice graphicsDevice) => _current?.Render(gameTime, graphicsDevice);
}
