using System;
using System.Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public class Skybox : Renderable3D<VertexPositionTexture>, IUpdatable
{
    private readonly Func<Vector3> _positionProvider;

    public Skybox(Effect effect, GraphicsDevice graphicsDevice, Camera camera, Texture2D texture, float size = 80f)
        : base(effect, Vertices, Indices)
    {
        EnableAutoRotation = false;
        Scale = new Vector3(size * 0.5f);
        Effect = new BasicEffect(graphicsDevice)
        {
            TextureEnabled = true,
            Texture = texture,
            VertexColorEnabled = false,
            LightingEnabled = false
        };

        CullMode = CullMode.None;

        _positionProvider = () => camera.Position;
    }

    public void Update(GameTime gameTime)
    {
        Position = _positionProvider();
    }

    public override bool SetState(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;
        graphicsDevice.BlendState = BlendState.Opaque;
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        return true;
    }

    private static readonly short[] Indices =
    [
        // Front
        0, 1, 2,
        0, 2, 3,
        // Back
        4, 5, 6,
        4, 6, 7,
        // Left
        8, 9, 10,
        8, 10, 11,
        // Right
        12, 13, 14,
        12, 14, 15,
        // Top
        16, 17, 18,
        16, 18, 19,
        // Bottom
        20, 21, 22,
        20, 22, 23
    ];

    private static readonly VertexPositionTexture[] Vertices =
        [
            // Front
            new(new Vector3(-1, -1, -1), new Vector2(0, 1)),
            new(new Vector3(-1,  1, -1), new Vector2(0, 0)),
            new(new Vector3( 1,  1, -1), new Vector2(1, 0)),
            new(new Vector3( 1, -1, -1), new Vector2(1, 1)),
            // Back
            new(new Vector3( 1, -1,  1), new Vector2(0, 1)),
            new(new Vector3( 1,  1,  1), new Vector2(0, 0)),
            new(new Vector3(-1,  1,  1), new Vector2(1, 0)),
            new(new Vector3(-1, -1,  1), new Vector2(1, 1)),
            // Left
            new(new Vector3(-1, -1,  1), new Vector2(0, 1)),
            new(new Vector3(-1,  1,  1), new Vector2(0, 0)),
            new(new Vector3(-1,  1, -1), new Vector2(1, 0)),
            new(new Vector3(-1, -1, -1), new Vector2(1, 1)),
            // Right
            new(new Vector3( 1, -1, -1), new Vector2(0, 1)),
            new(new Vector3( 1,  1, -1), new Vector2(0, 0)),
            new(new Vector3( 1,  1,  1), new Vector2(1, 0)),
            new(new Vector3( 1, -1,  1), new Vector2(1, 1)),
            // Top
            new(new Vector3(-1,  1, -1), new Vector2(0, 1)),
            new(new Vector3(-1,  1,  1), new Vector2(0, 0)),
            new(new Vector3( 1,  1,  1), new Vector2(1, 0)),
            new(new Vector3( 1,  1, -1), new Vector2(1, 1)),
            // Bottom
            new(new Vector3(-1, -1,  1), new Vector2(0, 1)),
            new(new Vector3(-1, -1, -1), new Vector2(0, 0)),
            new(new Vector3( 1, -1, -1), new Vector2(1, 0)),
            new(new Vector3( 1, -1,  1), new Vector2(1, 1)),
        ];
}
