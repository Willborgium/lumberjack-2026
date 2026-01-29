using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lumberjack;

public class DebugPanel : IUpdatable
{
    private readonly Dictionary<string, string> _stats = new Dictionary<string, string>();
    private Func<Camera?>? _cameraProvider;
    private Func<int>? _renderableCountProvider;
    private Func<IEnumerable<Renderable3DBase>>? _renderablesProvider;
    private BasicEffect? _overlayEffect;
    private Texture2D? _vertexCircle;
    private bool _drawWireOverlay = false;
    private SpriteFont? _font;
    private Texture2D? _background;
    private SpriteBatch? _spriteBatch;
    private KeyboardState _prevKeyboard;

    public bool Visible { get; private set; } = true;
    public float WidthPercent { get; set; } = 0.35f;
    public float HeightPercent { get; set; } = 0.25f;

    public void Load(ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, string fontAssetName)
    {
        _spriteBatch = spriteBatch;
        _font = content.Load<SpriteFont>(fontAssetName);

        _background = new Texture2D(graphicsDevice, 1, 1);
        _background.SetData(new[] { Color.White });
    }

    public void SetStat(string key, string value)
    {
        _stats[key] = value;
    }

    public void ConfigureStatProviders(Func<Camera?> cameraProvider, Func<int> renderableCountProvider)
    {
        _cameraProvider = cameraProvider;
        _renderableCountProvider = renderableCountProvider;
    }

    public void ConfigureOverlay(GraphicsDevice graphicsDevice, Matrix projection, Func<IEnumerable<Renderable3DBase>> renderablesProvider)
    {
        _overlayEffect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
            TextureEnabled = false,
            Projection = projection,
            View = Matrix.Identity,
            World = Matrix.Identity
        };

        _vertexCircle = ResourceLoader.CreateCircleTexture(graphicsDevice, 12, Color.White);
        _renderablesProvider = renderablesProvider;
    }

    public void UpdateOverlayProjection(Matrix projection)
    {
        if (_overlayEffect != null)
        {
            _overlayEffect.Projection = projection;
        }
    }

    public void Update(GameTime gameTime)
    {
        if (_cameraProvider != null)
        {
            var camera = _cameraProvider();
            if (camera != null)
            {
                _stats["Camera"] = $"{camera.Position.X:F2}, {camera.Position.Y:F2}, {camera.Position.Z:F2}";
            }
            else
            {
                _stats["Camera"] = "N/A";
            }
        }

        if (_renderableCountProvider != null)
        {
            _stats["Renderables"] = _renderableCountProvider().ToString();
        }

        var kb = Keyboard.GetState();
        if (kb.IsKeyDown(Keys.OemTilde) && !_prevKeyboard.IsKeyDown(Keys.OemTilde))
        {
            Visible = !Visible;
        }

        if (kb.IsKeyDown(Keys.F3) && !_prevKeyboard.IsKeyDown(Keys.F3))
        {
            _drawWireOverlay = !_drawWireOverlay;
        }

        _prevKeyboard = kb;
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        if (!Visible || _font == null || _background == null || _spriteBatch == null)
        {
            return;
        }

        var viewport = graphicsDevice.Viewport;
        int width = (int)(viewport.Width * WidthPercent);
        int height = (int)(viewport.Height * HeightPercent);
        var panelRect = new Rectangle(10, 10, width, height);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
        _spriteBatch.Draw(_background, panelRect, Color.Black * 0.6f);

        Vector2 cursor = new Vector2(20, 16);
        _spriteBatch.DrawString(_font, "Debug", cursor, Color.White);
        cursor.Y += _font.LineSpacing + 4;

        foreach (var kvp in _stats)
        {
            _spriteBatch.DrawString(_font, $"{kvp.Key}: {kvp.Value}", cursor, Color.White);
            cursor.Y += _font.LineSpacing;
        }

        if (DebugLog.Lines.Count > 0)
        {
            cursor.Y += 4;
            _spriteBatch.DrawString(_font, "Logs:", cursor, Color.White);
            cursor.Y += _font.LineSpacing;

            foreach (var line in DebugLog.Lines)
            {
                _spriteBatch.DrawString(_font, line, cursor, Color.LightGray);
                cursor.Y += _font.LineSpacing;
            }
        }

        _spriteBatch.End();
    }

    public void DrawOverlay(GraphicsDevice graphicsDevice, Matrix view)
    {
        if (!_drawWireOverlay || _overlayEffect == null || _vertexCircle == null || _spriteBatch == null || _renderablesProvider == null)
        {
            return;
        }

        var renderables = _renderablesProvider();

        _overlayEffect.View = view;

        graphicsDevice.BlendState = BlendState.Opaque;
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;

        foreach (var r in renderables)
        {
            var positions = r.GetPositions();
            var edges = r.GetEdges();
            if (positions.Count == 0 || edges.Count == 0) continue;

            _overlayEffect.World = r.World;

            var lineVerts = new VertexPositionColor[edges.Count * 2];
            int idx = 0;
            foreach (var (a, b) in edges)
            {
                lineVerts[idx++] = new VertexPositionColor(positions[a], Color.Black);
                lineVerts[idx++] = new VertexPositionColor(positions[b], Color.Black);
            }

            foreach (var pass in _overlayEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, lineVerts, 0, edges.Count);
            }

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            foreach (var pos in positions)
            {
                var worldPos = Vector3.Transform(pos, r.World);
                var projected = graphicsDevice.Viewport.Project(worldPos, _overlayEffect.Projection, _overlayEffect.View, Matrix.Identity);
                if (projected.Z < 0 || projected.Z > 1) continue;
                _spriteBatch.Draw(_vertexCircle, new Vector2(projected.X - _vertexCircle.Width * 0.5f, projected.Y - _vertexCircle.Height * 0.5f), Color.White);
            }
            _spriteBatch.End();
        }
    }
}
