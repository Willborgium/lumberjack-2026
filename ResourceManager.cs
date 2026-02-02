using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Lumberjack;

public class ResourceManager(ContentManager content) : IDisposable
{
    private readonly Dictionary<string, object> _cache = [];

    public T GetContent<T>(string path) where T : class
    {
        return Get(path, () => content.Load<T>(path));
    }

    public void Set<T>(string key, T value)
    {
        ArgumentNullException.ThrowIfNull(key);
        _cache[key] = value!;
    }

    public T Get<T>(string key, Func<T> factory) where T : class
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_cache.TryGetValue(key, out var existing))
        {
            return (T)existing;
        }

        var value = factory();

        _cache[key] = value;

        return value;
    }
    public T Get<T>(string key) where T : class
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_cache.TryGetValue(key, out var existing))
        {
            return (T)existing;
        }

        throw new KeyNotFoundException($"Resource with key '{key}' not found.");
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
