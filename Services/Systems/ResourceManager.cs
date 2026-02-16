using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Lumberjack;

public class ResourceManager(ContentManager content) : IDisposable
{
    private readonly Dictionary<string, object> _cache = [];
    private readonly object _sync = new();
    private bool _disposed;

    public T GetContent<T>(string path) where T : class
    {
        return Get(path, () => content.Load<T>(path));
    }

    public void Set<T>(string key, T value)
    {
        ArgumentNullException.ThrowIfNull(key);

        lock (_sync)
        {
            ThrowIfDisposed();
            _cache[key] = value!;
        }
    }

    public T Get<T>(string key, Func<T> factory) where T : class
    {
        ArgumentNullException.ThrowIfNull(key);

        lock (_sync)
        {
            ThrowIfDisposed();

            if (_cache.TryGetValue(key, out var existing))
            {
                return (T)existing;
            }
        }

        var value = factory();

        lock (_sync)
        {
            ThrowIfDisposed();

            if (_cache.TryGetValue(key, out var existing))
            {
                if (value is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                return (T)existing;
            }

            _cache[key] = value;
            return value;
        }
    }

    public T Get<T>(string key) where T : class
    {
        ArgumentNullException.ThrowIfNull(key);

        lock (_sync)
        {
            ThrowIfDisposed();

            if (_cache.TryGetValue(key, out var existing))
            {
                return (T)existing;
            }
        }

        throw new KeyNotFoundException($"Resource with key '{key}' not found.");
    }

    public void Dispose()
    {
        List<object> snapshot;

        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            snapshot = [.. _cache.Values];
            _cache.Clear();
        }

        foreach (var value in snapshot)
        {
            if (value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
