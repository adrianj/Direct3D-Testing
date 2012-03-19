#include "Structs.fxh"
#include "samplers.fxh"

Texture2D <float> tex;

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


// The most basic virtual texture pixel shader
float4 PS_Final( PS_TEX input ) : SV_Target
{
	float4 ret = (float4)0;
	float2 offset = float2(CameraPos.x, CameraPos.z);
	float2 uv = input.uv - offset * 0.03125;
	float col = tex.Sample(LinearWrapSampler, uv);
	ret = float4(col,col,col,1);
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
