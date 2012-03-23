#include "structs.fxh"
#include "samplers.fxh"

float InverseMapSize = (1.0/1024.0);
float MapSize = 1024;
float LoresInverseMapSize = (1.0/1024.0);
float LoresMapSize = 1024;
float2 TerrainCentreLocation = float2(0,0);
Texture2D <float> HiresTexture;
float2 HiresISize;
float2 HiresLocation;
Texture2D <float> LoresTexture;
float2 LoresISize;
float2 LoresLocation;
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


	texcoord = float2(pos.x, pos.z);
	texcoord = texcoord * HiresISize.x + 0.5;
	texcoord = texcoord - HiresLocation;

	loresTexcoord = float2(pos.x, pos.z);
	loresTexcoord = loresTexcoord * LoresISize.x * 0.1 + 0.5;
	loresTexcoord = loresTexcoord - LoresLocation;

	//hires = tex2Dlod_bilinear(HiresMap, texcoord - HiresLocation, InverseMapSize, mapSize);
	//lores = tex2Dlod_bilinear(LoresMap, loresTexcoord, LoresInverseMapSize, LoresMapSize);
	hires = tex2Dlod(HiresTexture, texcoord+HiresLocation);
	lores = tex2Dlod(LoresTexture, loresTexcoord+LoresLocation);

	if(texcoord.x < inv || texcoord.x > 1-inv || texcoord.y < inv || texcoord.y > 1-inv)
		pos.y = lores;
	else
		pos.y = hires;

	// These lines for testing...
	if(ZoomLevel == 1)
		pos.y = lores;
	else if(ZoomLevel == 2)
		pos.y = hires;

	output.uv = float3(pos.x,pos.y, pos.z);
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

struct PS_OUTPUT
{
	float4 colour : SV_TARGET0;
	float4 posX : SV_TARGET1;
	float4 posY : SV_TARGET2;
	float4 posZ : SV_TARGET3;
};

PS_OUTPUT PS ( PS_NORM_TEX input ) : SV_Target
{
	PS_OUTPUT output = (PS_OUTPUT)0;
	float h = input.uv.y;
	float4 water = float4(0.2,0.2,1,0.5);
	float4 sand = float4(0.7,0.7,0.2,1);
	float4 grass = float4(0.2,0.7,0.2,1);
	float4 rock = float4(0.7,0.5,0.4,1);
	float4 snow = float4(0.95,0.95,0.95,0.95);
	float4 colour;
	float3 direction;
	float col1, col2, ambient, intensity;

	output.posX = input.uv.x;
	output.posY = input.uv.y;
	output.posZ = input.uv.z;

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

	output.colour = colour;
	return output;

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

