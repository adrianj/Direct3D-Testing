/* 
MIT License

Copyright (c) 2009 Brad Blanchard (www.linedef.com)

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/

#include "States.fxh"
#include "VirtualTexture.fxh"

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
	float4 ret = float4(1,0,0,0);
	//return VirtualTexture( input.uv );			// Bilienar
	//ret = VirtualTextureTrilinear( input.uv );		// Trilienar
	ret = float4(input.uv, 1, 0);
	return ret;
}

// This is the pre-pass pixel shader.
// It returns the required page coordinates and mip level of the frame.
float4 PS_Feedback( PS_IN input ) : SV_Target
{
	float  mip  = floor( MipLevel( input.uv, VirtualTextureSize ) - MipBias );
	mip = clamp( mip, 0, log2( PageTableSize ) );

	const float2 offset = floor( input.uv * PageTableSize );
	return float4( float3( offset / exp2( mip ), mip ), 1.0 );
}

technique10 Render
{
	pass P1
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Feedback() ) );

		SetDepthStencilState( EnableDepthTest, 0 );
		SetRasterizerState( EnableMSAA );
	}

	pass P2
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Final() ) );

		//SetBlendState( EnableAlphaToCoverage, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		SetDepthStencilState( EnableDepthTest, 0 );
		SetRasterizerState( EnableMSAA );
	}
}
