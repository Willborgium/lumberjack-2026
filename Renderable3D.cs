using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public class Renderable3D
{
    private readonly VertexPositionColor[] _vertices;
    private readonly short[] _indices;
    private readonly BasicEffect _effect;

    public Matrix World = Matrix.Identity;

    public Renderable3D(GraphicsDevice graphicsDevice, VertexPositionColor[] vertices, short[] indices)
    {
        _vertices = vertices;
        _indices = indices;
        _effect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false
        };
    }

    public void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
    {
        _effect.World = World;
        _effect.View = view;
        _effect.Projection = projection;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                PrimitiveType.TriangleList,
                _vertices, 0, _vertices.Length,
                _indices, 0, _indices.Length / 3);
        }
    }
}
