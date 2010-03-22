// Line.fx
// Part of "Microbe Patrol" Version 1.0 -- January 15, 2007
// Copyright 2007 Michael Anderson


uniform extern float4x4 worldViewProj : WORLDVIEWPROJECTION;
float time;
float length;
float radius;
float4 lineColor;
float blurThreshold; // This really should vary based on line's screenspace size


struct VS_OUTPUT
{
    float4 position : POSITION;
    float3 polar : TEXCOORD0;
    float2 col : TEXCOORD1;
};


VS_OUTPUT MyVS(
    float4 pos  : POSITION, 
    float3 norm : NORMAL, 
    float2 tex : TEXCOORD0 )
{
    VS_OUTPUT Out = (VS_OUTPUT)0;
    
    // Scale X by radius, and translate X by length, in worldspace
    // based on what part of the line we're on
	pos.x *= (tex.x * radius);
	pos.x += (tex.y * length);
		
	// Always scale Y by radius regardless on what part of the line we're on
	pos.y *= radius;

	Out.position = mul(pos, worldViewProj);
	
	Out.polar = norm;

	// Send "modelspace" (adjusted for line length and radius) coords to shader
    Out.col.x = pos.x;
    Out.col.y = pos.y;

    return Out;
}


// Helper function used by several pixel shaders to blur the line edges
float BlurEdge( float rho )
{
	if( rho < blurThreshold )
	{
		return 1.0f;
	}
	else
	{
		float normrho = (rho - blurThreshold) * 1 / (1 - blurThreshold);
		return 1 - normrho;
	}
}


float4 MyPSStandard( float3 polar : TEXCOORD0, float2 modelSpace: TEXCOORD1 ) : COLOR0
{
	float4 finalColor;
	finalColor.rgb = lineColor.rgb;
	finalColor.a = lineColor.a * BlurEdge( polar.x );
	return finalColor;
}


float4 MyPSAnimatedRadial( float3 polar : TEXCOORD0, float2 modelSpace: TEXCOORD1 ) : COLOR0
{
	float4 finalColor;
	float modulation = sin( ( -polar.x * 0.1 + time * 0.05 ) * 20 * 3.14159) * 0.5 + 0.5;
	finalColor.rgb = lineColor.rgb * modulation;
	finalColor.a = lineColor.a * BlurEdge( polar.x );
	return finalColor;
}


technique Standard
{
    pass P0
    {
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		BlendOp = Add;
        vertexShader = compile vs_1_1 MyVS();
        pixelShader = compile ps_2_0 MyPSStandard();
    }
}


technique AnimatedRadial
{
    pass P0
    {
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		BlendOp = Add;
        vertexShader = compile vs_1_1 MyVS();
        pixelShader = compile ps_2_0 MyPSAnimatedRadial();
    }
}
