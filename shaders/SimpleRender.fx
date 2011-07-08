    float4x4 WorldViewProj : WorldViewProjection;
    float4 Opacity : Opacity = (0,1,1,1);

    struct VS_IN
    {
		float4 pos : POSITION;
		float4 col : COLOR0;
    };
     
    struct PS_IN
    {
		float4 pos : SV_POSITION;
		float4 col : COLOR0;
    };
     
    PS_IN VS(VS_IN input)
    {
		PS_IN output = (PS_IN)0;
		output.pos = input.pos;
		output.pos = mul(output.pos, WorldViewProj);
		output.col = input.col;
     
		return output;
    }
     
    float4 PS( PS_IN input ) : SV_Target
    {
		return input.col * Opacity;
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