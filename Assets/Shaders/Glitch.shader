Shader "Custom/Glitch"
{
    Properties
    {
        _Intensity ("Intensity", Range(0, 1)) = 0.3
        _BlockSize ("Block Size", Float) = 24
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        ZWrite Off Cull Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Intensity;
            float _BlockSize;

            float Hash(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 uv = i.texcoord;
                // re-roll the glitch ~12 times a second
                float t = floor(_Time.y * 12);

                // horizontal tearing: some rows of blocks get shoved sideways
                float row = floor(uv.y * _BlockSize);
                float r = Hash(float2(row, t));
                if (r < _Intensity * 0.5)
                    uv.x += (Hash(float2(row, t + 1)) - 0.5) * 0.1 * _Intensity;

                // RGB split
                float shift = 0.01 * _Intensity * (Hash(float2(t, 3)) - 0.5);
                half red   = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2(shift, 0)).r;
                half green = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).g;
                half blue  = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv - float2(shift, 0)).b;

                // scanlines
                half scan = 1 - 0.15 * _Intensity * step(0.5, frac(uv.y * _ScreenParams.y * 0.5));
                return half4(half3(red, green, blue) * scan, 1);
            }
            ENDHLSL
        }
    }
}
