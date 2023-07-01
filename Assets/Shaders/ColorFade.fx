#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_3_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float filterRed;
float filterGreen;
float filterBlue;

float amount;

sampler s0;

float4 PixelShaderFunction(float4 position : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	float4 color = tex2D(s0, texCoord) * color1;

	//float transition = round(amount * 6) / 6.0f;

	color.r = lerp(filterRed, color.r, amount);
	color.g = lerp(filterGreen, color.g, amount);
	color.b = lerp(filterBlue, color.b, amount);
	color.a = 1;
 
    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}