float4x4 wvp_Mx      :  WorldViewProjection     < string UIWidget = "None"; >;
float4x4 world_Mx    :  World                   < string UIWidget = "None"; >;
float4x4 viewInv_Mx  :  ViewInverse             < string UIWidget = "None"; >;
//float4x4 wit_Mx      :  WorldInverseTranspose   < string UIWidget = "None"; >;

// Maximum number of bone matrices we can render using shader 2.0 in a single pass.
// If you change this, update SkinnedModelProcessor.cs to match.
#define MaxBones 59
float4x4 Bones[MaxBones];


bool useAlphaMap
<
    string UIName = "Use a texture for alpha values?";
> = false;

bool useSpecular = true;
bool useAmbient = true;
bool useLambert= true;
bool drawNormals= false;

float3 ambientColour = float3(1.0, 1.0, 1.0);

texture2D diffuseTex;
sampler2D textureSampler = sampler_state {
    Texture = (diffuseTex);
    MagFilter = ANISOTROPIC;
    MinFilter = ANISOTROPIC;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D alphaTex;
sampler2D alphaSampler = sampler_state {
    Texture = (alphaTex);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};


struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
    float3 Tangent : TANGENT0;
	float2 TexCoord : TEXCOORD0;
	
    float4 BoneIndices : BLENDINDICES0;
    float4 BoneWeights : BLENDWEIGHT0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VS_Part1_Output
{
	float4 nPosition;
	
	float2 TexCoord;
};

struct PS_Output
{
	float4 Colour : COLOR0;
	float4 Glow : COLOR1;
	
};

VertexShaderOutput VSF_Body(VS_Part1_Output input, float isSkinned)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
	
	output.Position = mul(input.nPosition, wvp_Mx);
	float3 worldPosition;
	//if(isSkinned==1) worldPosition = input.nPosition;
	//else
	worldPosition = mul(input.nPosition, world_Mx);

	output.TexCoord = input.TexCoord;
    
	return output;
}


VertexShaderOutput VertexShaderFunction(VertexShaderInput input){
	VS_Part1_Output output = (VS_Part1_Output)0;
	
	output.nPosition = input.Position;
	
	output.TexCoord = input.TexCoord;
	
	return VSF_Body(output,0);
}

VertexShaderOutput VertexShaderFunction_Skinned(VertexShaderInput input){
	VS_Part1_Output output = (VS_Part1_Output)0;
	
		// Blend between the weighted bone matrices.
		float4x4 skinTransform = 0;
		float xW =input.BoneWeights.x;
		float yW =input.BoneWeights.y;
		float zW =input.BoneWeights.z;
		float wW =input.BoneWeights.w;
		float xI = input.BoneIndices.x;
		float yI = input.BoneIndices.y;
		float zI = input.BoneIndices.z;
		float wI = input.BoneIndices.w;
		
		skinTransform += Bones[xI] * xW;
		skinTransform += Bones[yI] * yW;
		skinTransform += Bones[zI] * zW;
		skinTransform += Bones[wI] * wW;
		
		// Skin the vertex position, normal ant tangent.
		output.nPosition = mul(input.Position, skinTransform);
	
	output.TexCoord = input.TexCoord;

	return VSF_Body(output,1);
}

PS_Output PixelShaderFunction(VertexShaderOutput input)
{
	PS_Output output;

    float3 textureColour = tex2D(textureSampler, input.TexCoord).xyz;
	
	float3 ambient = float3(1.0,1.0,1.0);
	if(useAmbient) ambient = ambientColour;
	
	float3 baseColour = ambient * textureColour;
	
	float3 glow = saturate(baseColour);
	output.Glow= float4(glow,1.0);
	
	if(useAlphaMap) output.Glow.a= tex2D(alphaSampler, input.TexCoord).r;
	
	output.Colour = float4(0.0,0.0,0.0,0.0);

    return output;
}

technique Technique1
{
    pass Pass1
    {
        AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

technique SkinnedGlow
{
    pass Pass1
    {
        AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
        VertexShader = compile vs_2_0 VertexShaderFunction_Skinned();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
