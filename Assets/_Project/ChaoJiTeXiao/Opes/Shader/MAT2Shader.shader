Shader "Custom/MAT2Shader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _Mask("Mask",2D)= "white" {}
    }

    SubShader
    {
        Tags 
            { 
                "RenderType"="Transparent" 
                "Queue"="Transparent"
                "RenderPipeline"="UniversalPipeline" 
            }

            Pass
            {
                Blend SrcAlpha OneMinusSrcAlpha
                ZWrite Off
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 uv : TEXCOORD0;
                float4 uv1:TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 Custom:TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_Mask);
            SAMPLER(sampler_Mask);
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float4 _Mask_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv.xy, _BaseMap);
                OUT.Custom=float4(IN.uv.zw,IN.uv1.xy);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half Mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, IN.uv).r ;
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                color.a*=Mask;
                color.a=IN.Custom.x;
                return color;
            }
            ENDHLSL
        }
    }
}
