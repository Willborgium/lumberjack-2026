using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack.Core;

public class Renderable3D<T> : Renderable3DBase where T : struct, IVertexType
{
    public Renderable3D(Effect effect, T[] vertices, short[] indices)
        : base(effect)
    {
        _vertices = vertices;
        _indices = indices;
    }

    public Renderable3D(Effect effect, T[] vertices, short[] indices, Texture2D texture)
        : base(effect)
    {
        _vertices = vertices;
        _indices = indices;
        _texture = texture;
    }

    public override void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
    {
        SelectTechnique();

        Effect.Parameters["World"]?.SetValue(World);
        Effect.Parameters["View"]?.SetValue(view);
        Effect.Parameters["Projection"]?.SetValue(projection);

        if (_texture != null)
        {
            Effect.Parameters["Texture"]?.SetValue(_texture);
        }

        foreach (var pass in Effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                _vertices, 0, _vertices.Length,
                _indices, 0, _indices.Length / 3
            );
        }
    }

    public override bool TryGetLocalVertices(out IReadOnlyList<Vector3> vertices)
    {
        var points = new List<Vector3>(_vertices.Length);
        foreach (var vertex in _vertices)
        {
            switch (vertex)
            {
                case VertexPositionNormalColor v:
                    points.Add(v.Position);
                    break;
                case VertexPositionNormalTextureColor v:
                    points.Add(v.Position);
                    break;
                case VertexPositionTexture v:
                    points.Add(v.Position);
                    break;
                case VertexPositionColor v:
                    points.Add(v.Position);
                    break;
                default:
                    vertices = Array.Empty<Vector3>();
                    return false;
            }
        }

        vertices = points;
        return true;
    }

    private void SelectTechnique()
    {
        if (Effect.Techniques.Count <= 1)
        {
            return;
        }

        EffectTechnique? celTextured = null;
        EffectTechnique? celColor = null;
        foreach (var technique in Effect.Techniques)
        {
            if (string.Equals(technique.Name, "CelTextured", StringComparison.Ordinal))
            {
                celTextured = technique;
            }
            else if (string.Equals(technique.Name, "CelColor", StringComparison.Ordinal))
            {
                celColor = technique;
            }
        }

        if (_texture != null && celTextured != null)
        {
            Effect.CurrentTechnique = celTextured;
            return;
        }

        if (_texture == null && celColor != null)
        {
            Effect.CurrentTechnique = celColor;
        }
    }

    private readonly T[] _vertices;
    private readonly short[] _indices;
    private readonly Texture2D? _texture;
}
