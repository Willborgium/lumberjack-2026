using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public abstract class BaseState : IState, IDisposable
{
    public bool IsExitRequested { get; private set; } = false;

    protected Color ClearColor { get; set; } = Color.CornflowerBlue;

    protected ICamera Camera { get; set; } = null!;

    public void SetDebugger(IDebugger debugger) { _debugger = debugger; }

    public void Load(ContentManager content, GraphicsDevice graphicsDevice, ResourceManager resources, InputService input)
    {
        DisposeTrackedObjects();

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
        var cameraForward = Camera.Target - Camera.Position;
        if (cameraForward.LengthSquared() > 0.000001f)
        {
            cameraForward.Normalize();
        }
        else
        {
            cameraForward = Vector3.Forward;
        }

        foreach (var renderable in Renderables)
        {
            if (renderable.EnableBehindCameraCulling)
            {
                var toRenderable = renderable.Position - Camera.Position;
                if (Vector3.Dot(cameraForward, toRenderable) <= 0f)
                {
                    continue;
                }
            }

            PushRenderState(graphicsDevice);
            try
            {
                if (!renderable.SetState(graphicsDevice))
                {
                    graphicsDevice.RasterizerState = renderable.CullMode switch
                    {
                        CullMode.CullClockwiseFace => RasterizerState.CullClockwise,
                        CullMode.CullCounterClockwiseFace => RasterizerState.CullCounterClockwise,
                        _ => RasterizerState.CullNone
                    };
                }

                renderable.Draw(graphicsDevice, view, projection);
            }
            finally
            {
                PopRenderState(graphicsDevice);
            }
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

    protected virtual bool ShouldExit() => Input.IsAction(InputAction.Exit);

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
    private readonly Stack<RenderStateSnapshot> _renderStateStack = [];

    protected readonly List<Renderable3DBase> Renderables = [];
    protected readonly List<IUpdatable> Updatables = [];

    public virtual void Dispose()
    {
        DisposeTrackedObjects();
    }

    private void DisposeTrackedObjects()
    {
        var disposed = new HashSet<object>(ReferenceEqualityComparer.Instance);

        foreach (var renderable in Renderables)
        {
            disposed.Add(renderable);
            renderable.Dispose();
        }

        foreach (var updatable in Updatables)
        {
            if (!disposed.Add(updatable))
            {
                continue;
            }

            if (updatable is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        Renderables.Clear();
        Updatables.Clear();
    }

    private void PushRenderState(GraphicsDevice graphicsDevice)
    {
        _renderStateStack.Push(new RenderStateSnapshot(
            graphicsDevice.DepthStencilState,
            graphicsDevice.RasterizerState,
            graphicsDevice.BlendState,
            graphicsDevice.SamplerStates[0]));
    }

    private void PopRenderState(GraphicsDevice graphicsDevice)
    {
        if (_renderStateStack.Count == 0)
        {
            return;
        }

        var state = _renderStateStack.Pop();
        graphicsDevice.DepthStencilState = state.DepthStencilState;
        graphicsDevice.RasterizerState = state.RasterizerState;
        graphicsDevice.BlendState = state.BlendState;
        graphicsDevice.SamplerStates[0] = state.SamplerState;
    }

    private readonly record struct RenderStateSnapshot(
        DepthStencilState DepthStencilState,
        RasterizerState RasterizerState,
        BlendState BlendState,
        SamplerState SamplerState);

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static ReferenceEqualityComparer Instance { get; } = new();

        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
