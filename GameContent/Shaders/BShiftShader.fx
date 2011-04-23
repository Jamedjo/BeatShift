float4x4 wvp_Mx;//WorldViewProjection matrix
float4x4 world_Mx;
float4x4 view_Mx;
//float3 ViewVector;

float4x4 wit_Mx;//WorldInverseTranspose matrix

float3 DiffuseLightDirection = float3(1, 2, 0);
float4 DiffuseColour = float4(1, 1, 1, 1);
float DiffuseIntensity = 1.0;

float4 ambientColour = float4(1, 1, 1, 1);
float ambientIntensity = 0.2;

float Shininess = 10;
float4 SpecularColour = float4(1, 1, 1, 0.2);    
float SpecularIntensity = 0.5;

float bumpMagnitude = 3.5;


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
    float3 Binormal : BINORMAL0;

	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;

	//float4 Colour : COLOR0;
	float3 Normal : TEXCOORD1;
    float3 Tangent : TEXCOORD2;
    float3 Binormal : TEXCOORD3;
	
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, wvp_Mx);

    output.Normal = normalize(mul(input.Normal, wit_Mx));
    output.Tangent = normalize(mul(input.Tangent, wit_Mx));
    output.Binormal = normalize(mul(input.Binormal, wit_Mx));


	//float4 normal = mul(input.Normal, wit_Mx);
	//float lightintensity = dot(normal, DiffuseLightDirection);
	//output.Colour = saturate(DiffuseColour * DiffuseIntensity *lightintensity);

	output.TexCoord = input.TexCoord;
    
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	//Bump in range -0.5 to 0.5 instead of 0-1
	float3 bump = bumpMagnitude * (tex2D(normalSampler,input.TexCoord)-(0.5,0.5,0.5));
	float3 newNormal = input.Normal + (bump.x * input.Tangent + bump.y * input.Binormal);//TODO: check this line
	newNormal = normalize(newNormal);

	//Diffuse lighting now calculated per pixel in pixel shader
	float3 n_light = normalize(DiffuseLightDirection);
	float lightintensity = dot(n_light, newNormal);
	if(lightintensity<0) lightintensity=0;

	// Specular using new normal: R = 2 * (N.L) * N – L
    float3 R = normalize(2 * lightintensity * newNormal - n_light);
	float3 viewvec = view_Mx._m13_m23_m33;
	float3 v = normalize(mul(normalize(viewvec), world_Mx));
	float specularValue = max(pow(dot(R, v), Shininess),0);
	float4 specular = SpecularIntensity * SpecularColour * specularValue * lightintensity;

    float4 textureColour = tex2D(textureSampler, input.TexCoord);
	textureColour.a= tex2D(alphaSampler, input.TexCoord).r;

    return saturate(lightintensity * textureColour + ambientColour * ambientIntensity + specular);
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
