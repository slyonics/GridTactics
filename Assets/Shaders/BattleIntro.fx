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
sampler noise;

float4 PixelShaderFunction(float4 position : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	if (texCoord.y < 110.0f / 160.0f)
	{
		float2 coord = float2(round(texCoord.x * 40) / 40, round(texCoord.y * 30) / 30);

		if (amount <= tex2D(noise, coord).x)
		{
			return float4(0, 0, 0, 1);
		}
	}

	float4 color = tex2D(s0, texCoord) * color1;
	float transition = round(amount * 6) / 6.0f;

	color.r = lerp(filterRed, color.r, transition);
	color.g = lerp(filterGreen, color.g, transition);
	color.b = lerp(filterBlue, color.b, transition);
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