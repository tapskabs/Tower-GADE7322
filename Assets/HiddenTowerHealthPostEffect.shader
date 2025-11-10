Shader "Hidden/TowerHealthPostEffect"
{
    Properties
    {
        _TintColor ("Tint Color", Color) = (1, 0, 0, 1)
        _Intensity ("Intensity", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "TowerHealthPostEffect"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            float4 _TintColor;
            float _Intensity;

            float4 Frag(VaryingsDefault i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, i.texcoord);
                float3 tinted = lerp(col.rgb, _TintColor.rgb, _Intensity);
                return float4(tinted, col.a);
            }
            ENDHLSL
        }
    }
}
