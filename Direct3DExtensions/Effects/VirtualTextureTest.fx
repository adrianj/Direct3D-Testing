#include "Structs.fxh"
#include "samplers.fxh"

Texture2D MyTexture;
float InverseTextureSize = 0.03125;

float4 GetPosition( float4 pos )
{
	float4 output = pos;
	output = mul ( output, World);
	output = mul ( output, View );
	output = mul ( output, Proj );
	return output;
}

PS_TEX VS_Pos_Normal ( VS_POS_TEX input )
{
	PS_TEX output = (PS_TEX)0;
	
	output.pos = GetPosition(float4(input.pos, 1.0));
	output.uv = input.uv;
	
	return output;
}

/*
	tex: the Texture2D we're sampling.
	uv: tex coordinate, eg, worked out from world position.
	offset: offset before tex lookup, eg, from camera position.
	scale: DisplayedTextureSize / ActualTextureSize.
*/
float4 SampleVirtualTexture(Texture2D tex, float2 uv, float2 offset,float scale)
{
	uv = (uv - offset)*scale;
	return tex.Sample(LinearWrapSampler, uv);
}

// The most basic virtual texture pixel shader
float4 PS_Final( PS_TEX input ) : SV_Target
{
	float4 ret = (float4)0;
	float2 offset = float2(CameraPos.x, CameraPos.z);
	offset = offset * InverseTextureSize;
	ret = SampleVirtualTexture(MyTexture, input.uv, offset, 0.5);
	return ret;
}

technique10 Render
{
	pass P1
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS_Pos_Normal() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Final() ) );

	}
}
