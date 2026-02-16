using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack.Services.Systems;

public class StateManager(ResourceManager resources, InputService inputService) : IDisposable
{
    public bool IsExitRequested => _current?.IsExitRequested ?? false;

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        _spriteBatch = new SpriteBatch(graphicsDevice);
        var font = resources.GetContent<SpriteFont>(ContentPaths.DebugFont);
        _debugPanel.Initialize(graphicsDevice, _spriteBatch, font);
    }

    public void SetState(IState state, ContentManager content, GraphicsDevice graphicsDevice)
    {
        if (_current is IDisposable disposable)
        {
            disposable.Dispose();
        }

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

    public void Dispose()
    {
        if (_current is IDisposable disposable)
        {
            disposable.Dispose();
            _current = null;
        }

        _debugPanel.Dispose();
        _spriteBatch?.Dispose();
        _spriteBatch = null;
    }

    protected readonly DebugPanel _debugPanel = new(inputService);
    private IState? _current;
    private SpriteBatch? _spriteBatch;
}
