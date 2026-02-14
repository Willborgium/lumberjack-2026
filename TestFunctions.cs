using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public static class TestFunctions
{
    public static Renderable3D<VertexPositionNormalColor> CreateCube(Effect effect, int divisions = 2, float size = 1f)
    {
        // Create subdivided cube by generating a grid on each face and sharing vertices at same positions
        int div = Math.Max(1, divisions);
        float half = size * 0.5f;

        var positionsList = new List<Vector3>();
        var colorsList = new List<Color>();
        var indicesList = new List<short>();
        var indexMap = new Dictionary<string, short>();

        // Helper to get shared vertex index by position
        short GetIndex(Vector3 pos, Color col)
        {
            string key = $"{pos.X:F6}|{pos.Y:F6}|{pos.Z:F6}";
            if (indexMap.TryGetValue(key, out var idx)) return idx;
            idx = (short)positionsList.Count;
            positionsList.Add(pos);
            colorsList.Add(col);
            indexMap[key] = idx;
            return idx;
        }

        // define faces as center + two axes
        (Vector3 center, Vector3 u, Vector3 v, Color color)[] faces = new[]
        {
            // front (+Z)
            (new Vector3(0,0,half), new Vector3(1,0,0) * size, new Vector3(0,1,0) * size, Color.CornflowerBlue),
            // back (-Z)
            (new Vector3(0,0,-half), new Vector3(-1,0,0) * size, new Vector3(0,1,0) * size, Color.CadetBlue),
            // right (+X)
            (new Vector3(half,0,0), new Vector3(0,0,-1) * size, new Vector3(0,1,0) * size, Color.LightGreen),
            // left (-X)
            (new Vector3(-half,0,0), new Vector3(0,0,1) * size, new Vector3(0,1,0) * size, Color.Gold),
            // top (+Y)
            (new Vector3(0,half,0), new Vector3(1,0,0) * size, new Vector3(0,0,-1) * size, Color.OrangeRed),
            // bottom (-Y)
            (new Vector3(0,-half,0), new Vector3(1,0,0) * size, new Vector3(0,0,1) * size, Color.MediumPurple),
        };

        for (int f = 0; f < faces.Length; f++)
        {
            var face = faces[f];
            for (int iy = 0; iy <= div; iy++)
            {
                float vy = (iy / (float)div - 0.5f);
                for (int ix = 0; ix <= div; ix++)
                {
                    float vx = (ix / (float)div - 0.5f);
                    var pos = face.center + face.u * vx + face.v * vy;
                    var col = face.color;
                    GetIndex(pos, col);
                }
            }

            // create indices for this face grid
            int row = div + 1;
            // build temporary mapping from (ix,iy) to index
            short[,] map = new short[row, row];
            int counter = 0;
            for (int iy = 0; iy <= div; iy++)
            {
                float vy = (iy / (float)div - 0.5f);
                for (int ix = 0; ix <= div; ix++)
                {
                    float vx = (ix / (float)div - 0.5f);
                    var pos = face.center + face.u * vx + face.v * vy;
                    map[ix, iy] = GetIndex(pos, face.color);
                    counter++;
                }
            }

            for (int iy = 0; iy < div; iy++)
            {
                for (int ix = 0; ix < div; ix++)
                {
                    short a = map[ix, iy];
                    short b = map[ix + 1, iy];
                    short c = map[ix + 1, iy + 1];
                    short d = map[ix, iy + 1];
                    // two triangles with consistent winding for project-wide backface culling.
                    indicesList.Add(a);
                    indicesList.Add(c);
                    indicesList.Add(b);

                    indicesList.Add(a);
                    indicesList.Add(d);
                    indicesList.Add(c);
                }
            }
        }

        var positions = positionsList.ToArray();
        var colors = colorsList.ToArray();
        var indices = indicesList.ToArray();
        var normals = ComputeNormals(positions, indices);
        var verts = new VertexPositionNormalColor[positions.Length];
        for (int i = 0; i < positions.Length; i++) verts[i] = new VertexPositionNormalColor(positions[i], normals[i], colors[i]);

        return new Renderable3D<VertexPositionNormalColor>(effect, verts, indices);
    }

    public static Renderable3D<VertexPositionNormalColor> CreateSphere(Effect effect, int stacks = 8, int slices = 12, float radius = 1f)
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
        return new Renderable3D<VertexPositionNormalColor>(effect, vertsOut, inds.ToArray());
    }

    public static Renderable3D<VertexPositionNormalColor> CreatePyramid(Effect effect, float size = 1f, float height = 1.2f)
    {
        float s = size;
        var positions = new Vector3[5];
        positions[0] = new Vector3(-s, 0, -s);
        positions[1] = new Vector3(s, 0, -s);
        positions[2] = new Vector3(s, 0, s);
        positions[3] = new Vector3(-s, 0, s);
        positions[4] = new Vector3(0, height, 0);

        var colors = new Color[5] { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.White };

        var idx = new short[]
        {
            0,1,2, 0,2,3,
            4,1,0,
            4,2,1,
            4,3,2,
            4,0,3
        };

        var normals = ComputeNormals(positions, idx);
        var verts = new VertexPositionNormalColor[positions.Length];
        for (int i = 0; i < positions.Length; i++) verts[i] = new VertexPositionNormalColor(positions[i], normals[i], colors[i]);
        return new Renderable3D<VertexPositionNormalColor>(effect, verts, idx);
    }

    public static Renderable3D<VertexPositionNormalTextureColor> CreateTexturedPlane(Effect effect, Texture2D texture, float width = 10f, float depth = 10f, float uvScale = 1f)
    {
        float hw = width * 0.5f;
        float hd = depth * 0.5f;

        var normal = Vector3.Up;
        var color = Color.White;

        var verts = new VertexPositionNormalTextureColor[4];
        // positions on XZ plane, Y = 0
        verts[0] = new VertexPositionNormalTextureColor(new Vector3(-hw, 0, -hd), normal, new Vector2(0, uvScale), color);
        verts[1] = new VertexPositionNormalTextureColor(new Vector3(hw, 0, -hd), normal, new Vector2(uvScale, uvScale), color);
        verts[2] = new VertexPositionNormalTextureColor(new Vector3(hw, 0, hd), normal, new Vector2(uvScale, 0), color);
        verts[3] = new VertexPositionNormalTextureColor(new Vector3(-hw, 0, hd), normal, new Vector2(0, 0), color);

        // CCW winding when viewed from above (+Y), normal points upward
        var inds = new short[]
        {
            0, 2, 1,
            0, 3, 2
        };

        return new Renderable3D<VertexPositionNormalTextureColor>(effect, verts, inds, texture);
    }

    public static Renderable3D<VertexPositionNormalColor> CreateRectangularPrism(Effect effect, float width = 1.5f, float height = 0.8f, float depth = 0.6f)
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

        var colors = new Color[8] { Color.CornflowerBlue, Color.CadetBlue, Color.LightGreen, Color.Gold, Color.OrangeRed, Color.MediumPurple, Color.White, Color.PaleVioletRed };

        // Face winding is intentionally consistent with the project's global culling convention.
        var idx = new short[]
        {
            // back (-Z)
            1,3,0, 1,2,3,
            // front (+Z)
            4,6,5, 4,7,6,
            // left (-X)
            0,7,4, 0,3,7,
            // right (+X)
            5,2,1, 5,6,2,
            // top (+Y)
            3,6,7, 3,2,6,
            // bottom (-Y)
            0,5,1, 0,4,5
        };

        var normals = ComputeNormals(positions, idx);
        var verts = new VertexPositionNormalColor[positions.Length];
        for (int i = 0; i < positions.Length; i++) verts[i] = new VertexPositionNormalColor(positions[i], normals[i], colors[i]);
        return new Renderable3D<VertexPositionNormalColor>(effect, verts, idx);
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
