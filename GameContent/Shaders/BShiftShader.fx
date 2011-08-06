float4x4 wvp_Mx      :  WorldViewProjection     < string UIWidget = "None"; >;
float4x4 world_Mx    :  World                   < string UIWidget = "None"; >;
float4x4 viewInv_Mx  :  ViewInverse             < string UIWidget = "None"; >;
//float4x4 wit_Mx      :  WorldInverseTranspose   < string UIWidget = "None"; >;

// Maximum number of bone matrices we can render using shader 2.0 in a single pass.
// If you change this, update SkinnedModelProcessor.cs to match.
#define MaxBones 59
float4x4 Bones[MaxBones];


float3 LightDirection_0 : DIRECTION
<
    string UIName = "Light 0 Direction";
    int refID = 0;
>  = float3(-0.52,-0.57,-0.62);
float3 LightColour_0 = float3(0.586, 0.563, 0.474);

float3 LightDirection_1 : DIRECTION
<
    string UIName = "Light 1 Direction";
    int refID = 1;
>  = float3(0.71,0.34,0.60);
float3 LightColour_1 = float3(0.708,0.604,0.627);

float3 LightDirection_2 : DIRECTION
<
    string UIName = "Light 2 Direction";
    int refID = 2;
>  = float3(0.45, -0.76, 0.45);
float3 LightColour_2 = float3(0.538,0.605,0.655);

bool useAlphaMap
<
    string UIName = "Use a texture for alpha values?";
> = false;

bool useSpecular = true;
bool useAmbient = true;
bool useLambert= true;
bool drawNormals= false;

float3 ambientColour = float3(0.231, 0.225, 0.172);

float Shininess = 20;

float3 SpecularColour = float3(0.7, 0.7, 0.7); //Darkness doubles as SpecularIntensity

float bumpMagnitude = 0.43;

float reflectivity = 0.0f;
float reflectOverride = 0.0f;

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


texture2D normalTex;
sampler2D normalSampler = sampler_state {
    Texture = (normalTex);
    MagFilter = ANISOTROPIC;
    MinFilter = ANISOTROPIC;
    AddressU = Wrap;
    AddressV = Wrap;
};


texture2D reflectionTexture; 
samplerCUBE reflectionSampler = sampler_state { 
   texture = <reflectionTexture>; 
   magfilter = LINEAR; 
   minfilter = LINEAR; 
   mipfilter = LINEAR; 
   AddressU = Mirror; 
   AddressV = Mirror; 
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
	
	float3 Light0 : TEXCOORD1;
	float3 Light1 : TEXCOORD2;
	float3 Light2 : TEXCOORD3;
	float3 View : TEXCOORD4;	
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
	
	
    // Blend between the weighted bone matrices.
    float4x4 skinTransform = 0;
    skinTransform += Bones[input.BoneIndices.x] * input.BoneWeights.x;
    skinTransform += Bones[input.BoneIndices.y] * input.BoneWeights.y;
    skinTransform += Bones[input.BoneIndices.z] * input.BoneWeights.z;
    skinTransform += Bones[input.BoneIndices.w] * input.BoneWeights.w;
    
    // Skin the vertex position, normal ant tangent.
    float4 nPosition = mul(input.Position, skinTransform);
	float4 nNormal = mul(input.Normal, skinTransform);
	float4 nTangent = mul(input.Tangent, skinTransform);
    output.Position = mul(nPosition, wvp_Mx);
	float3 worldPosition 	= mul(nPosition, world_Mx);

	float3x3 worldToTangentSpace;
	worldToTangentSpace[0] = mul(nTangent,world_Mx);
    worldToTangentSpace[1] = mul(cross(nTangent,nNormal),world_Mx);
    worldToTangentSpace[2] = mul(nNormal,world_Mx);

	output.Light0 = mul(worldToTangentSpace,-LightDirection_0);
	output.Light1 = mul(worldToTangentSpace,-LightDirection_1);
	output.Light2 = mul(worldToTangentSpace,-LightDirection_2);
	
	//float3 halfAngle = normalize(DiffuseLightDirection) + normalize( viewInv_Mx[3].xyz - worldPosition.xyz );
	//output.EyeVec = viewInv_Mx[3].xyz - worldPosition;
	//output.halfAngle = mul(halfAngle, worldToTangentSpace);
	output.View = mul(worldToTangentSpace, viewInv_Mx[3].xyz - worldPosition);

	
    //output.Normal = normalize(mul(input.Normal, wit_Mx));
    //output.Tangent = normalize(mul(input.Tangent, wit_Mx));
    //output.Binormal = normalize(mul(input.Binormal, wit_Mx));


	output.TexCoord = input.TexCoord;
    
	return output;
}

