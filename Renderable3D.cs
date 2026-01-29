using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public abstract class Renderable3DBase
{
    private Vector3 _position = Vector3.Zero;
    private Vector3 _rotation = Vector3.Zero; // pitch(x), yaw(y), roll(z)
    private Vector3 _scale = Vector3.One;

    private Matrix _worldCache = Matrix.Identity;
    private bool _dirty = true;

    public bool EnableAutoRotation { get; set; } = true;
    public CullMode CullMode { get; set; } = CullMode.CullClockwiseFace;

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

    private void RecalculateWorld()
    {
        var rot = Matrix.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z);
        var scl = Matrix.CreateScale(_scale);
        var trans = Matrix.CreateTranslation(_position);
        _worldCache = scl * rot * trans;
        _dirty = false;
    }

    public abstract void Draw(BasicEffect effect, GraphicsDevice graphicsDevice);
    public abstract IReadOnlyList<Vector3> GetPositions();
    public abstract IReadOnlyList<(short, short)> GetEdges();
}

public class Renderable3D<T> : Renderable3DBase where T : struct, IVertexType
{
    private readonly T[] _vertices;
    private readonly short[] _indices;
    private readonly Texture2D? _texture;

    private Vector3[]? _positionCache;
    private (short, short)[]? _edgeCache;
    private static readonly Func<T, Vector3>? _positionAccessor = CreatePositionAccessor();

    public Renderable3D(T[] vertices, short[] indices)
    {
        _vertices = vertices;
        _indices = indices;
    }

    public Renderable3D(T[] vertices, short[] indices, Texture2D texture)
    {
        _vertices = vertices;
        _indices = indices;
        _texture = texture;
    }

    public override IReadOnlyList<Vector3> GetPositions()
    {
        if (_positionAccessor == null)
        {
            return Array.Empty<Vector3>();
        }

        if (_positionCache == null)
        {
            var arr = new Vector3[_vertices.Length];
            for (int i = 0; i < _vertices.Length; i++)
            {
                arr[i] = _positionAccessor(_vertices[i]);
            }
            _positionCache = arr;
        }

        return _positionCache;
    }

    public override IReadOnlyList<(short, short)> GetEdges()
    {
        if (_edgeCache != null) return _edgeCache;

        var edges = new HashSet<(short, short)>();

        for (int i = 0; i + 2 < _indices.Length; i += 3)
        {
            short i0 = _indices[i];
            short i1 = _indices[i + 1];
            short i2 = _indices[i + 2];

            AddEdge(edges, i0, i1);
            AddEdge(edges, i1, i2);
            AddEdge(edges, i2, i0);
        }

        _edgeCache = new (short, short)[edges.Count];
        edges.CopyTo(_edgeCache);
        return _edgeCache;
    }

    public override void Draw(BasicEffect effect, GraphicsDevice graphicsDevice)
    {
        effect.World = World;

        graphicsDevice.RasterizerState = CullMode switch
        {
            CullMode.CullClockwiseFace => RasterizerState.CullClockwise,
            CullMode.CullCounterClockwiseFace => RasterizerState.CullCounterClockwise,
            _ => RasterizerState.CullNone
        };

        if (_texture != null)
        {
            effect.TextureEnabled = true;
            effect.Texture = _texture;
            graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }
        else
        {
            effect.TextureEnabled = false;
            graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        }

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives<T>(
                PrimitiveType.TriangleList,
                _vertices, 0, _vertices.Length,
                _indices, 0, _indices.Length / 3);
        }
    }

    private static void AddEdge(HashSet<(short, short)> edges, short a, short b)
    {
        if (a == b) return;
        edges.Add(a < b ? (a, b) : (b, a));
    }

    private static Func<T, Vector3>? CreatePositionAccessor()
    {
        if (typeof(T) == typeof(VertexPositionNormalColor))
        {
            return static (T v) => Unsafe.As<T, VertexPositionNormalColor>(ref v).Position;
        }

        if (typeof(T) == typeof(VertexPositionNormalTextureColor))
        {
            return static (T v) => Unsafe.As<T, VertexPositionNormalTextureColor>(ref v).Position;
        }

        return null;
    }
}
