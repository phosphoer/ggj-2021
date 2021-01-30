Shader "Custom/CellShaded"
{
  Properties
  {
    _Color ("Color", Color) = (1,1,1,1)
    _MainTex ("Texture", 2D) = "white" {}
    _LightRamp ("Light Ramp", 2D) = "white" {}
  }
  SubShader
  {
    CGINCLUDE 
      #include "UnityCG.cginc"
      #include "UnityLightingCommon.cginc"
      #include "Lighting.cginc"
      #include "AutoLight.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        fixed4 normal : NORMAL;
        fixed4 color : COLOR;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float4 pos : SV_POSITION;
        fixed4 color : COLOR;
        float2 uv : TEXCOORD0;
        SHADOW_COORDS(1)
        fixed3 worldNormal : TEXCOORD2;
        UNITY_FOG_COORDS(3)
      };

      sampler2D _MainTex;
      float4 _Color;
      
      v2f vert (appdata v)
      {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        o.worldNormal = mul(unity_ObjectToWorld, fixed4(v.normal.xyz, 0));
        o.color = v.color;

        TRANSFER_SHADOW(o)
        UNITY_TRANSFER_FOG(o, o.pos);

        return o;
      }

      fixed4 frag (v2f i) : SV_Target
      {
        // Get base diffuse color
        fixed3 texColor = tex2D(_MainTex, i.uv).rgb;
        fixed3 diffuse = _Color.rgb * texColor;

        UNITY_APPLY_FOG(i.fogCoord, diffuse);

        return fixed4(diffuse, 1);
      }
    ENDCG 

    Pass
    {
      Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
      Stencil 
      {
        Ref 1
        Comp always
        Pass replace
      }

      ZWrite On 

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_fog			
      #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
      ENDCG
    }
  }

  Fallback "Diffuse"
}
