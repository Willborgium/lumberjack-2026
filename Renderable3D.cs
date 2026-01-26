using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public abstract class Renderable3DBase
{
    private Vector3 _position = Vector3.Zero;
    private Vector3 _rotation = Vector3.Zero; // pitch(x), yaw(y), roll(z)
    private Vector3 _scale = Vector3.One;

    private Matrix _worldCache = Matrix.Identity;
    private bool _dirty = true;

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

    private void RecalculateWorld()
    {
        var rot = Matrix.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z);
        var scl = Matrix.CreateScale(_scale);
        var trans = Matrix.CreateTranslation(_position);
        _worldCache = scl * rot * trans;
        _dirty = false;
    }

    public abstract void Draw(BasicEffect effect, GraphicsDevice graphicsDevice);
}

public class Renderable3D<T> : Renderable3DBase where T : struct, IVertexType
{
    private readonly T[] _vertices;
    private readonly short[] _indices;
    private readonly Texture2D? _texture;

    public Renderable3D(T[] vertices, short[] indices)
    {
        _vertices = vertices;
        _indices = indices;
    }

    public Renderable3D(T[] vertices, short[] indices, Texture2D texture)
    {
        _vertices = vertices;
        _indices = indices;
        _texture = texture;
    }

    public override void Draw(BasicEffect effect, GraphicsDevice graphicsDevice)
    {
        effect.World = World;

        if (_texture != null)
        {
            effect.TextureEnabled = true;
            effect.Texture = _texture;
        }
        else
        {
            effect.TextureEnabled = false;
        }

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives<T>(
                PrimitiveType.TriangleList,
                _vertices, 0, _vertices.Length,
                _indices, 0, _indices.Length / 3);
        }
    }
}
