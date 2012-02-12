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

#include "Samplers.fxh"

float VirtualTextureSize;	// This size of the virtual texture
float PageTableSize;		// The size of the page table (VirtualTextureSize/PageSize)

float AtlasScale;			// This value is used to scale the uv to the texture atlas. It holds (PageSize/TextureAtlasSize)

float BorderScale;			// These values are used to adjust the uv for the borders
float BorderOffset;			// BorderScale = (PageSize-2*BorderSize)/PageSize
							// BorderOffset = BorderSize/PageSize
float MipBias;				// This is used during the prepass to adjust the page loading

Texture2D		PageTable;
Texture2D		TextureAtlas;

// This function estimates mipmap levels
float MipLevel( float2 uv, float size )
{
	float2 dx = ddx( uv * size );
    float2 dy = ddy( uv * size );
    float d = max( dot( dx, dx ), dot( dy, dy ) );

	return max( 0.5 * log2( d ), 0 );
}

// This function samples the page table and returns the page's 
// position and mip level. 
float3 SampleTable( float2 uv, float mip )
{
	const float2 offset = frac( uv * PageTableSize ) / PageTableSize;
	return PageTable.SampleLevel( PointClampSampler, uv - offset, mip ).xyz;
}

// This functions samples from the texture atlas and returns the final color
float4 SampleAtlas( float3 page, float2 uv )
{
	const float mipsize = exp2( floor( page.z * 255.0 + 0.5 ) );

	uv = frac( uv * PageTableSize / mipsize );

	uv *= BorderScale;
	uv += BorderOffset;

	const float2 offset = floor( page.xy * 255 + 0.5 );

	return TextureAtlas.Sample( LinearClampSampler, ( offset + uv ) * AtlasScale );
}

// Ugly brute force trilinear, look up twice and lerp
float4 VirtualTextureTrilinear( float2 uv )
{
	float miplevel = MipLevel( uv, VirtualTextureSize );
	miplevel = clamp( miplevel, 0, log2( PageTableSize )-1 );

	const float mip1     = floor( miplevel );
	const float mip2	 = mip1 + 1;
	const float mipfrac  = miplevel - mip1;

	const float3 page1 = SampleTable( uv, mip1 );
	const float3 page2 = SampleTable( uv, mip2 );

	const float4 sample1 = SampleAtlas( page1, uv );
	const float4 sample2 = SampleAtlas( page2, uv );

	return lerp( sample1, sample2, mipfrac );
}

// Simple bilinear
float4 VirtualTexture( float2 uv )
{
	float mip = floor( MipLevel( uv, VirtualTextureSize ) );
	mip = clamp( mip, 0, log2( PageTableSize ) );

	const float3 page = SampleTable( uv, mip );
	return SampleAtlas( page, uv );
}
