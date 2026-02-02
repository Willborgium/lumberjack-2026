float4x4 World;
float4x4 View;
float4x4 Projection;

Texture2D Texture;
SamplerState TextureSampler
{
    Filter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VSInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
};

VSOutput VSMain(VSInput input)
{
    VSOutput output;
    float4 worldPos = mul(input.Position, World);
    output.Position = mul(worldPos, View);
    output.Position = mul(output.Position, Projection);
    output.TexCoord = input.TexCoord;
    return output;
}

float4 PSMain(VSOutput input) : SV_Target
{
    return Texture.Sample(TextureSampler, input.TexCoord);
}

technique Skybox
{
    pass P0
    {
        VertexShader = compile vs_4_0_level_9_1 VSMain();
        PixelShader  = compile ps_4_0_level_9_1 PSMain();
    }
}
