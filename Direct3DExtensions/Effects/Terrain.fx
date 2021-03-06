#include "structs.fxh"
#include "samplers.fxh"

float InverseMapSize = (1.0/1024.0);
float MapSize = 1024;
float LoresInverseMapSize = (1.0/1024.0);
float LoresMapSize = 1024;
float2 TerrainCentreLocation = float2(0,0);
float2 LoresTerrainCentreLocation = float2(0,0);
Texture2D <float> HeightMap;
Texture2D <float> LoresMap;
int ZoomLevel = 10;

SamplerState HeightSampler
{
	//Filter = ANISOTROPIC;
	Filter   = MIN_MAG_MIP_LINEAR;
//	Filter = MIN_MAG_MIP_POINT;
	//AddressU = Border;
	//AddressV = Border;
	//AddressW = Border;
	AddressU = Wrap;
	AddressV = Wrap;
	//BorderColor = float4( 10,10,10,10 );
};


float tex2Dlod( Texture2D<float> hMap, float2 uv)
{
	float ret = hMap.SampleLevel(HeightSampler, uv, 0);
	return ret;
}

float4 tex2Dlod_bilinear( Texture2D<float> hMap, float2 uv, float inverseMapSize, float mapSize )

{
	float it = inverseMapSize * 0.5;
	float4 height00 = hMap.SampleLevel(HeightSampler, uv, 0);
	float4 height10 = hMap.SampleLevel(HeightSampler, uv+float2(it, 0), 0);
	float4 height01 = hMap.SampleLevel(HeightSampler, uv+float2(0,it), 0);
	float4 height11 = hMap.SampleLevel(HeightSampler, uv+float2(it,it), 0);

        float2 f = frac( uv.xy * mapSize*2.0 );

        float4 tA = lerp( height00, height10, f.x );

        float4 tB = lerp( height01, height11, f.x );

        return lerp( tA, tB, f.y );
}

PS_TEX VS( VS_POS input )
{
	PS_TEX output = (PS_TEX)0;
	float4 pos = float4(input.pos,1);
	float2 texcoord;
	float2 loresTexcoord;
	float yScale = World._m11;
	float xScale = World._m00;
	float mapSize = MapSize;
	float hires, lores;
	float inv = InverseMapSize;
	pos = mul ( pos, World);


	texcoord = float2(pos.x, pos.z) - TerrainCentreLocation;
	texcoord = texcoord * InverseMapSize + 0.5;
	loresTexcoord = float2(pos.x, pos.z) - LoresTerrainCentreLocation;
	loresTexcoord = loresTexcoord * LoresInverseMapSize + 0.5;
	//hires = tex2Dlod_bilinear(HeightMap, texcoord, InverseMapSize, mapSize);
	//lores = tex2Dlod_bilinear(LoresMap, loresTexcoord, LoresInverseMapSize, LoresMapSize);
	hires = tex2Dlod(HeightMap, texcoord);
	lores = tex2Dlod(LoresMap, loresTexcoord);

	if(texcoord.x < inv || texcoord.x > 1-inv || texcoord.y < inv || texcoord.y > 1-inv)
		pos.y = lores;
	else
		pos.y = hires;

	// These lines for testing...
	if(ZoomLevel == 1)
		pos.y = lores;
	else if(ZoomLevel == 2)
		pos.y = hires;

	output.uv = float2(pos.x,pos.y);
	pos.y = pos.y * yScale;
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
	float h = input.uv.y;
	float4 water = float4(0.2,0.2,1,0.5);
	float4 sand = float4(0.7,0.7,0.2,1);
	float4 grass = float4(0.2,0.7,0.2,1);
	float4 rock = float4(0.7,0.5,0.4,1);
	float4 snow = float4(0.95,0.95,0.95,0.95);
	float4 colour;
	float3 direction;
	float col1, col2, ambient, intensity;
	col1 = clamp(h * 0.1, 0, 1);
	col2 = clamp(h * 0.1, 0.5, 1);
	colour = float4(col1,col2,col1,1);
	if(h < 5)
	{
		h = h * 0.2;
		colour = sand*h + water*(1-h);
	}
	else if(h < 25)
	{
		h = (h-5)*0.5;
		colour = grass*h + sand*(1-h);
	}
	else if(h < 525)
	{
		h = (h-25) * 0.002;
		colour = rock*h + grass*(1-h);
	}
	else
	{
		h = (h-525) * 0.001;
		colour = snow*h + rock*(1-h);
	}
	colour = clamp(colour,0,1);
	ambient = 0.0;
	direction = normalize(float3(1,1,1));
	intensity = dot(input.norm, direction) * 0.5 + 0.5;
	intensity = clamp(intensity + ambient,0,1);
	
	colour.xyz = colour.xyz * intensity;
	//colour.w = 1;
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

