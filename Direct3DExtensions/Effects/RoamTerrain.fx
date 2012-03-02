#include "Structs.fxh"

PS_NORM_TEX VS_Terrain(VS_POS_TEX input)
{
	PS_NORM_TEX output = (PS_NORM_TEX)0;
	output.uv = input.uv;
	output.norm = float3(0,1,0);
	output.pos = mul(float4(input.pos,1), World);
	//output.pos = mul(output.pos, View);
	//output.pos = mul(output.pos, Proj);

	return output;
}

[maxvertexcount(12)]
void GS_Terrain( triangle PS_NORM_TEX input[3], inout TriangleStream<PS_NORM_TEX> TriStream )
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

float4 PS_Terrain(PS_NORM_TEX input) : SV_Target
{
	float4 colour = float4(0.5,0.5,1,1);
	float ambient = 0.2;
	float3 direction = normalize(float3(1,1,1));
	float intensity = dot(input.norm, direction) + ambient;
	colour = colour * intensity;
	colour.w = 1;
	return colour;
}