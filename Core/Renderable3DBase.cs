using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public abstract class Renderable3DBase(Effect effect) : IDisposable, ITranslatable
{
    public CullMode CullMode { get; set; } = CullMode.CullCounterClockwiseFace;
    public bool EnableBehindCameraCulling { get; set; } = true;

    public Effect Effect { get; set; } = effect;
    public bool OwnsEffect { get; set; } = true;

    public Vector3 Position
    {
        get;
        set { if (field != value) { field = value; _dirty = true; } }
    }

    public Vector3 Rotation
    {
        get;
        set { if (field != value) { field = value; _dirty = true; } }
    }

    public Vector3 Scale
    {
        get;
        set { if (field != value) { field = value; _dirty = true; } }
    } = Vector3.One;

    public Matrix World => GetWorld();

    public virtual bool SetState(GraphicsDevice graphicsDevice) { return false; }

    public abstract void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection);

    public void Translate(Vector3 delta)
    {
        Position += delta;
    }

    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (OwnsEffect)
        {
            Effect.Dispose();
        }

        _disposed = true;
    }

    private Matrix GetWorld()
    {
        if (_dirty)
        {
            var rot = Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            var scl = Matrix.CreateScale(Scale);
            var trans = Matrix.CreateTranslation(Position);
            _worldCache = scl * rot * trans;
            _dirty = false;
        }

        return _worldCache;
    }

    private Matrix _worldCache = Matrix.Identity;
    private bool _dirty = true;
    private bool _disposed;
}
