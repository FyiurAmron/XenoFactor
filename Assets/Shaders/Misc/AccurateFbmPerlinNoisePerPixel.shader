Shader "URP Custom/Accurate FBM Perlin Noise Per Pixel"
{
    Properties
    {
        [Header(General)]
        [IntRange] _iterations( "Iterations/octaves", Range( 0, 16 ) ) = 6
        _resultPowerExponent( "Result Power Exponent", Range( 0.1 , 5.0 ) ) = 2.0

        [Header(Input XY Precalc)]
        _offsetX( "Offset X", Float ) = 0.0
        _offsetY( "Offset Y", Float ) = 0.0

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
                // float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : WS_POSITION;
                // float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                float3 inPos = IN.positionOS.xyz;

                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(inPos);
                OUT.positionWS = TransformObjectToWorld(inPos);
                // OUT.uv = IN.uv;

                return OUT;
            }

            uint triple32(uint x)
            {
                x ^= x >> 17;
                x *= 0xed5ad4bbU;
                x ^= x >> 11;
                x *= 0xac4c1b51U;
                x ^= x >> 15;
                x *= 0x31848babU;
                x ^= x >> 14;
                return x;
            }

            float hash(float2 v2)
            {
                return float(triple32(triple32(asuint(v2.x)) + asuint(v2.y))) / float(0xffffffffU);
            }

            float noise(float2 x)
            {
                float2 i = floor(x);
                float2 f = frac(x);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                float2 t = f * f * (f * (f * 6.0 - 15.0) + 10.0); // fade fn

                float noise = lerp(lerp(a, b, t.x), lerp(c, d, t.x), t.y);

                return noise;
            }

                        float fbm(float2 p)
                        {
                            float value = _initialValue;
                            float freq = _initialFrequency;
                            float amp = _initialAmplitude;
            
                            p += float2(_offsetX, _offsetY);
            
                            for (int i = 0; i < _iterations; i++)
                            {
                                value += amp * noise(p * freq);
                                freq *= _frequencyMultiplierPerIteration;
                                amp *= _amplitudeMultiplierPerIteration;
                            }
            
                            return pow(clamp(value, -1.0, 1.0) * 0.5 + 0.5, _resultPowerExponent);
                        }

            half4 frag(Varyings IN) : SV_Target
            {
                float c = fbm(IN.positionWS.xz);

                float4 baseColor = (_1bit == 1)
                                       ? (c < _1bitThreshold)
                                             ? 0
                                             : 1
                                       : float4(c, c, c, 1);

                return baseColor * _colorMultiplicative + _colorAdditive;
            }

            //
            ENDHLSL
        }
    }
}