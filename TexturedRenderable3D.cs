using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public class TexturedRenderable3D
{
    private readonly VertexPositionNormalTextureColor[] _vertices;
    private readonly short[] _indices;
    private Texture2D _texture;

    private Vector3 _position = Vector3.Zero;
    private Vector3 _rotation = Vector3.Zero;
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

    public TexturedRenderable3D(VertexPositionNormalTextureColor[] vertices, short[] indices, Texture2D texture)
    {
        _vertices = vertices;
        _indices = indices;
        _texture = texture;
    }

    private void RecalculateWorld()
    {
        var rot = Matrix.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z);
        var scl = Matrix.CreateScale(_scale);
        var trans = Matrix.CreateTranslation(_position);
        _worldCache = scl * rot * trans;
        _dirty = false;
    }

    public void Draw(BasicEffect effect, GraphicsDevice graphicsDevice)
    {
        effect.World = World;
        // enable and bind texture for this draw
        effect.TextureEnabled = true;
        effect.Texture = _texture;

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTextureColor>(
                PrimitiveType.TriangleList,
                _vertices, 0, _vertices.Length,
                _indices, 0, _indices.Length / 3);
        }

        // leave effect.TextureEnabled true; caller may disable if needed
    }
}
