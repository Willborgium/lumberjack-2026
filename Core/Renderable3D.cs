using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

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

    private readonly T[] _vertices;
    private readonly short[] _indices;
    private readonly Texture2D? _texture;
}
