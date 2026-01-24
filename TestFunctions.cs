using System;
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

    public static void CreateSphere(out VertexPositionColor[] vertices, out short[] indices, int stacks = 8, int slices = 12, float radius = 1f)
    {
        var verts = new System.Collections.Generic.List<VertexPositionColor>();
        var inds = new System.Collections.Generic.List<short>();

        for (int i = 0; i <= stacks; i++)
        {
            float v = i / (float)stacks;
            float theta = v * MathF.PI;

            for (int j = 0; j <= slices; j++)
            {
                float u = j / (float)slices;
                float phi = u * MathF.PI * 2f;
                
                float x = MathF.Sin(theta) * MathF.Cos(phi);
                float y = MathF.Cos(theta);
                float z = MathF.Sin(theta) * MathF.Sin(phi);

                var pos = new Microsoft.Xna.Framework.Vector3(x, y, z) * radius;
                // color by normal
                var col = new Microsoft.Xna.Framework.Color((x + 1f) * 0.5f, (y + 1f) * 0.5f, (z + 1f) * 0.5f);
                verts.Add(new VertexPositionColor(pos, col));
            }
        }

        int ring = slices + 1;
        for (int i = 0; i < stacks; i++)
        {
            for (int j = 0; j < slices; j++)
            {
                short first = (short)(i * ring + j);
                short second = (short)((i + 1) * ring + j);

                inds.Add(first);
                inds.Add(second);
                inds.Add((short)(first + 1));

                inds.Add(second);
                inds.Add((short)(second + 1));
                inds.Add((short)(first + 1));
            }
        }

        vertices = verts.ToArray();
        indices = inds.ToArray();
    }

    public static void CreatePyramid(out VertexPositionColor[] vertices, out short[] indices, float size = 1f, float height = 1.2f)
    {
        float s = size;
        var v = new VertexPositionColor[5];
        v[0] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(-s, 0, -s), Microsoft.Xna.Framework.Color.Red);
        v[1] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(s, 0, -s), Microsoft.Xna.Framework.Color.Green);
        v[2] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(s, 0, s), Microsoft.Xna.Framework.Color.Blue);
        v[3] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(-s, 0, s), Microsoft.Xna.Framework.Color.Yellow);
        v[4] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0, height, 0), Microsoft.Xna.Framework.Color.White);

        // base two triangles + 4 side triangles
        var idx = new short[]
        {
            0,1,2, 0,2,3,
            0,1,4,
            1,2,4,
            2,3,4,
            3,0,4
        };

        vertices = v;
        indices = idx;
    }

    public static void CreateRectangularPrism(out VertexPositionColor[] vertices, out short[] indices, float width = 1.5f, float height = 0.8f, float depth = 0.6f)
    {
        float hw = width * 0.5f;
        float hh = height * 0.5f;
        float hd = depth * 0.5f;

        var v = new VertexPositionColor[8];
        v[0] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(-hw, -hh, -hd), Microsoft.Xna.Framework.Color.CornflowerBlue);
        v[1] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(hw, -hh, -hd), Microsoft.Xna.Framework.Color.CadetBlue);
        v[2] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(hw, hh, -hd), Microsoft.Xna.Framework.Color.LightGreen);
        v[3] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(-hw, hh, -hd), Microsoft.Xna.Framework.Color.Gold);
        v[4] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(-hw, -hh, hd), Microsoft.Xna.Framework.Color.OrangeRed);
        v[5] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(hw, -hh, hd), Microsoft.Xna.Framework.Color.MediumPurple);
        v[6] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(hw, hh, hd), Microsoft.Xna.Framework.Color.White);
        v[7] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(-hw, hh, hd), Microsoft.Xna.Framework.Color.PaleVioletRed);

        var idx = new short[]
        {
            0,1,2, 0,2,3,
            4,6,5, 4,7,6,
            4,3,7, 4,0,3,
            1,5,6, 1,6,2,
            3,2,6, 3,6,7,
            4,5,1, 4,1,0
        };

        vertices = v;
        indices = idx;
    }
}
