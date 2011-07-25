float4x4 wvp_Mx      :  WorldViewProjection     < string UIWidget = "None"; >;
float4x4 world_Mx    :  World                   < string UIWidget = "None"; >;
float4x4 viewInv_Mx  :  ViewInverse             < string UIWidget = "None"; >;
float4x4 wit_Mx      :  WorldInverseTranspose   < string UIWidget = "None"; >;


float3 DiffuseLightDirection : POSITION
<
    string UIName = "Diffuse Light Direction";
    string Object = "DirectionalLight";
    string Space = "World";
    int refID = 0;
>  = float3(1, 2, 0);

bool useAlphaMap
<
    string UIName = "Use a texture for alpha values?";
> = false;

float3 ambientColour = float3(1, 1, 1);
float ambientIntensity = 0.3;

float Shininess = 16;

float3 SpecularColour = float3(1, 1, 1); //Darkness doubles as SpecularIntensity

float bumpMagnitude = 1.6;

texture diffuseTex;
sampler2D textureSampler = sampler_state {
    Texture = (diffuseTex);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture alphaTex;
sampler2D alphaSampler = sampler_state {
    Texture = (alphaTex);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};


texture normalTex;
sampler2D normalSampler = sampler_state {
    Texture = (normalTex);
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
    //float3 Binormal : BINORMAL0;

	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;

	//float4 Colour : COLOR0;
	//float3 Normal : TEXCOORD1;
    //float3 Tangent : TEXCOORD2;
    //float3 Binormal : TEXCOORD3;
	
	float3 Light : TEXCOORD1;
	float3 View : TEXCOORD2;
	
	//float3 EyeVec :	TEXCOORD4;
	
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = mul(input.Position, wvp_Mx);
	float3 worldPosition 	= mul(input.Position, world_Mx);

	float3x3 worldToTangentSpace;
	worldToTangentSpace[0] = mul(input.Tangent,world_Mx);
    worldToTangentSpace[1] = mul(cross(input.Tangent,input.Normal),world_Mx);
    worldToTangentSpace[2] = mul(input.Normal,world_Mx);

	output.Light = mul(worldToTangentSpace,DiffuseLightDirection);
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
	float3 LightDir = normalize(lDir);
	float3 halfway = normalize(LightDir + normalize(View));
	float spec=saturate(dot(normal, halfway));
	return pow(spec,exponent);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	//Bump in range -1 to 1 from normal texture
	float3 normal = tex2D(normalSampler,input.TexCoord.xy).xyz * 2.0 - 1.0;
	//Use bumpMagnitude to scale bump effect
	normal = float3(normal.x * bumpMagnitude, normal.y * bumpMagnitude, normal.z);

	float3 lambert_1 = lambert(input.Light,normal);
	float3 specular_1 = specular(input.Light,normal,input.View,Shininess);


	//// Specular using new Phong: R = 2 * (N.L) * N � L
    //float3 R = normalize(2 * lightintensity * newNormal - n_light);
	//float3 EV = normalize(input.EyeVec);
	//float specularValue = max(pow(dot(R, EV), Shininess),0);
	
	
    float3 textureColour = tex2D(textureSampler, input.TexCoord).xyz;
	float3 lambertSum = lambert_1 * textureColour;
	float3 specularSum = SpecularColour.xyz * specular_1;

	float Ambient = ambientColour * textureColour * ambientIntensity;
	
	float3 colour = saturate(Ambient + lambertSum + specularSum);
	float4 outFloat = float4(colour,1.0);//outFloat.a =1;
	//float4(lambert_1,lambert_1,lambert_1,1.0);
	
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
