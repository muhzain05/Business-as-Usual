Shader "BusinessAsUsual/Rescue Outline" {
 Properties { _Color("Work cue",Color)=(1,.75,.2,1) _Width("Width",Float)=.007 }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Geometry+1"}
 Pass { Cull Front ZWrite On ZTest LEqual
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 float4 _Color;float _Width;
 struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;};struct V {float4 positionCS:SV_POSITION;};
 V vert(A a){V v;float3 p=TransformObjectToWorld(a.positionOS.xyz);p+=TransformObjectToWorldNormal(a.normalOS)*_Width;v.positionCS=TransformWorldToHClip(p);return v;}float4 frag(V v):SV_Target{return _Color;}
 ENDHLSL
 } } }
