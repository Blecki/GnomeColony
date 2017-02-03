float4x4 World;
float4x4 View;
float4x4 Projection;
texture Texture;

sampler diffuseSampler = sampler_state
{
    Texture = (Texture);
    MAGFILTER = POINT;
    MINFILTER = POINT;
    MIPFILTER = POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};


struct TexturedVertexShaderInput
{
	float3 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 Texcoord : TEXCOORD0;
	float3 Tangent : TEXCOORD1;
	float3 BiNormal : TEXCOORD2;
};

struct TexturedVertexShaderOutput
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
};

struct CubeVSOutput
{
	float4 Position : POSITION0;
	float3 WorldPos : TEXCOORD0;
};


TexturedVertexShaderOutput TexturedVertexShaderFunction(TexturedVertexShaderInput input)
{
    TexturedVertexShaderOutput output;
    float4 worldPosition = mul(float4(input.Position, 1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Texcoord = input.Texcoord;

    return output;
}

CubeVSOutput CubeVS(TexturedVertexShaderInput input)
{
	CubeVSOutput output;
	float4 worldPosition = mul(float4(input.Position, 1), World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPos = input.Position;
	return output;
}

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

PixelShaderOutput PSTexturedColor(TexturedVertexShaderOutput input)
{
    PixelShaderOutput output;
	output.Color = saturate(tex2D(diffuseSampler, input.Texcoord));
    return output;
}

PixelShaderOutput PSCube(CubeVSOutput input)
{
	PixelShaderOutput output;
	output.Color = texCUBE(diffuseSampler, -normalize(input.WorldPos));
	return output;
}

technique DrawTextured
{
    pass Pass1
    {
		AlphaBlendEnable = false;
		BlendOp = Add;
		SrcBlend = One;
		DestBlend = InvSrcAlpha;
		
        VertexShader = compile vs_4_0 TexturedVertexShaderFunction();
        PixelShader = compile ps_4_0 PSTexturedColor();
    }
}

technique DrawCube
{
	pass Pass1
	{
		AlphaBlendEnable = false;
		BlendOp = Add;
		SrcBlend = One;
		DestBlend = InvSrcAlpha;

		VertexShader = compile vs_4_0 CubeVS();
		PixelShader = compile ps_4_0 PSCube();
	}
}

