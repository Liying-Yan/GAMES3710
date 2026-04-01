Shader "Custom/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.02)) = 0.005
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "OutlineEffect" }

            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS     : POSITION;
                float3 normalOS       : NORMAL;
                float3 smoothNormalOS : TEXCOORD3;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float4 _OutlineColor;
            float  _OutlineWidth;

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 normalOS = dot(input.smoothNormalOS, input.smoothNormalOS) > 0.001
                    ? normalize(input.smoothNormalOS)
                    : normalize(input.normalOS);

                float4 posCS = TransformObjectToHClip(input.positionOS.xyz);

                float3 normalWS = TransformObjectToWorldNormal(normalOS);
                float3 normalVS = mul((float3x3)UNITY_MATRIX_V, normalWS);

                // Project through P so the direction matches the actual screen mapping
                float4 normalCS = mul(UNITY_MATRIX_P, float4(normalVS, 0.0));
                float2 dir = normalCS.xy;

                float dirLenSq = dot(dir, dir);
                if (dirLenSq > 1e-6)
                {
                    dir *= rsqrt(dirLenSq);

                    // Correct aspect ratio: shrink X so pixel width is uniform
                    float aspectInv = _ScreenParams.y / _ScreenParams.x;
                    posCS.xy += dir * _OutlineWidth * posCS.w * float2(aspectInv, 1.0);
                }

                output.positionCS = posCS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(_OutlineColor.rgb, 1);
            }
            ENDHLSL
        }
    }
}
