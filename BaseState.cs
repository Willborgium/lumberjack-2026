using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lumberjack;

public abstract class BaseState : IState
{
    public bool IsExitRequested { get; private set; } = false;

    protected Color ClearColor { get; set; } = Color.CornflowerBlue;

    protected ICamera Camera { get; set; } = null!;

    public void SetDebugger(IDebugger debugger) { _debugger = debugger; }

    public void Load(ContentManager content, GraphicsDevice graphicsDevice, ResourceManager resources, InputService input)
    {
        Input = input;
        Resources = resources;

        Renderables.Clear();
        Updatables.Clear();

        OnLoad(content, graphicsDevice);
    }

    public void Update(GameTime gameTime)
    {
        if (ShouldExit())
        {
            IsExitRequested = true;
            return;
        }

        SetDebugStat($"Renderables Count", $"{Renderables.Count}");

        foreach (var updatable in Updatables)
        {
            updatable.Update(gameTime);
        }
    }

    public void Render(GameTime gameTime, GraphicsDevice graphicsDevice)
    {
        ResetGraphicsDevice(graphicsDevice);

        var view = Camera.GetViewMatrix();
        var projection = GetProjection(graphicsDevice);

        foreach (var renderable in Renderables)
        {
            graphicsDevice.RasterizerState = renderable.CullMode switch
            {
                CullMode.CullClockwiseFace => RasterizerState.CullClockwise,
                CullMode.CullCounterClockwiseFace => RasterizerState.CullCounterClockwise,
                _ => RasterizerState.CullNone
            };

            renderable.Draw(graphicsDevice, view, projection);
        }
    }

    private void ResetGraphicsDevice(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.Clear(ClearColor);

        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };
        graphicsDevice.BlendState = BlendState.Opaque;
        graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
    }

    protected virtual bool ShouldExit() => Input.IsKeyDown(Keys.Escape);

    protected abstract void OnLoad(ContentManager content, GraphicsDevice graphicsDevice);

    protected virtual Matrix GetProjection(GraphicsDevice graphicsDevice)
    {
        return Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), graphicsDevice.Viewport.AspectRatio, 0.1f, 200f);
    }

    protected void SetDebugStat(string key, string value)
    {
        _debugger?.SetStat(key, value);
    }

    protected InputService Input = null!;
    protected ResourceManager Resources = null!;
    private IDebugger? _debugger = null;

    protected readonly List<Renderable3DBase> Renderables = [];
    protected readonly List<IUpdatable> Updatables = [];
}
