#include "States.fxh"
#include "Samplers.fxh"

float TextureZoomLevel;
Texture2D <float4> Texture_0;

cbuffer PerFrame
{
	float4x4 WorldViewProj;
};

struct VS_IN
{
	float3 pos : POSITION;
	float2 uv  : TEXCOORD;
};

struct PS_IN
{
	float4 pos   : SV_POSITION;
	float2 uv    : TEXCOORD;
};

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = mul( float4( input.pos, 1.0 ), WorldViewProj );
	output.uv  = input.uv;
	
	return output;
}

// The most basic virtual texture pixel shader
float4 PS_Final( PS_IN input ) : SV_Target
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

	float4 color = Texture_0.Sample(AnisoClampSampler,texCoord, 0 );


	return color;
}


technique10 Render
{

	pass P1
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Final() ) );

		//SetBlendState( EnableAlphaToCoverage, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		SetDepthStencilState( EnableDepthTest, 0 );
		SetRasterizerState( EnableMSAA );
	}
}
