#include "structs.fxh"
#include "samplers.fxh"

float InverseMapSize = (1.0/1024.0);
float MapSize = 1024;
float InverseLosresMapSize = (1.0/1024.0);
float LoresMapSize = 1024;
float2 TerrainCentreLocation = float2(0,0);
Texture2D <float> HeightMap;
int ZoomLevel = 10;

SamplerState HeightSampler
{
	//Filter = ANISOTROPIC;
	Filter   = MIN_MAG_MIP_LINEAR;
//	Filter = MIN_MAG_MIP_POINT;
	AddressU = Border;
	AddressV = Border;
	AddressW = Border;
	BorderColor = float4( 0,0,0,0 );
};

float4 tex2Dlod_bilinear( float2 uv, float inverseMapSize, float mapSize )

{
	float it = inverseMapSize / 2.0;
	//float textureSize = exp2(11);
	//float texelSize = exp2(-11);
        //float4 height00 = tex2Dlod(texSam, uv);
        //float4 height10 = tex2Dlod(texSam, uv + float4(texelSize, 0, 0, 0));
        //float4 height01 = tex2Dlod(texSam, uv + float4(0, texelSize, 0, 0));
        //float4 height11 = tex2Dlod(texSam, uv + float4(texelSize , texelSize, 0, 0));
	float4 height00 = HeightMap.SampleLevel(HeightSampler, uv, 0);
	float4 height10 = HeightMap.SampleLevel(HeightSampler, uv+float2(it, 0), 0);
	float4 height01 = HeightMap.SampleLevel(HeightSampler, uv+float2(0,it), 0);
	float4 height11 = HeightMap.SampleLevel(HeightSampler, uv+float2(it,it), 0);

        float2 f = frac( uv.xy * mapSize*2.0 );

        float4 tA = lerp( height00, height10, f.x );

        float4 tB = lerp( height01, height11, f.x );

        return lerp( tA, tB, f.y );

}

PS_TEX VS( VS_POS input )
{
	PS_TEX output = (PS_TEX)0;
	float4 pos = float4(input.pos,1);
	float2 texcoord = float2(pos.x,pos.z);
	float yScale = World._m11;
	float xScale = World._m00;
	float inverseMapSize = InverseMapSize;
	float mapSize = MapSize;
	pos = mul ( pos, World);


	texcoord = float2(pos.x, pos.z) - TerrainCentreLocation;
	texcoord = texcoord * InverseMapSize + 0.5;
	pos.y = tex2Dlod_bilinear( texcoord, inverseMapSize, mapSize);

	pos.y = pos.y * yScale;
	output.pos = pos;
	output.uv = float2(pos.x,pos.y);
	return output;
}

[maxvertexcount(3)]
void GS( triangle PS_TEX input[3], inout TriangleStream<PS_NORM_TEX> TriStream )
{
    PS_NORM_TEX output = (PS_NORM_TEX)0;
    float3 faceEdgeA = input[1].pos.xyz - input[0].pos.xyz;
    float3 faceEdgeB = input[2].pos.xyz - input[0].pos.xyz;
    float3 faceNormal = normalize( cross(faceEdgeA, faceEdgeB) );
    for( int v=0; v<3; v++ )
    {
        output.pos = input[v].pos;
        output.pos = mul( output.pos, View );
        output.pos = mul( output.pos, Proj );
        
        output.norm = faceNormal;
        
        output.uv = input[v].uv;
        
        TriStream.Append( output );
    }
    TriStream.RestartStrip();
}


float4 PS ( PS_NORM_TEX input ) : SV_Target
{
	float col = input.uv.y * 0.2;
	float4 colour = float4(col,col,1,1);
	float ambient = 0.4;
	float3 direction = normalize(float3(1,1,1));
	float intensity = clamp(dot(input.norm, direction) + ambient,0,1);
	colour = colour * intensity;
	colour.w = 1;
	return colour;

}

technique10 Render
{

	pass ExTerrain
	{
		SetGeometryShader( CompileShader( gs_4_0, GS()) );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
		//ZEnable = true;
		//ZWriteEnable = true;		
		//CullMode=None; 
	}
}

