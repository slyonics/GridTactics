#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_3_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler s0;

float time;
float amplitude;

float red;
float green;
float blue;
float transparency;

float4 PixelShaderFunction(float4 position : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	texCoord.x = texCoord.x + sin(time + texCoord.y * 2) * amplitude * 0.005f;
	float4 Color = tex2D(s0, texCoord);
	float4 Result;
	
	Result.r = (red * transparency) + (Color.r * (1.0f - transparency));
	Result.g = (green * transparency) + (Color.g * (1.0f - transparency));
	Result.b = (blue * transparency) + (Color.b * (1.0f - transparency));
	Result.a = 1.0f;
	
	return Result;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}