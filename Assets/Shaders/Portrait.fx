#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_3_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float destroyInterval;
float flashInterval;
float4 flashColor;

sampler s0;
sampler noise;

float4 PixelShaderFunction(float4 position : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	if (destroyInterval <= tex2D(noise, texCoord).x)
	{
		return float4(0, 0, 0, 0);
	}

	float4 color = tex2D(s0, texCoord) * color1;
	if (color.a <= 0) return color;

	color.a = lerp(color.a, flashColor.w, flashInterval);
	color.r = lerp(color.r, flashColor.x, flashInterval);
	color.g = lerp(color.g, flashColor.y, flashInterval);
	color.b = lerp(color.b, flashColor.z, flashInterval);

	return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}