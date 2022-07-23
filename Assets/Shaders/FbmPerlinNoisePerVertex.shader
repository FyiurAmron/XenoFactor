Shader "URP Custom/FBM Perlin Noise Per Vertex"
{
    Properties
    {
        [Header(General)]
        [IntRange] _iterations( "Iterations/octaves", Range( 0, 16 ) ) = 6
        _resultPowerExponent( "Result Power Exponent", Range( 0.1 , 5.0 ) ) = 2.0

        [Header(Input XY Precalc)]
        _offsetX( "Offset X", Float ) = 0.0
        _offsetY( "Offset Y", Float ) = 0.0
        _scale( "Scale", Float ) = 1.0

        [Header(Initial values)]
        _initialValue( "Value", Range( -2.0 , 2.0 ) ) = 0.0
        _initialAmplitude( "Amplitude", Range( 0.0 , 5.0 ) ) = 1.5
        _initialFrequency( "Frequency", Range( 0.0 , 6.0 ) ) = 2.0

        [Header(Per iteration multipliers)]
        _amplitudeMultiplierPerIteration( "Amplitude", Range( 0.0 , 1.0 ) ) = 0.5
        _frequencyMultiplierPerIteration( "Frequency", Range( 1.0 , 5.0 ) ) = 2

        [Header(1 bit)]
        [Toggle] _1bit( "1-bit", Range( 0, 1 ) ) = 0
        _1bitThreshold( "1-bit Threshold", Range( 0.0 , 1.0 ) ) = 0.3
        
        [Header(Colors)]
        [HDR] [MainColor] _colorMultiplicative( "Color Multiplicative", Color ) = ( 1.0, 1.0, 1.0, 1.0 )
        [HDR] _colorAdditive( "Color Additive", Color ) = ( 0.0, 0.0, 0.0, 0.0 )
        
        [Space(30)] [Header(Other)] [Toggle] _( "Other options", Int ) = 0
    }

    Subshader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            //
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _iterations, _resultPowerExponent;
            float _offsetX, _offsetY, _scale;
            float _amplitudeMultiplierPerIteration, _frequencyMultiplierPerIteration;
            float _initialValue, _initialAmplitude, _initialFrequency;
            
            int _1bit;
            float _1bitThreshold;

            float4 _colorMultiplicative, _colorAdditive;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : WS_POSITION;
                float noise : NOISE;
            };

            float2 hash(float2 v2) // result in range [-1,1) in both X,Y
            {
                return -1.0 + 2.0 * frac(43758.5453123 * sin(
                    float2(dot(v2, float2(127.1, 311.7)), dot(v2, float2(269.5, 183.3)))
                ));
            }

            float fbmDot(float2 l, float2 r, float2 v)
            {
                return dot( hash( l + v ), r - v );
            }

            float fbm(float2 p) // range [0,1] in both X,Y
            {
                float value = _initialValue;
                float freq = _initialFrequency;
                float amp = _initialAmplitude;

                p *= _scale;
                p += float2(_offsetX, _offsetY);

                for (int i = 0; i < _iterations; i++)
                {
                    float2 l = floor(p * freq);
                    float2 r = frac(p * freq);

                    float2 t = r * r * r * (r * (r * 6.0 - 15.0) + 10.0);

                    float A = fbmDot( l, r, float2(0.0, 0.0) );
                    float B = fbmDot( l, r, float2(1.0, 0.0) );
                    float C = fbmDot( l, r, float2(0.0, 1.0) );
                    float D = fbmDot( l, r, float2(1.0, 1.0) );

                    float noise = lerp(lerp(A, B, t.x), lerp(C, D, t.x), t.y);

                    value += amp * noise;
                    freq *= _frequencyMultiplierPerIteration;
                    amp *= _amplitudeMultiplierPerIteration;
                }

                return pow(clamp(value, -1.0, 1.0) * 0.5 + 0.5, _resultPowerExponent);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                float3 posOS = IN.positionOS.xyz;

                float3 posWS = TransformObjectToWorld(posOS);
                OUT.noise = fbm(posWS.xz);
                posWS.y += OUT.noise - 0.5;
                OUT.positionHCS = TransformWorldToHClip(posWS);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float noise = IN.noise;

                float4 baseColor = (_1bit == 1)
                           ? (noise < _1bitThreshold)
                                 ? 0
                                 : 1
                           : float4(noise, noise, noise, 1);

                return baseColor * _colorMultiplicative + _colorAdditive;
            }
            //
            ENDHLSL
        }
    }
}