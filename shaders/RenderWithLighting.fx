float4x4 ViewProj;
float4x4 World;
float4x4 LocalRotation;
float4 LightDir = (1,1,1,1);
float AmbientIntensity = 0.9f;
float DirectionalIntensity = 0.1f;

struct VS_IN
{
	float4 pos : POSITION;
	float4 norm : NORMAL;
	float4 col : COLOR;
};
 
struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 norm : NORMAL;
	float4 col : COLOR;
};


//
// Vertex Shader
//
PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	float4x4 posTrans = mul(World,ViewProj);
	output.pos = mul(input.pos,posTrans);
	output.col = input.col;
	output.norm = normalize(mul(input.norm,LocalRotation));
	
	//output.norm = input.norm;
	return output;
}
 
//
// Pixel Shader
//
float4 PS( PS_IN input ) : SV_Target
{
	// Added the +1 so that normals kind of behind the object also get some light.
	float dotProd = dot((float3)input.norm,(float3)LightDir)+1;
	float directionalIntensity = dotProd * DirectionalIntensity/2;
	
	//return AmbientIntensity;
	return saturate((AmbientIntensity + directionalIntensity)*input.col);
}
 
technique10 Render
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}