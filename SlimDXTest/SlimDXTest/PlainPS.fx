sampler2D TexMap;
float4 HmdWarpParam;
float LensCenterOffset;
float AspectRatio;
float2 Scale;

float2 textureCoordsToDistortionOffsetCoords(float2 texCoord)
{
	float2 result = texCoord;
	result.x *= 2.0f;
	result.x -= 1.0f;
	result.x *= 0.5f;
	result.y *= 2.0f;
	result.y -= 1.0f;
	result.y *= 1.1f;
	result.x -= LensCenterOffset;
	result.y /= AspectRatio;
	return result;
}

float2 distortionOffsetCoordsToTextureCoords(float2 distCoord)
{
	float2 result;
	result.x = distCoord.x / Scale.x;
	result.y = distCoord.y / Scale.y;
	result.y *= AspectRatio;
	result.x += LensCenterOffset;
	result.x += 1.0f;
	result.x /= 2.0f;
	result.y += 1.0f;
	result.y /= 2.0f;
	return result;
}

float distortionScale(float2 offset)
{
	float2 offsetSq;
	offsetSq.x = offset.x * offset.x;
	offsetSq.y = offset.y * offset.y;
	float radiusSquared = offsetSq.x + offsetSq.y;
	float distortionScale = HmdWarpParam.x + HmdWarpParam.y * radiusSquared + HmdWarpParam.z * radiusSquared * radiusSquared + HmdWarpParam.w  * radiusSquared * radiusSquared * radiusSquared;
	return distortionScale;
}

float4 PlainPS(float2 tc : TEXCOORD0) : COLOR
{
	return tex2D(TexMap, tc);
}

technique ViewShader
{
	pass P0
	{
		VertexShader = null;
		PixelShader = compile ps_2_0 PlainPS();
	}
}