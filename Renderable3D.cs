using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public class Renderable3D
{
    private readonly VertexPositionNormalColor[] _vertices;
    private readonly short[] _indices;
    private readonly BasicEffect _effect;

    private Vector3 _position = Vector3.Zero;
    private Vector3 _rotation = Vector3.Zero; // pitch(x), yaw(y), roll(z)
    private Vector3 _scale = new Vector3(1f, 1f, 1f);

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

    public Renderable3D(GraphicsDevice graphicsDevice, VertexPositionNormalColor[] vertices, short[] indices)
    {
        _vertices = vertices;
        _indices = indices;
        _effect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = true,
            SpecularPower = 16f
        };
    }

    private void RecalculateWorld()
    {
        var rot = Matrix.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z);
        var scl = Matrix.CreateScale(_scale);
        var trans = Matrix.CreateTranslation(_position);
        _worldCache = scl * rot * trans;
        _dirty = false;
    }

    public void SetDirectionalLight(Vector3 direction, Vector3 diffuseColor, Vector3 specularColor, Vector3 ambientColor)
    {
        _effect.LightingEnabled = true;
        _effect.EnableDefaultLighting();
        _effect.DirectionalLight0.Enabled = true;
        _effect.DirectionalLight0.Direction = Vector3.Normalize(direction);
        _effect.DirectionalLight0.DiffuseColor = diffuseColor;
        _effect.DirectionalLight0.SpecularColor = specularColor;
        _effect.AmbientLightColor = ambientColor;
    }

    public void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
    {
        _effect.World = World;
        _effect.View = view;
        _effect.Projection = projection;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalColor>(
                PrimitiveType.TriangleList,
                _vertices, 0, _vertices.Length,
                _indices, 0, _indices.Length / 3);
        }
    }
}
