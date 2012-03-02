#include "structs.fxh"
#include "samplers.fxh"

float InverseMapSize = 0.02;
Texture2D <float> HeightMap;


SamplerState HeightSampler
{
	Filter = ANISOTROPIC;
	//Filter   = MIN_MAG_MIP_LINEAR;
	AddressU = Border;
	AddressV = Border;
	AddressW = Border;
	BorderColor = float4( 0,0,0,0 );
};

PS_TEX VS( VS_POS input )
{
	PS_TEX output = (PS_TEX)0;
	float4 pos = float4(input.pos,1);
	float2 texcoord = (float2)0;
	float yScale = World._m11;
	pos = mul ( pos, World);
	output.uv = float2(pos.x, pos.z) * InverseMapSize + 0.5;
	pos.y = HeightMap.SampleLevel(HeightSampler, output.uv, 0) * yScale;
	output.pos = pos;
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

	float4 colour = float4(0.5,0.5,1,1);
	float ambient = 0.2;
	float3 direction = normalize(float3(1,1,1));
	float intensity = dot(input.norm, direction) + ambient;
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

