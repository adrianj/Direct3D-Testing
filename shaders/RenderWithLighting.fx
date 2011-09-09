//
// Global Variables
//
float4x4 ViewProj;
float4x4 World;
float4x4 LocalRotation;
float4 LightDir = (1,1,1,1);
float AmbientIntensity = 0.9f;
float DirectionalIntensity = 0.1f;

//
// Textures.
// I'm writing an app where simple diffuse colored objects appear in the same scene as textured ones,
// so I'm using the first two fields of the COLOR vector as the TEXCOORDs, and an int indicating which index to use
// (-1 or anything less than zero to use diffuse color)
//
int TextureIndex;
Texture2D <float4> Texture_0;
Texture2D <float4> Texture_1;
Texture2D <float4> Texture_2;
Texture2D <float4> Texture_3;
Texture2D <float4> Texture_4;
Texture2D <float4> Texture_5;
Texture2D <float4> Texture_6;
Texture2D <float4> Texture_7;
Texture2D <float4> Texture_8;
Texture2D <float4> Texture_9;
Texture2D <float4> Texture_10;
Texture2D <float4> Texture_11;
Texture2D <float4> Texture_12;
Texture2D <float4> Texture_13;
Texture2D <float4> Texture_14;
Texture2D <float4> Texture_15;


SamplerState TextureSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};
 
struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 norm : NORMAL;
	float4 col : COLOR;
};

float4 TransformPosition(float4 pos)
{
	return mul(pos,mul(World,ViewProj));
}

float4 TransformNormal(float4 norm)
{
	return normalize(mul(norm,LocalRotation));
}

//
// Colored With Lighting
//
PS_IN VS(float4 inPos : POSITION, float4 inNorm : NORMAL, float4 inColor : COLOR)
{
	PS_IN output = (PS_IN)0;
	output.pos = TransformPosition(inPos);
	output.norm = TransformNormal(inNorm);
	output.col = inColor;
	
	return output;
}

float CalculateIntensity(float4 inNorm)
{
	if(AmbientIntensity >= 1.0 || DirectionalIntensity <= 0.0)
		return 1.0;
	float dotProd = dot((float3)inNorm,(float3)LightDir);
	float directionalIntensity = dotProd * DirectionalIntensity;
	return saturate(AmbientIntensity + directionalIntensity);
}

float4 CalculateColor(float4 inColor)
{
	float4 color = inColor;
	color.w = 1;

	if(TextureIndex >= 0)
	{
		float2 texCoord = float2(inColor.x,inColor.y);
		
		texCoord.x = 1-inColor.x;
		texCoord.y = 1-inColor.y;
		if(TextureIndex == 0)
			color = Texture_0.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 1)
			color = Texture_1.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 2)
			color = Texture_2.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 3)
			color = Texture_3.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 4)
			color = Texture_4.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 5)
			color = Texture_5.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 6)
			color = Texture_6.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 7)
			color = Texture_7.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 8)
			color = Texture_8.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 9)
			color = Texture_9.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 10)
			color = Texture_10.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 11)
			color = Texture_11.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 12)
			color = Texture_12.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 13)
			color = Texture_13.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 14)
			color = Texture_14.Sample(TextureSampler,texCoord );
		else if(TextureIndex == 15)
			color = Texture_15.Sample(TextureSampler,texCoord );
	}
	return color;
}

 
float4 PS( PS_IN input ) : SV_Target
{
	float4 final;
	float finalIntensity = 1.0f;
	float4 color = CalculateColor(input.col);
	finalIntensity = CalculateIntensity(input.norm);
	final =  finalIntensity*color;
	// Add back in Alpha, as it will be handled by the Alpha Blend operation.
	final.w = color.w * input.col.w;
	return final;
}
 
technique10 Render
{
	pass ColoredWithLighting
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}