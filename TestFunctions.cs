using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public static class TestFunctions
{
    public static (VertexPositionColor[] vertices, short[] indices) CreateCube()
    {
        var vertices = new VertexPositionColor[8];
        float s = 1f;
        vertices[0] = new VertexPositionColor(new Vector3(-s, -s, -s), Color.Red);
        vertices[1] = new VertexPositionColor(new Vector3(s, -s, -s), Color.Green);
        vertices[2] = new VertexPositionColor(new Vector3(s, s, -s), Color.Blue);
        vertices[3] = new VertexPositionColor(new Vector3(-s, s, -s), Color.Yellow);
        vertices[4] = new VertexPositionColor(new Vector3(-s, -s, s), Color.Cyan);
        vertices[5] = new VertexPositionColor(new Vector3(s, -s, s), Color.Magenta);
        vertices[6] = new VertexPositionColor(new Vector3(s, s, s), Color.White);
        vertices[7] = new VertexPositionColor(new Vector3(-s, s, s), Color.Orange);

        var indices = new short[]
        {
            0,1,2, 0,2,3,
            4,6,5, 4,7,6,
            4,3,7, 4,0,3,
            1,5,6, 1,6,2,
            3,2,6, 3,6,7,
            4,5,1, 4,1,0
        };

        return (vertices, indices);
    }

    public static (VertexPositionColor[] vertices, short[] indices) CreateSphere(int stacks = 8, int slices = 12, float radius = 1f)
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

                var pos = new Vector3(x, y, z) * radius;
                // color by normal
                var col = new Color((x + 1f) * 0.5f, (y + 1f) * 0.5f, (z + 1f) * 0.5f);
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

        return (verts.ToArray(), inds.ToArray());
    }

    public static (VertexPositionColor[] vertices, short[] indices) CreatePyramid(float size = 1f, float height = 1.2f)
    {
        float s = size;
        var v = new VertexPositionColor[5];
        v[0] = new VertexPositionColor(new Vector3(-s, 0, -s), Color.Red);
        v[1] = new VertexPositionColor(new Vector3(s, 0, -s), Color.Green);
        v[2] = new VertexPositionColor(new Vector3(s, 0, s), Color.Blue);
        v[3] = new VertexPositionColor(new Vector3(-s, 0, s), Color.Yellow);
        v[4] = new VertexPositionColor(new Vector3(0, height, 0), Color.White);

        // base two triangles + 4 side triangles
        var idx = new short[]
        {
            0,1,2, 0,2,3,
            0,1,4,
            1,2,4,
            2,3,4,
            3,0,4
        };

        return (v, idx);
    }

    public static (VertexPositionColor[] vertices, short[] indices) CreateRectangularPrism(float width = 1.5f, float height = 0.8f, float depth = 0.6f)
    {
        float hw = width * 0.5f;
        float hh = height * 0.5f;
        float hd = depth * 0.5f;

        var v = new VertexPositionColor[8];
        v[0] = new VertexPositionColor(new Vector3(-hw, -hh, -hd), Color.CornflowerBlue);
        v[1] = new VertexPositionColor(new Vector3(hw, -hh, -hd), Color.CadetBlue);
        v[2] = new VertexPositionColor(new Vector3(hw, hh, -hd), Color.LightGreen);
        v[3] = new VertexPositionColor(new Vector3(-hw, hh, -hd), Color.Gold);
        v[4] = new VertexPositionColor(new Vector3(-hw, -hh, hd), Color.OrangeRed);
        v[5] = new VertexPositionColor(new Vector3(hw, -hh, hd), Color.MediumPurple);
        v[6] = new VertexPositionColor(new Vector3(hw, hh, hd), Color.White);
        v[7] = new VertexPositionColor(new Vector3(-hw, hh, hd), Color.PaleVioletRed);

        var idx = new short[]
        {
            0,1,2, 0,2,3,
            4,6,5, 4,7,6,
            4,3,7, 4,0,3,
            1,5,6, 1,6,2,
            3,2,6, 3,6,7,
            4,5,1, 4,1,0
        };

        return (v, idx);
    }
}
