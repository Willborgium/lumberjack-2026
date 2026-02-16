float4x4 World;
float4x4 View;
float4x4 Projection;

float3 LightDirection = normalize(float3(-0.4f, -1.0f, -0.25f));
float3 AmbientColor = float3(0.2f, 0.2f, 0.2f);
float3 LightColor = float3(1.0f, 1.0f, 1.0f);
float ToonLevels = 4.0f;

Texture2D Texture;
SamplerState TextureSampler
{
    Filter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VSInputColor
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float4 Color    : COLOR0;
};

struct VSInputTextured
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float4 Color    : COLOR0;
};

struct VSOutputColor
{
    float4 Position : SV_Position;
    float3 Lighting : TEXCOORD0;
    float4 Color    : COLOR0;
};

struct VSOutputTextured
{
    float4 Position : SV_Position;
    float3 Lighting : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
    float4 Color    : COLOR0;
};

float3 Quantize(float3 value)
{
    float levels = max(ToonLevels, 1.0f);
    return floor(value * levels) / (levels - 1.0f);
}

VSOutputColor VSColor(VSInputColor input)
{
    VSOutputColor output;
    float4 worldPos = mul(input.Position, World);
    float3 worldNormal = normalize(mul(input.Normal, (float3x3)World));

    float ndotl = saturate(dot(worldNormal, -LightDirection));
    float3 lit = AmbientColor + LightColor * ndotl;
    output.Lighting = Quantize(saturate(lit));

    output.Color = input.Color;
    output.Position = mul(worldPos, View);
    output.Position = mul(output.Position, Projection);
    return output;
}

float4 PSColor(VSOutputColor input) : SV_Target
{
    return float4(input.Color.rgb * input.Lighting, input.Color.a);
}

VSOutputTextured VSTextured(VSInputTextured input)
{
    VSOutputTextured output;
    float4 worldPos = mul(input.Position, World);
    float3 worldNormal = normalize(mul(input.Normal, (float3x3)World));

    float ndotl = saturate(dot(worldNormal, -LightDirection));
    float3 lit = AmbientColor + LightColor * ndotl;
    output.Lighting = Quantize(saturate(lit));

    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    output.Position = mul(worldPos, View);
    output.Position = mul(output.Position, Projection);
    return output;
}

float4 PSTextured(VSOutputTextured input) : SV_Target
{
    float4 tex = Texture.Sample(TextureSampler, input.TexCoord);
    float3 shaded = tex.rgb * input.Color.rgb * input.Lighting;
    return float4(shaded, tex.a * input.Color.a);
}

technique CelColor
{
    pass P0
    {
        VertexShader = compile vs_4_0_level_9_1 VSColor();
        PixelShader  = compile ps_4_0_level_9_1 PSColor();
    }
}

technique CelTextured
{
    pass P0
    {
        VertexShader = compile vs_4_0_level_9_1 VSTextured();
        PixelShader  = compile ps_4_0_level_9_1 PSTextured();
    }
}
