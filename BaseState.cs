using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lumberjack;

public abstract class BaseState : IState
{
    protected readonly List<Renderable3DBase> Renderables = [];
    protected readonly List<IUpdatable> Updatables = [];

    protected InputService Input = null!;
    protected ResourceManager Resources = null!;
    public bool IsExitRequested => _exitRequested;

    private bool _exitRequested = false;
    private Matrix _cachedProjection;
    private int _cachedProjectionWidth = -1;
    private int _cachedProjectionHeight = -1;
    private bool _hasCachedProjection;

    protected virtual Vector3 AutoRotationDelta => new(0.01f, 0.02f, 0.03f);

    protected virtual Color ClearColor => Color.CornflowerBlue;

    private IDebugger? _debugger = null;

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
            _exitRequested = true;
            return;
        }

        UpdateDebugStats();

        foreach (var updatable in Updatables)
        {
            updatable.Update(gameTime);
        }

        // TODO: move this into the game state
        foreach (var renderable in Renderables)
        {
            if (!renderable.EnableAutoRotation) continue;
            renderable.Rotation += AutoRotationDelta;
        }
    }

    public void Render(GameTime gameTime, GraphicsDevice graphicsDevice)
    {
        ResetGraphicsDevice(graphicsDevice);

        var view = GetView(graphicsDevice);
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

    protected abstract Matrix GetView(GraphicsDevice graphicsDevice);

    protected virtual Matrix GetProjection(GraphicsDevice graphicsDevice)
    {
        var vp = graphicsDevice.Viewport;
        if (!_hasCachedProjection || vp.Width != _cachedProjectionWidth || vp.Height != _cachedProjectionHeight)
        {
            _cachedProjection = CalculateProjection(vp);
            _cachedProjectionWidth = vp.Width;
            _cachedProjectionHeight = vp.Height;
            _hasCachedProjection = true;
        }

        return _cachedProjection;
    }

    protected virtual Matrix CalculateProjection(Viewport viewport)
    {
        return Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), viewport.AspectRatio, 0.1f, 200f);
    }

    protected abstract void OnLoad(ContentManager content, GraphicsDevice graphicsDevice);

    protected void SetDebugStat(string key, string value)
    {
        _debugger?.SetStat(key, value);
    }

    private void UpdateDebugStats()
    {
        var camera = GetActiveCamera();
        if (camera != null)
        {

            SetDebugStat($"Camera Position", $"{camera.Position.X:F2}, {camera.Position.Y:F2}, {camera.Position.Z:F2}");
            SetDebugStat($"Camera Target", $"{camera.Target.X:F2}, {camera.Target.Y:F2}, {camera.Target.Z:F2}");
        }

        SetDebugStat($"Renderables Count", $"{Renderables.Count}");
    }

    protected virtual Camera? GetActiveCamera() => null;
}
