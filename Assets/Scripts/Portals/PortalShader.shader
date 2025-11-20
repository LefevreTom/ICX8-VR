Shader "Unlit/PortalTextureURP" {
    Properties {
        _MainTex ("Texture", 2D) = "black" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass {
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP include file
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert (Attributes IN) {
                Varyings OUT;

                // FIX: use float3 so no truncation warning
                float3 posOS = IN.positionOS.xyz;
                OUT.positionHCS = TransformObjectToHClip(posOS);

                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
            }

            ENDHLSL
        }
    }
}
