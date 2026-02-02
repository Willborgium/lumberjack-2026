using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public abstract class Renderable3DBase(Effect effect)
{
    public bool EnableAutoRotation { get; set; } = true;
    public CullMode CullMode { get; set; } = CullMode.CullClockwiseFace;
    public Effect Effect { get; set; } = effect;

    public Vector3 Position
    {
        get => _position;
        set { if (_position != value) { _position = value; _dirty = true; } }
    }

    public Vector3 Rotation
    {
        get => _rotation;
        set { if (_rotation != value) { _rotation = value; _dirty = true; } }
    }

    public Vector3 Scale
    {
        get => _scale;
        set { if (_scale != value) { _scale = value; _dirty = true; } }
    }

    public Matrix World
    {
        get { if (_dirty) RecalculateWorld(); return _worldCache; }
    }

    public virtual bool SetState(GraphicsDevice graphicsDevice) { return false; }

    public abstract void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection);

    private void RecalculateWorld()
    {
        var rot = Matrix.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z);
        var scl = Matrix.CreateScale(_scale);
        var trans = Matrix.CreateTranslation(_position);
        _worldCache = scl * rot * trans;
        _dirty = false;
    }

    private Vector3 _position = Vector3.Zero;
    private Vector3 _rotation = Vector3.Zero;
    private Vector3 _scale = Vector3.One;

    private Matrix _worldCache = Matrix.Identity;
    private bool _dirty = true;
}
