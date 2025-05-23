#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0

Texture2D InputTexture : register(t0);
SamplerState InputSampler : register(s0);

// Inputs expected from SpriteBatch
struct PSInput
{
    float4 Position : SV_POSITION;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

static const float maxDelta = 0.1;

// Externally set colors
float4 dcolor1;
float4 dcolor2;
float4 color1;
float4 color2;

float4 MainPS(PSInput input) : SV_TARGET
{
    float4 src = InputTexture.Sample(InputSampler, input.TexCoord);

    if (src.a == 0.0) return src;

    float3 diff1 = abs(src.rgb - dcolor1.rgb);
    float3 diff2 = abs(src.rgb - dcolor2.rgb);

    bool match1 = all(diff1 <= maxDelta);
    bool match2 = all(diff2 <= maxDelta);

    float4 result = src;
    if (match2) result = color2;
    if (match1) result = color1;

    return result;
}

technique MainDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
