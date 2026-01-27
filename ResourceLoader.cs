using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public static class ResourceLoader
{
    // Attempts Content pipeline load first; falls back to streaming common image files
    public static Texture2D LoadTexture(ContentManager content, GraphicsDevice graphicsDevice, string assetName)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        if (graphicsDevice == null) throw new ArgumentNullException(nameof(graphicsDevice));

        try
        {
            return content.Load<Texture2D>(assetName);
        }
        catch
        {
            // try common extensions relative to Content.RootDirectory
            string[] exts = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
            foreach (var ext in exts)
            {
                var fileName = assetName.EndsWith(ext, StringComparison.OrdinalIgnoreCase) ? assetName : assetName + ext;
                var path = Path.Combine(content.RootDirectory ?? string.Empty, fileName);
                if (File.Exists(path))
                {
                    using var fs = File.OpenRead(path);
                    return Texture2D.FromStream(graphicsDevice, fs);
                }
            }

            // as last resort, try assetName as-is (maybe it already contains extension)
            var fallback = Path.Combine(content.RootDirectory ?? string.Empty, assetName);
            if (File.Exists(fallback))
            {
                using var fs = File.OpenRead(fallback);
                return Texture2D.FromStream(graphicsDevice, fs);
            }

            throw new FileNotFoundException($"Texture asset '{assetName}' not found via Content pipeline or file fallback.", assetName);
        }
    }

    public static Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int diameter, Color color)
    {
        if (graphicsDevice == null) throw new ArgumentNullException(nameof(graphicsDevice));
        if (diameter <= 1) throw new ArgumentOutOfRangeException(nameof(diameter), "Diameter must be greater than 1.");

        int radius = diameter / 2;
        var tex = new Texture2D(graphicsDevice, diameter, diameter);
        var data = new Color[diameter * diameter];
        float radiusSq = radius * radius;
        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                float dx = x - radius + 0.5f;
                float dy = y - radius + 0.5f;
                float distSq = dx * dx + dy * dy;
                data[y * diameter + x] = distSq <= radiusSq ? color : Color.Transparent;
            }
        }
        tex.SetData(data);
        return tex;
    }
}