float3 lambert(float3 lDir,float3 normal){
	//Diffuse lighting now calculated per pixel in pixel shader
	float3 LightDir = normalize(lDir);
	return saturate(dot(normal,LightDir));
}
	
float3 specular(float3 lDir,float3 normal,float3 View,float exponent){
	//Specular using Blinn-Phong = Ks * exp(N.H, a) * lightintensity
	float3 halfway = normalize(normalize(lDir) + View);
	float spec=saturate(dot(normal, halfway));
	return pow(spec,exponent);
}

struct BasicInfo
{
	float3 normal;
	float3 nView;
	
    float3 textureColour;
	float3 lambertSum;
	float3 specularSum;
};

BasicInfo BasePixelShaderFunction(VertexShaderOutput input)
{
	BasicInfo b;
	
	//Bump in range -1 to 1 from normal texture
	b.normal = 2.0 * tex2D(normalSampler,input.TexCoord).xyz - 1.0;//float3(0.5,1,0.1);//
	//Use bumpMagnitude to scale bump effect
	//float3 specularNormal = normal * bumpMagnitude;
	//normal = normalize(normal);
	b.normal = normalize(float3(b.normal.x * bumpMagnitude, b.normal.y * bumpMagnitude, b.normal.z));

	b.nView = normalize(input.View);

	float3 lambert_0 = lambert(input.Light0,b.normal);
	float3 specular_0 = specular(input.Light0,b.normal,b.nView,Shininess);
	float3 lambert_1 = lambert(input.Light1,b.normal);
	float3 specular_1 = specular(input.Light1,b.normal,b.nView,Shininess);
	float3 lambert_2 = lambert(input.Light2,b.normal);
	float3 specular_2 = specular(input.Light2,b.normal,b.nView,Shininess);

	
    b.textureColour = tex2D(textureSampler, input.TexCoord).xyz;
	b.lambertSum = lambert_0*LightColour_0+lambert_1*LightColour_1+lambert_2*LightColour_2;
	b.specularSum = SpecularColour.xyz * (specular_0+specular_1+specular_2);
	
	return b;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	BasicInfo b;
	b = BasePixelShaderFunction(input);
	
	float r = reflectivity+reflectOverride;
	if(r>1.0) r=1.0;
	if(r>0.0) {
	float3 reflectDir = reflect(-b.nView, b.normal);
	float3 reflectTex = texCUBE(reflectionSampler, normalize(reflectDir));
	b.textureColour = b.textureColour*(1-r)+r*reflectTex;//Value now between 0,0,0 and 2,2,2
	}
	
	float3 ambient = ambientColour;
	float3 baseColour = (b.lambertSum+ambient) * b.textureColour;
	float3 colour = saturate(baseColour + b.specularSum);
	float4 outFloat = float4(colour,1.0);
	
	if(useAlphaMap) outFloat.a= tex2D(alphaSampler, input.TexCoord).r;
	
    return outFloat;
}

float4 NoReflect_PS(VertexShaderOutput input) : COLOR0
{
	BasicInfo b;
	b = BasePixelShaderFunction(input);
	
	if(!useSpecular) b.specularSum = float3(0,0,0);
	if(!useLambert) b.lambertSum = float3(0,0,0);
	float3 ambient = float3(0,0,0);
	if(useAmbient) ambient = ambientColour;
	
	float3 baseColour = (b.lambertSum+ambient) * b.textureColour;
	
	float3 colour = saturate(baseColour + b.specularSum);
	if(drawNormals) colour = b.normal;
	float4 outFloat = float4(colour,1.0);
	
	if(useAlphaMap) outFloat.a= tex2D(alphaSampler, input.TexCoord).r;
	
    return outFloat;
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

technique NoReflect
{
    pass Pass1
    {
        AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 NoReflect_PS();
    }
}
