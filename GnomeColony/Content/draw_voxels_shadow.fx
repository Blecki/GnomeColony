float4x4 World;
float4x4 View;
float4x4 Projection;

texture Texture;
sampler diffuseSampler = sampler_state
{
    Texture = (Texture);
    MAGFILTER = POINT;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
	float3 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	float3 Tangent : TEXCOORD1;
	float3 Binormal : TEXCOORD2;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float2 Depth : TEXCOORD1;
};

struct PixelShaderOutput
{
    float4 Depth : COLOR0;
};

VertexShaderOutput ClearVS(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position, 1);
	return output;
}

PixelShaderOutput ClearPS(VertexShaderOutput input)
{
	PixelShaderOutput output;
	output.Depth = 0.0f;
	return output;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(float4(input.Position,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.TexCoord = input.TexCoord;
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;

    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;
	float4 color = tex2D(diffuseSampler, input.TexCoord);
	clip(color.a < 0.5 ? -1 : 1);
	output.Depth = 1.0f - (input.Depth.x / input.Depth.y);
    return output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

technique Technique2
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 ClearVS();
		PixelShader = compile ps_4_0 ClearPS();
	}
}
