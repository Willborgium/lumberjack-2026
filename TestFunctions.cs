using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public static class TestFunctions
{
    public static void CreateCube(out VertexPositionColor[] vertices, out short[] indices)
    {
        vertices = new VertexPositionColor[8];
        float s = 1f;
        vertices[0] = new VertexPositionColor(new Vector3(-s, -s, -s), Color.Red);
        vertices[1] = new VertexPositionColor(new Vector3(s, -s, -s), Color.Green);
        vertices[2] = new VertexPositionColor(new Vector3(s, s, -s), Color.Blue);
        vertices[3] = new VertexPositionColor(new Vector3(-s, s, -s), Color.Yellow);
        vertices[4] = new VertexPositionColor(new Vector3(-s, -s, s), Color.Cyan);
        vertices[5] = new VertexPositionColor(new Vector3(s, -s, s), Color.Magenta);
        vertices[6] = new VertexPositionColor(new Vector3(s, s, s), Color.White);
        vertices[7] = new VertexPositionColor(new Vector3(-s, s, s), Color.Orange);

        indices = new short[]
        {
            0,1,2, 0,2,3,
            4,6,5, 4,7,6,
            4,3,7, 4,0,3,
            1,5,6, 1,6,2,
            3,2,6, 3,6,7,
            4,5,1, 4,1,0
        };
    }
}
