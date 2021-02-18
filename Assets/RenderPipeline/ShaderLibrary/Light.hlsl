#ifndef ZRP_LIGHT_INCLUDED
#define ZRP_LIGHT_INCLUDED

struct Light {
	float3 color;
	float3 direction;
};

Light GetDirectionalLight() {
	Light light;
	light.color = _DirectionalLightColor;
	light.direction = _DirectionalLightDirection;
	return light;
}

#endif