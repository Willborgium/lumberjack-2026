using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public static class TestFunctions
{
    public static (VertexPositionNormalColor[] vertices, short[] indices) CreateCube()
    {
        var positions = new Vector3[8];
        float s = 1f;
        positions[0] = new Vector3(-s, -s, -s);
        positions[1] = new Vector3(s, -s, -s);
        positions[2] = new Vector3(s, s, -s);
        positions[3] = new Vector3(-s, s, -s);
        positions[4] = new Vector3(-s, -s, s);
        positions[5] = new Vector3(s, -s, s);
        positions[6] = new Vector3(s, s, s);
        positions[7] = new Vector3(-s, s, s);

        var colors = new Color[8];
        colors[0] = Color.Red;
        colors[1] = Color.Green;
        colors[2] = Color.Blue;
        colors[3] = Color.Yellow;
        colors[4] = Color.Cyan;
        colors[5] = Color.Magenta;
        colors[6] = Color.White;
        colors[7] = Color.Orange;

        var indices = new short[]
        {
            0,1,2, 0,2,3,
            4,6,5, 4,7,6,
            4,3,7, 4,0,3,
            1,5,6, 1,6,2,
            3,2,6, 3,6,7,
            4,5,1, 4,1,0
        };

        var normals = ComputeNormals(positions, indices);
        var verts = new VertexPositionNormalColor[positions.Length];
        for (int i = 0; i < positions.Length; i++) verts[i] = new VertexPositionNormalColor(positions[i], normals[i], colors[i]);
        return (verts, indices);
    }

    public static (VertexPositionNormalColor[] vertices, short[] indices) CreateSphere(int stacks = 8, int slices = 12, float radius = 1f)
    {
        var positions = new List<Vector3>();
        var colors = new List<Color>();
        var inds = new List<short>();

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
                var col = new Color((x + 1f) * 0.5f, (y + 1f) * 0.5f, (z + 1f) * 0.5f);
                positions.Add(pos);
                colors.Add(col);
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

        var posArr = positions.ToArray();
        var colArr = colors.ToArray();
        var normals = ComputeNormals(posArr, inds.ToArray());
        var vertsOut = new VertexPositionNormalColor[posArr.Length];
        for (int i = 0; i < posArr.Length; i++) vertsOut[i] = new VertexPositionNormalColor(posArr[i], normals[i], colArr[i]);
        return (vertsOut, inds.ToArray());
    }

    public static (VertexPositionNormalColor[] vertices, short[] indices) CreatePyramid(float size = 1f, float height = 1.2f)
    {
        float s = size;
        var positions = new Vector3[5];
        positions[0] = new Vector3(-s, 0, -s);
        positions[1] = new Vector3(s, 0, -s);
        positions[2] = new Vector3(s, 0, s);
        positions[3] = new Vector3(-s, 0, s);
        positions[4] = new Vector3(0, height, 0);

        var colors = new Color[5]{ Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.White };

        var idx = new short[]
        {
            0,1,2, 0,2,3,
            0,1,4,
            1,2,4,
            2,3,4,
            3,0,4
        };

        var normals = ComputeNormals(positions, idx);
        var verts = new VertexPositionNormalColor[positions.Length];
        for (int i = 0; i < positions.Length; i++) verts[i] = new VertexPositionNormalColor(positions[i], normals[i], colors[i]);
        return (verts, idx);
    }

    public static (VertexPositionNormalColor[] vertices, short[] indices) CreateRectangularPrism(float width = 1.5f, float height = 0.8f, float depth = 0.6f)
    {
        float hw = width * 0.5f;
        float hh = height * 0.5f;
        float hd = depth * 0.5f;

        var positions = new Vector3[8];
        positions[0] = new Vector3(-hw, -hh, -hd);
        positions[1] = new Vector3(hw, -hh, -hd);
        positions[2] = new Vector3(hw, hh, -hd);
        positions[3] = new Vector3(-hw, hh, -hd);
        positions[4] = new Vector3(-hw, -hh, hd);
        positions[5] = new Vector3(hw, -hh, hd);
        positions[6] = new Vector3(hw, hh, hd);
        positions[7] = new Vector3(-hw, hh, hd);

        var colors = new Color[8]{ Color.CornflowerBlue, Color.CadetBlue, Color.LightGreen, Color.Gold, Color.OrangeRed, Color.MediumPurple, Color.White, Color.PaleVioletRed };

        var idx = new short[]
        {
            0,1,2, 0,2,3,
            4,6,5, 4,7,6,
            4,3,7, 4,0,3,
            1,5,6, 1,6,2,
            3,2,6, 3,6,7,
            4,5,1, 4,1,0
        };

        var normals = ComputeNormals(positions, idx);
        var verts = new VertexPositionNormalColor[positions.Length];
        for (int i = 0; i < positions.Length; i++) verts[i] = new VertexPositionNormalColor(positions[i], normals[i], colors[i]);
        return (verts, idx);
    }

    private static Vector3[] ComputeNormals(Vector3[] positions, short[] indices)
    {
        var normals = new Vector3[positions.Length];
        for (int i = 0; i < normals.Length; i++) normals[i] = Vector3.Zero;

        for (int i = 0; i < indices.Length; i += 3)
        {
            int a = indices[i];
            int b = indices[i + 1];
            int c = indices[i + 2];

            var p0 = positions[a];
            var p1 = positions[b];
            var p2 = positions[c];

            var edge1 = p1 - p0;
            var edge2 = p2 - p0;
            var n = Vector3.Cross(edge1, edge2);
            if (n.LengthSquared() > 0.000001f) n.Normalize();

            normals[a] += n;
            normals[b] += n;
            normals[c] += n;
        }

        for (int i = 0; i < normals.Length; i++)
        {
            if (normals[i].LengthSquared() > 0.000001f) normals[i].Normalize();
        }

        return normals;
    }
}
