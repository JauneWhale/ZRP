#ifndef ZRP_META_PASS_INCLUDED
#define ZRP_META_PASS_INCLUDED

#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Shadows.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"

bool4 unity_MetaFragmentControl;
float unity_OneOverOutputBoost;
float unity_MaxOutputValue;

struct Attributes {
	float3 positionOS : POSITION;
	float2 baseUV : TEXCOORD0;
	float2 lightMapUV : TEXCOORD1;
};

struct Varyings {
	float4 positionCS : SV_POSITION;
	float2 baseUV : VAR_BASE_UV;
};

Varyings MetaPassVertex (Attributes input) {
	Varyings output;
	// Pass the light map uv to fragment in this way
	input.positionOS.xy =
		input.lightMapUV * unity_LightmapST.xy + unity_LightmapST.zw;
	// OpenGL doesn't work unless it explicitly uses the Z coordinate.
	input.positionOS.z = input.positionOS.z > 0.0 ? FLT_MIN : 0.0;
	output.positionCS = TransformWorldToHClip(input.positionOS);
	output.baseUV = TransformBaseUV(input.baseUV);
	return output;
}

float4 MetaPassFragment (Varyings input) : SV_TARGET {
	float4 base = GetBase(input.baseUV);
	Surface surface;
	ZERO_INITIALIZE(Surface, surface);
	surface.color = base.rgb;
	surface.metallic = GetMetallic(input.baseUV);
	surface.smoothness = GetSmoothness(input.baseUV);
	BRDF brdf = GetBRDF(surface);
	float4 meta = 0.0;
	// diffuse reflection
	if (unity_MetaFragmentControl.x) {
		meta = float4(brdf.diffuse, 1.0);
		// highly specular but rough materials also pass along some indirect light
		meta.rgb += brdf.specular * brdf.roughness * 0.5;
		// raising it to a power provided via unity_OneOverOutputBoost with the PositivePow method
		// and then limited it to unity_MaxOutputValue.
		meta.rgb = min(
			PositivePow(meta.rgb, unity_OneOverOutputBoost), unity_MaxOutputValue
		);
	}
	// emissive light
	else if (unity_MetaFragmentControl.y) {
		meta = float4(GetEmission(input.baseUV), 1.0);
	}
	return meta;
}


#endif