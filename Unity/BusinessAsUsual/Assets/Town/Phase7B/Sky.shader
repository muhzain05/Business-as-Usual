Shader "Business as Usual/Town Sky"
{
    Properties { _Daylight("Daylight", Range(0,1)) = 1 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 p:POSITION; };
            struct V { float4 p:SV_POSITION; float3 direction:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
            half _Daylight;
            CBUFFER_END
            V vert(A a) { V v; v.p=TransformObjectToHClip(a.p.xyz); v.direction=a.p.xyz; return v; }
            half4 frag(V v):SV_Target
            {
                half height=saturate(normalize(v.direction).y);
                half horizon=pow(height,.55);
                half3 day=lerp(half3(.72,.81,.78),half3(.32,.56,.76),horizon);
                half3 night=lerp(half3(.14,.20,.29),half3(.045,.085,.16),horizon);
                return half4(lerp(night,day,_Daylight),1);
            }
            ENDHLSL
        }
    }
}
