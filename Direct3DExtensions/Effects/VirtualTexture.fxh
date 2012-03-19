
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

float4 VirtualTexture( float2 uv )
{
	float mip = floor( MipLevel( uv, VirtualTextureSize ) );
	mip = clamp( mip, 0, log2( PageTableSize ) );

	const float3 page = SampleTable( uv, mip );
	return SampleAtlas( page, uv );
}
