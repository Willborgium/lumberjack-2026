using System;
using System.Collections.Generic;

namespace Lumberjack;

public class ResourceManager : IDisposable
{
    private readonly Dictionary<string, object?> _cache = [];

    public T? Get<T>(string key, Func<T>? factory = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_cache.TryGetValue(key, out var existing))
        {
            return (T?)existing;
        }

        var value = factory?.Invoke();

        _cache[key] = value;

        return value;
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
