Shader "PostProcess/NoiseEffect"
{
    Properties
    {
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.3
        _NoiseFrameRate ("Noise Frame Rate", Range(1, 60)) = 30
    }
    
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _NoiseIntensity;
                float _NoiseFrameRate;
            CBUFFER_END

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            Varyings Vert(uint vertexID : SV_VertexID)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(vertexID);
                output.texcoord = GetFullScreenTriangleTexCoord(vertexID);
                return output;
            }

            float Random(float2 uv, float seed)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233)) + seed) * 43758.5453);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float3 color = SampleSceneColor(input.texcoord);
                
                float timeSeed = floor(_Time.y * _NoiseFrameRate);
                float noise = Random(input.texcoord, timeSeed);
                noise = (noise - 0.5) * 2.0 * _NoiseIntensity;
                
                color += noise;
                
                return float4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
