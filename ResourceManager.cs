using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public class ResourceManager : IDisposable
{
    private readonly ContentManager _content;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Dictionary<(Type type, string key), object> _cache = new();

    public ResourceManager(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _content = content ?? throw new ArgumentNullException(nameof(content));
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
    }

    public T Get<T>(string key, Func<ContentManager, GraphicsDevice, T>? factory = null) where T : class
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        var cacheKey = (typeof(T), key);
        if (_cache.TryGetValue(cacheKey, out var existing))
        {
            return (T)existing;
        }

        T created = factory != null
            ? factory(_content, _graphicsDevice)
            : _content.Load<T>(key);

        _cache[cacheKey] = created;
        return created;
    }

    public bool TryGet<T>(string key, out T value) where T : class
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        var cacheKey = (typeof(T), key);
        if (_cache.TryGetValue(cacheKey, out var existing))
        {
            value = (T)existing;
            return true;
        }

        value = null!;
        return false;
    }

    public void Dispose()
    {
        foreach (var kvp in _cache)
        {
            if (kvp.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _cache.Clear();
    }
}
