Shader "Business as Usual/Soft Town Cloud"
{
    Properties { _BaseColor("Cream", Color) = (.94,.91,.80,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 p:POSITION; float3 n:NORMAL; };
            struct V { float4 p:SV_POSITION; float3 n:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            half4 _TownCloudTint;
            V vert(A a) { V v; v.p=TransformObjectToHClip(a.p.xyz); v.n=TransformObjectToWorldNormal(a.n); return v; }
            half4 frag(V v):SV_Target { half shade=.76+.24*saturate(dot(normalize(v.n),normalize(float3(-.4,1,-.3)))); half3 tint=_TownCloudTint.a>0?_TownCloudTint.rgb:half3(1,1,1); return half4(_BaseColor.rgb*shade*tint,1); }
            ENDHLSL
        }
    }
}
