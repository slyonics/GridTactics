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
	float2 tileSample = float2(round(texCoord.x * 20) / 20.0f + 8.0f / 320.0f, round(texCoord.y * 15) / 15.0f);
	float angle = (atan2(tileSample.y - 0.5f, tileSample.x - 0.5f) + 3.14159265f) * 180 / 3.14159265f;

	float4 color = tex2D(s0, texCoord) * color1;
	if (90 - (angle % 90) + 15 > amount * 90)
	{
		color.r = filterRed;
		color.g = filterGreen;
		color.b = filterBlue;
		color.a = 1;
	}

	return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}