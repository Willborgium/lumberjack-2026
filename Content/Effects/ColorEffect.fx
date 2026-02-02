float4x4 World;
float4x4 View;
float4x4 Projection;

float3 LightDirection = normalize(float3(-0.5f, -1.0f, -0.3f));
float3 DiffuseColor = float3(1.0f, 1.0f, 1.0f);
float3 AmbientColor = float3(0.18f, 0.18f, 0.18f);

struct VSInput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float4 Color    : COLOR0;
};

struct VSOutput
{
    float4 Position : SV_Position;
    float4 Color    : COLOR0;
};

VSOutput VSMain(VSInput input)
{
    VSOutput output;

    float4 worldPos = mul(input.Position, World);
    float3 worldNormal = normalize(mul(input.Normal, (float3x3)World));
    float NdotL = saturate(dot(worldNormal, -LightDirection));
    float3 lighting = AmbientColor + DiffuseColor * NdotL;

    output.Color = float4(input.Color.rgb * lighting, input.Color.a);
    output.Position = mul(worldPos, View);
    output.Position = mul(output.Position, Projection);
    return output;
}

float4 PSMain(VSOutput input) : SV_Target
{
    return input.Color;
}

technique BasicColor
{
    pass P0
    {
        VertexShader = compile vs_4_0_level_9_1 VSMain();
        PixelShader  = compile ps_4_0_level_9_1 PSMain();
    }
}
