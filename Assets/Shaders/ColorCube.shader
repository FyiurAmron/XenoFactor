Shader "Custom URP/Color Cube"
{
    Properties {}

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : WS_POSITION;
            };

            Varyings vert( Attributes IN )
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS);
                return OUT;
            }

            half4 frag( Varyings IN ) : SV_Target
            {
                // float3 wave = 0.5 * ( sin( IN.positionWS * 0.1 ) + 1 );
                return half4( IN.positionWS.xyz, 1 );
            }
            
            ENDHLSL
        }
    }
}