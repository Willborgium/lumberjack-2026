using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lumberjack;

public class Skybox
{
    private readonly VertexPositionTexture[] _vertices;
    private readonly short[] _indices;
    private readonly Texture2D _texture;
    private readonly BasicEffect _effect;

    public Skybox(GraphicsDevice graphicsDevice, Texture2D texture, float size = 80f)
    {
        _texture = texture;
        _effect = new BasicEffect(graphicsDevice)
        {
            TextureEnabled = true,
            Texture = texture,
            VertexColorEnabled = false,
            LightingEnabled = false
        };

        float s = size * 0.5f;

        _vertices = new VertexPositionTexture[]
        {
            // Front
            new(new Vector3(-s, -s, -s), new Vector2(0, 1)),
            new(new Vector3(-s,  s, -s), new Vector2(0, 0)),
            new(new Vector3( s,  s, -s), new Vector2(1, 0)),
            new(new Vector3( s, -s, -s), new Vector2(1, 1)),
            // Back
            new(new Vector3( s, -s,  s), new Vector2(0, 1)),
            new(new Vector3( s,  s,  s), new Vector2(0, 0)),
            new(new Vector3(-s,  s,  s), new Vector2(1, 0)),
            new(new Vector3(-s, -s,  s), new Vector2(1, 1)),
            // Left
            new(new Vector3(-s, -s,  s), new Vector2(0, 1)),
            new(new Vector3(-s,  s,  s), new Vector2(0, 0)),
            new(new Vector3(-s,  s, -s), new Vector2(1, 0)),
            new(new Vector3(-s, -s, -s), new Vector2(1, 1)),
            // Right
            new(new Vector3( s, -s, -s), new Vector2(0, 1)),
            new(new Vector3( s,  s, -s), new Vector2(0, 0)),
            new(new Vector3( s,  s,  s), new Vector2(1, 0)),
            new(new Vector3( s, -s,  s), new Vector2(1, 1)),
            // Top
            new(new Vector3(-s,  s, -s), new Vector2(0, 1)),
            new(new Vector3(-s,  s,  s), new Vector2(0, 0)),
            new(new Vector3( s,  s,  s), new Vector2(1, 0)),
            new(new Vector3( s,  s, -s), new Vector2(1, 1)),
            // Bottom
            new(new Vector3(-s, -s,  s), new Vector2(0, 1)),
            new(new Vector3(-s, -s, -s), new Vector2(0, 0)),
            new(new Vector3( s, -s, -s), new Vector2(1, 0)),
            new(new Vector3( s, -s,  s), new Vector2(1, 1)),
        };

        _indices = new short[]
        {
            // Front
            0,1,2, 0,2,3,
            // Back
            4,5,6, 4,6,7,
            // Left
            8,9,10, 8,10,11,
            // Right
            12,13,14, 12,14,15,
            // Top
            16,17,18, 16,18,19,
            // Bottom
            20,21,22, 20,22,23
        };
    }

    public void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection, Vector3 cameraPosition)
    {
        // Remove translation from view to keep sky centered on camera
        Matrix viewNoTranslation = view;
        viewNoTranslation.Translation = Vector3.Zero;

        _effect.View = viewNoTranslation;
        _effect.Projection = projection;
        _effect.World = Matrix.CreateTranslation(cameraPosition);

        // Draw inside of cube
        var prevDepth = graphicsDevice.DepthStencilState;
        var prevRaster = graphicsDevice.RasterizerState;
        var prevBlend = graphicsDevice.BlendState;
        var prevSampler = graphicsDevice.SamplerStates[0];

        graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;
        graphicsDevice.BlendState = BlendState.Opaque;
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, _vertices, 0, _vertices.Length, _indices, 0, _indices.Length / 3);
        }

        graphicsDevice.DepthStencilState = prevDepth;
        graphicsDevice.RasterizerState = prevRaster;
        graphicsDevice.BlendState = prevBlend;
        graphicsDevice.SamplerStates[0] = prevSampler;
    }
}
