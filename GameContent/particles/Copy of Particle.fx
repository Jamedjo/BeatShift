float4x4 View;
float4x4 Projection;
#define worldUp cross(half3(-1,0,0),half3(0,0,-1))

Texture theTexture;
float4 particleColor;

float BillboardSize;
sampler ColoredTextureSampler = sampler_state
{
texture = <theTexture> ;
magfilter = LINEAR;
minfilter = LINEAR;
mipfilter= POINT;
AddressU = Clamp;
AddressV = Clamp;
};
struct VertexShaderInput
{
float4 Position : POSITION0;
float2 textureCoordinates : TEXCOORD0;
//float3 Normal : NORMAL0;
};
struct VertexShaderOutput
{
float4 Position : POSITION0;
float2 textureCoordinates : TEXCOORD0;
};
struct PixelShaderInput
{
float2 textureCoordinates : TEXCOORD0;
};
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
VertexShaderOutput output;
// Work out what direction we are viewing the billboard from.
    float3 viewDirection = View._m02_m12_m22;
	float3 rightVector = normalize(cross(viewDirection, input.Normal));
	float3 position = input.Position;

	    // Offset to the left or right.
    position += rightVector * (input.textureCoordinates.x - 0.5) * BillboardSize;
    
    // Offset upward if we are one of the top two vertices.
    position += input.Normal * (1 - input.textureCoordinates.y) * BillboardSize;

	float4 viewPosition = mul(float4(position, 1), View);
	output.Position = mul(viewPosition, Projection);
//output.Position = mul(input.Position, WorldViewProjection);
output.textureCoordinates = input.textureCoordinates;
return output;
}
float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
float4 color = tex2D( ColoredTextureSampler, input.textureCoordinates);
color.r = particleColor.r * color.a;
color.g = particleColor.g * color.a;
color.b = particleColor.b * color.a;
return color;
}
technique Technique1
{
pass Pass1
{
VertexShader = compile vs_2_0 VertexShaderFunction( );
PixelShader = compile ps_2_0 PixelShaderFunction( );
}
}