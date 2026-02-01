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
    protected readonly List<Renderable3DBase> Renderables = new List<Renderable3DBase>();
    protected readonly List<IUpdatable> Updatables = new List<IUpdatable>();

    protected InputService Input = null!;
    protected ResourceManager Resources = null!;
    protected DebugPanel? DebugPanel;
    protected BasicEffect DefaultEffect => Resources.Get<BasicEffect>("default-basic-effect") ?? throw new InvalidOperationException("Default effect not found in resources.");

    private bool _exitRequested;
    private Matrix _cachedProjection;
    private int _cachedProjectionWidth = -1;
    private int _cachedProjectionHeight = -1;
    private bool _hasCachedProjection;

    public bool IsExitRequested => _exitRequested;

    public void Load(ContentManager content, GraphicsDevice graphicsDevice, ResourceManager resources, InputService input)
    {
        Input = input;
        Resources = resources;
        Renderables.Clear();
        Updatables.Clear();
        _exitRequested = false;

        OnLoad(content, graphicsDevice);

        // Ensure every state gets a debug panel
        DebugPanel ??= new DebugPanel();
        ConfigureDebugPanel(content, graphicsDevice);
        if (DebugPanel != null)
        {
            Updatables.Add(DebugPanel);
        }

        // Let derived class customize effect defaults
        ConfigureEffectDefaults(graphicsDevice);

        // Ensure overlay projection starts correct
        var projection = GetProjection(graphicsDevice);
        DebugPanel?.UpdateOverlayProjection(projection);
    }

    public void Update(GameTime gameTime)
    {
        if (ShouldExit())
        {
            _exitRequested = true;
            return;
        }

        foreach (var updatable in Updatables)
        {
            updatable.Update(gameTime);
        }

        foreach (var renderable in Renderables)
        {
            if (!renderable.EnableAutoRotation) continue;
            renderable.Rotation += AutoRotationDelta;
        }
    }

    public void Render(GameTime gameTime, GraphicsDevice graphicsDevice)
    {
        graphicsDevice.Clear(ClearColor);

        // ensure 3D pipeline state is restored before drawing meshes (SpriteBatch changes these)
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };
        graphicsDevice.BlendState = BlendState.Opaque;
        graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

        var view = GetView(graphicsDevice);
        var projection = GetProjection(graphicsDevice);

        DebugPanel?.UpdateOverlayProjection(projection);

        DrawSkybox(graphicsDevice, view, projection);

        var defaultEffect = DefaultEffect;
        foreach (var group in Renderables.GroupBy(r => r.Effect ?? defaultEffect))
        {
            var fx = group.Key;
            fx.View = view;
            fx.Projection = projection;

            foreach (var renderable in group)
            {
                renderable.Draw(fx, graphicsDevice);
            }
        }

        DebugPanel?.DrawOverlay(graphicsDevice, view);
        DebugPanel?.Draw(graphicsDevice);
    }

    protected virtual bool ShouldExit() => Input.IsKeyDown(Keys.Escape);

    protected virtual Vector3 AutoRotationDelta => new Vector3(0.01f, 0.02f, 0.03f);

    protected virtual Color ClearColor => Color.CornflowerBlue;

    protected virtual void DrawSkybox(GraphicsDevice graphicsDevice, Matrix view, Matrix projection) { }

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

    protected virtual void ConfigureEffectDefaults(GraphicsDevice graphicsDevice)
    {
        // Default effect is expected to be created by the state manager and stored in resources.
    }

    protected virtual void ConfigureDebugPanel(ContentManager content, GraphicsDevice graphicsDevice)
    {
        if (DebugPanel == null) return;

        var spriteBatch = new SpriteBatch(graphicsDevice);
        DebugPanel.Load(content, graphicsDevice, spriteBatch, "DebugFont");
        DebugPanel.ConfigureStatProviders(GetActiveCamera, () => Renderables.Count);
        DebugPanel.ConfigureOverlay(graphicsDevice, GetProjection(graphicsDevice), () => Renderables);
    }

    protected virtual Camera? GetActiveCamera() => null;
}
