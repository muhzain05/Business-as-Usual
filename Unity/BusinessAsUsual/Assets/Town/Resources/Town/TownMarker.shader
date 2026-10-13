Shader "Business as Usual/Town Marker"
{
    Properties { _MainTex ("Paper symbol", 2D) = "white" {} _Color ("Ink tint", Color) = (1,1,1,1) }
    SubShader {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest LEqual
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex); float4 _Color;
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            V Vert(A input){V o;o.positionCS=TransformObjectToHClip(input.positionOS.xyz);o.uv=input.uv;return o;}
            half4 Frag(V input):SV_Target{return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv)*_Color;}
            ENDHLSL
        }
    }
}
