#if OPENGL
#define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler2D input : register(s0);

// threshold for “close enough”
static const float maxDelta = 0.1;

// target and replacement colors
extern float4 dcolor1;
extern float4 dcolor2;
extern float4 color1;
extern float4 color2;

float4 MainPS(float2 uv : TEXCOORD) : COLOR
{
    float4 src = tex2D(input, uv);
    
    // if fully transparent, just return as-is
    if (src.a == 0.0)
        return src;

    // compute absolute channel-wise deltas
    float3 diff1 = abs(src.rgb - dcolor1.rgb);
    float3 diff2 = abs(src.rgb - dcolor2.rgb);

    // all channels within threshold?
    bool match1 = all(diff1 <= maxDelta);
    bool match2 = all(diff2 <= maxDelta);

    // pick final color: match1 → color1, match2 → color2, else → src
    // note: if both match (unlikely), priority goes to color1
    float4 result = src;
    result = match2 ? color2 : result;
    result = match1 ? color1 : result;
    
    return result;
}

technique MainDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};