// Combines two images into one warped Side-by-Side image


sampler2D TexMap;

float4 HmdWarpParam;
float2 TexCoordOffset;
float4 TCScaleMat;
float AspectRatio;
float2 Scale;
float PupilOffset;

float4 SBSRift(float2 tc : TEXCOORD0) : COLOR
{
	
    float2 newtc = tc;
    
    newtc.x += PupilOffset;
    float d = newtc.x * newtc.x + newtc.y * newtc.y;
    newtc *= (HmdWarpParam.x + d*(HmdWarpParam.y + d*(HmdWarpParam.z + HmdWarpParam.w*d)));

    float2 newtc2;
    newtc2.x = newtc.x*TCScaleMat.x + newtc.y*TCScaleMat.y;
    newtc2.y = newtc.x*TCScaleMat.z + newtc.y*TCScaleMat.w;

    newtc2.x *= Scale.x;
    newtc2.x *= AspectRatio;
    newtc2.y *= Scale.y;
    newtc2.x += 0.25;
    newtc2.y += 0.5;
    newtc2.x = (newtc2.x > 0.5) ? 10.0 : newtc2.x;
    newtc2.x = (newtc2.x < 0.0) ? 10.0 : newtc2.x;
    newtc2.x += TexCoordOffset.x;
    
    return tex2D(TexMap, newtc2);
	//return tex2D(TexMap, tc);
    //return float4(newtc2.x, newtc2.x, newtc2.x, 1.0);
}

float2 textureCoordsToDistortionOffsetCoords(float2 texCoord)
{
	float lensCenterOffset = 0.15f;
	float aspect = 0.8f;
	float2 result = texCoord * 2.0f - 1.0f;
	result -= lensCenterOffset;
	result.y /= aspect;
	return result;
}

float2 distortionOffsetCoordsToTextureCoords(float2 distCoord)
{
	float lensCenterOffset = 0.15f;
	float aspect = 0.8f;
	float scale = 1.5f;
	float2 result = distCoord / scale;
	result.y *= aspect;
	result += lensCenterOffset;
	result += 1.0f;
	result /= 2.0f;
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

float4 RiftPS(float2 tc : TEXCOORD0) : COLOR
{
	float2 newtc = textureCoordsToDistortionOffsetCoords(tc);
	newtc.x *= distortionScale(newtc);
	newtc.y *= distortionScale(newtc);
	float2 newtc2 = distortionOffsetCoordsToTextureCoords(newtc);

	return tex2D(TexMap, newtc2);
}

/*
Texture2D Texture : register(t0);
SamplerState Linear : register(s0);
float2 LensCenter;
float2 ScreenCenter;
float2 Scale;
float2 ScaleIn;
float4 HmdWarpParam;

// Scales input texture coordinates for distortion.
// ScaleIn maps texture coordinates to Scales to ([-1, 1]), although top/bottom will be
// larger due to aspect ratio.
float2 HmdWarp(float2 in01)
{
   float2 theta = (in01 - LensCenter) * ScaleIn; // Scales to [-1, 1]
   float  rSq = theta.x * theta.x + theta.y * theta.y;
   float2 theta1 = theta * (HmdWarpParam.x + HmdWarpParam.y * rSq + 
                   HmdWarpParam.z * rSq * rSq + HmdWarpParam.w * rSq * rSq * rSq);
   return LensCenter + Scale * theta1;
}

float4 RiftWarp(float2 tc : TEXCOORD0) : COLOR
{
   float2 newtc = HmdWarp(tc);
   if (any(clamp(tc, ScreenCenter-float2(0.25,0.5), ScreenCenter+float2(0.25, 0.5)) - tc))
       return 0;
   return Texture.Sample(Linear, tc);
};
*/

technique ViewShader
{
    pass P0
    {
        VertexShader = null;
        //PixelShader = compile ps_2_0 SBSRift();
		PixelShader = compile ps_2_0 RiftPS();
    }
}