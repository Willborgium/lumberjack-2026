using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack.Services.Systems;

public interface IDebugger
{
    void SetStat(string key, string value);
}

public class DebugPanel(InputService inputService) : IUpdatable, IDebugger, IDisposable
{
    private readonly Dictionary<string, string> _stats = [];
    private SpriteFont? _font;
    private Texture2D? _background;
    private SpriteBatch? _spriteBatch;

    public bool Visible { get; private set; } = true;
    public float WidthPercent { get; set; } = 0.35f;
    public float HeightPercent { get; set; } = 0.25f;

    public void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
    {
        _spriteBatch = spriteBatch;
        _font = font;

        _background = new Texture2D(graphicsDevice, 1, 1);
        _background.SetData([Color.White]);
    }

    public void SetStat(string key, string value)
    {
        _stats[key] = value;
    }

    public void Update(GameTime gameTime)
    {
        if (inputService.IsAction(InputAction.ToggleDebugPanel))
        {
            Visible = !Visible;
        }
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        if (_font == null || _background == null || _spriteBatch == null)
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

    public void Dispose()
    {
        _background?.Dispose();
        _background = null;
    }
}
