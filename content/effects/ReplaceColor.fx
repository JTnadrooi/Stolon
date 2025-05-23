#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler2D input : register(s0);
extern float4 dcolor1;
extern float4 dcolor2;

extern float4 color1;
extern float4 color2;

bool1 ColorEqual(float4 incolor1, float4 incolor2)
{
    float maxDelta = float(0.1);
    return (abs(incolor1.r - incolor2.r) < maxDelta && abs(incolor1.g - incolor2.g) < maxDelta && abs(incolor1.b - incolor2.b) < maxDelta);
}
float4 MainPS(float2 uv : TEXCOORD) : COLOR
{
    float4 color = tex2D(input, uv.xy);
    
    if (ColorEqual(color.rgba, dcolor1))
        return color1;
    if (ColorEqual(color.rgba, dcolor2))
        return color2;
    if (color.a == float(0))
        return color;
    return color;
}

technique MainDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};