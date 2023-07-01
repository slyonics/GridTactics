#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_3_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler s0;

float4x4 lightX;
float4x4 lightY;
float4x4 lightI;
float4x4 lightR;
float4x4 lightG;
float4x4 lightB;

float4 ambient;
float bloom;

float4 PixelShaderFunction(float4 position : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	float4 pixel = tex2D(s0, texCoord) * color1;

	float distance1 = 0;
	if (lightI._11 > 0) distance1 = (lightI._11 - sqrt(pow(position.x - lightX._11, 2) + pow(position.y - lightY._11, 2))) / pow(lightI._11, 2.3);
	if (distance1 < 0) distance1 = 0;

	float distance2 = 0;
	if (lightI._12 > 0) distance2 = (lightI._12 - sqrt(pow(position.x - lightX._12, 2) + pow(position.y - lightY._12, 2))) / pow(lightI._12, 2.3);
	if (distance2 < 0) distance2 = 0;

	float distance3 = 0;
	if (lightI._13 > 0) distance3 = (lightI._13 - sqrt(pow(position.x - lightX._13, 2) + pow(position.y - lightY._13, 2))) / pow(lightI._13, 2.3);
	if (distance3 < 0) distance3 = 0;

	float distance4 = 0;
	if (lightI._14 > 0) distance4 = (lightI._14 - sqrt(pow(position.x - lightX._14, 2) + pow(position.y - lightY._14, 2))) / pow(lightI._14, 2.3);
	if (distance4 < 0) distance4 = 0;

	float distance5 = 0;
	if (lightI._21 > 0) distance5 = (lightI._21 - sqrt(pow(position.x - lightX._21, 2) + pow(position.y - lightY._21, 2))) / pow(lightI._21, 2.3);
	if (distance5 < 0) distance5 = 0;

	float distance6 = 0;
	if (lightI._22 > 0) distance6 = (lightI._22 - sqrt(pow(position.x - lightX._22, 2) + pow(position.y - lightY._22, 2))) / pow(lightI._22, 2.3);
	if (distance6 < 0) distance6 = 0;

	float distance7 = 0;
	if (lightI._23 > 0) distance7 = (lightI._23 - sqrt(pow(position.x - lightX._23, 2) + pow(position.y - lightY._23, 2))) / pow(lightI._23, 2.3);
	if (distance7 < 0) distance7 = 0;

	float distance8 = 0;
	if (lightI._24 > 0) distance8 = (lightI._24 - sqrt(pow(position.x - lightX._24, 2) + pow(position.y - lightY._24, 2))) / pow(lightI._24, 2.3);
	if (distance8 < 0) distance8 = 0;

	float distance9 = 0;
	if (lightI._31 > 0) distance9 = (lightI._31 - sqrt(pow(position.x - lightX._31, 2) + pow(position.y - lightY._31, 2))) / pow(lightI._31, 2.3);
	if (distance9 < 0) distance9 = 0;

	float distance10 = 0;
	if (lightI._32 > 0) distance10 = (lightI._32 - sqrt(pow(position.x - lightX._32, 2) + pow(position.y - lightY._32, 2))) / pow(lightI._32, 2.3);
	if (distance10 < 0) distance10 = 0;

	float distance11 = 0;
	if (lightI._33 > 0) distance11 = (lightI._33 - sqrt(pow(position.x - lightX._33, 2) + pow(position.y - lightY._33, 2))) / pow(lightI._33, 2.3);
	if (distance11 < 0) distance11 = 0;

	float distance12 = 0;
	if (lightI._34 > 0) distance12 = (lightI._34 - sqrt(pow(position.x - lightX._34, 2) + pow(position.y - lightY._34, 2))) / pow(lightI._34, 2.3);;
	if (distance12 < 0) distance12 = 0;

	float redlight = distance1 * lightR._11 + distance2 * lightR._12 + distance3 * lightR._13 + distance4 * lightR._14 + distance5 * lightR._21 + distance6 * lightR._22 + distance7 * lightR._23 + distance8 * lightR._24 + distance9 * lightR._31 + distance10 * lightR._32 + distance11 * lightR._33 + distance11 * lightR._34;
	if (redlight > bloom) redlight = bloom;
	if (redlight < 0.0) redlight = 0.0;
	float greenlight = distance1 * lightG._11 + distance2 * lightG._12 + distance3 * lightG._13 + distance4 * lightG._14 + distance5 * lightG._21 + distance6 * lightG._22 + distance7 * lightG._23 + distance8 * lightG._24 + distance9 * lightG._31 + distance10 * lightG._32 + distance11 * lightG._33 + distance11 * lightG._34;
	if (greenlight > bloom) greenlight = bloom;
	if (greenlight < 0.0) greenlight = 0.0;
	float bluelight = distance1 * lightB._11 + distance2 * lightB._12 + distance3 * lightB._13 + distance4 * lightB._14 + distance5 * lightB._21 + distance6 * lightB._22 + distance7 * lightB._23 + distance8 * lightB._24 + distance9 * lightB._31 + distance10 * lightB._32 + distance11 * lightB._33 + distance11 * lightB._34;
	if (bluelight > bloom) bluelight = bloom;
	if (bluelight < 0.0) bluelight = 0.0;
	

	return float4(lerp(ambient.x * pixel.x, pixel.x, redlight), lerp(ambient.y * pixel.y, pixel.y, greenlight), lerp(ambient.z * pixel.z, pixel.z, bluelight), pixel.w);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}