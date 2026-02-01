using System;
using System.Collections.Generic;

namespace Lumberjack;

public class ResourceManager : IDisposable
{
    private readonly Dictionary<string, object> _cache = [];

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
