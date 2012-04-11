#include "States.fxh"
#include "Samplers.fxh"
#include "Structs.fxh"
#include "RoamTerrain.fx"

float TextureZoomLevel;

Texture2D <float4> Texture_0;

float4 GetPosition( float4 pos )
{
	float4 output = pos;
	output = mul ( output, World);
	output = mul ( output, View );
	output = mul ( output, Proj );
	return output;
}

PS_TEX VS( VS_POS_TEX input )
{
	PS_TEX output = (PS_TEX)0;
	
	output.pos = GetPosition(float4(input.pos, 1.0));
	output.uv  = input.uv;
	
	return output;
}


PS_NORM VS_Pos_Normal ( VS_POS_NORM input )
{
	PS_NORM output = (PS_NORM)0;
	
	output.pos = GetPosition(float4(input.pos, 1.0));
	output.norm  = input.norm;
	
	return output;
}

PS_TEX VS_Height_Texture( VS_POS_TEX input )
{
	PS_TEX output = (PS_TEX)0;

	float2 texCoord = float2(input.pos.x, input.pos.z);
	float4 pos = float4(input.pos,1.0);

	// This line should reference heighttexture...
	float4 displacement = (float4)0;
	displacement.y = -1.0;
	pos = pos + displacement;

	output.pos = GetPosition(pos);
	output.uv  = input.uv;
	
	return output;
}

PS_NORM VS_Pos_Only (VS_POS input )
{
	PS_NORM output = (PS_NORM)0;
	
	output.pos = GetPosition(float4(input.pos, 1.0));
	output.norm = float3(input.pos.x,input.pos.z,input.pos.y);
	
	return output;
}

float4 PS_Normal ( PS_NORM input ) : SV_Target
{
	float4 output = (float4)0;
	output.x = input.norm.x;
	output.y = input.norm.y;
	output.z = input.norm.z;
	output.w = 1;
	return output;
}


float4 PS_Final( PS_TEX input ) : SV_Target
{
	//float2 texCoord = float2(inColor.x,inColor.y);
	//float2 texCoord = input.uv;
	float left = input.uv.x;
	float top = 1-input.uv.y;

	float trueTop = 0;
	float trueLeft = 0;

	float trueWidth = 2.0/3.0;
	float trueHeight = 1;
	float i = 1;
	if(TextureZoomLevel > 0 && TextureZoomLevel <= 128)
	{
		for(i = 0; i < TextureZoomLevel; i++)
		{
			trueHeight = trueHeight / 2;
			trueWidth = trueWidth/2;
		}
	}
	if(TextureZoomLevel > 0)
	{
		trueLeft = 2.0/3.0;	
		trueTop = 1.0 -  (2.0*trueHeight);
	}
	top = (top)*trueHeight + trueTop;
	left = (left)*trueWidth + trueLeft;

	float2 texCoord = float2(left,top);

	float4 color = Texture_0.Sample(AnisoClampSampler,texCoord);


	return color;
}


technique10 Render
{

	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Final() ) );
		//SetDepthStencilState( EnableDepthTest, 0 );
		//SetRasterizerState( EnableMSAA );
	}

	pass P1
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS_Height_Texture() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Final() ) );
		//SetDepthStencilState( EnableDepthTest, 0 );
		//SetRasterizerState( EnableMSAA );
	}

	pass P2
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS_Pos_Normal() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Normal() ) );
		//SetDepthStencilState( EnableDepthTest, 0 );
		//SetRasterizerState( EnableMSAA );
	}

	pass Terrain
	{
		SetVertexShader( CompileShader( vs_4_0, VS_Terrain()));
		SetGeometryShader(CompileShader(gs_4_0, GS_Terrain()));
		SetPixelShader(CompileShader(ps_4_0, PS_Terrain()));
	}

	pass PosOnly
	{
		SetGeometryShader(0);
		SetVertexShader(CompileShader( vs_4_0, VS_Pos_Only() ) );
		SetPixelShader(CompileShader( ps_4_0, PS_Normal() ) );
	}
}
