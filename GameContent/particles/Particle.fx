//float4x4 WorldViewProjection;
float4x4 World;
//float3 worldUp;
Texture theTexture;
float4 particleColor;
float4x4 View;
float4x4 Projection;
//float4x4 xWorld;
//float3 xCamPos;
//float3 xAllowedRotDir;
float BillboardSize;

#define worldUp cross(half3(-1,0,0),half3(0,0,-1))//half3(0,1,0)//


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

    


	float3 finalPos = mul(input.Position,World);  //input.Position;//center;


	float3 eyeVector = View._m02_m12_m22;
    
    float3 sideVector = normalize(cross(eyeVector,worldUp));    
    float3 upVector = normalize(cross(sideVector,eyeVector));        
    
    finalPos  += (input.textureCoordinates.x-0.5f)*(sideVector)* BillboardSize;
    finalPos += (0.5f-input.textureCoordinates.y)*(upVector) * BillboardSize;
	
    float4 finalPosition4 = float4(finalPos, 1);
		
	float4x4 ViewProjection = mul (View, Projection);
	output.Position = mul(finalPosition4, ViewProjection);


    //half4 finalPos4 = half4(finalPos,1);    
    
    //output.Position = mul(finalPos4,WorldViewProjection);

output.textureCoordinates = input.textureCoordinates;
return output;
}
float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
float4 color = tex2D( ColoredTextureSampler, input.textureCoordinates);
color.r = particleColor.r * color.a;
color.g = particleColor.g * color.a;
color.b = particleColor.b * color.a;
color.a =0.4;
return color;
}
technique Technique1
{
pass Pass1
{
        ZEnable = false;  
        ZWriteEnable = false;  
        AlphaBlendEnable = true; 
		SrcBlend = SrcAlpha;
		DestBlend = One;
VertexShader = compile vs_2_0 VertexShaderFunction( );
PixelShader = compile ps_2_0 PixelShaderFunction( );
}
}