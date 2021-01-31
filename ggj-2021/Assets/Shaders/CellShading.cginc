#include "UnityLightingCommon.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"

sampler2D _LightRamp; 

fixed NDotL(float3 worldNormal, float3 lightDir)
{
  // Calculate lighting 
  fixed nl = max(0, dot(normalize(worldNormal), -lightDir));
  return nl;
}

// Shadow atten == 0 == fully shadowed
fixed4 CalculateLighting(float3 worldNormal, float lightAtten, float shadowAtten, float extraLight = 0)
{
  // Lighting compnent from sky lights
  fixed nl0 = NDotL(worldNormal, -_WorldSpaceLightPos0);
  fixed3 nlRamp0 = tex2D(_LightRamp, float2(0.5, nl0 + extraLight)).rgb * lightAtten;

  // Apply shadows
  fixed3 darkestShadow = tex2D(_LightRamp, float2(0.5, 0.0)).rgb; 
  fixed shadowFactor = lerp(1.0, shadowAtten, 1);
  nlRamp0 = lerp(darkestShadow, nlRamp0, shadowFactor);

  // Sum up lighting with ambient and other factors
  fixed3 lighting = _LightColor0 * nlRamp0;
  lighting += ShadeSH9(half4(normalize(worldNormal), 1)) * 0.5;

  // Get base time tinted color
  fixed4 diffuse = lighting.rgbr;
  diffuse.a = 1;

  return diffuse;
}