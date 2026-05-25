Shader "Custom/ShaderMat1"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _Map("Map",2D) = "white" {}
        _Mask("Mask",2D) = "white" {}
        _UVStrength("UVStrength",Range(0,4))=1
        _Power("Power",Range(0,10))=1
        _A0("A0", Color) = (0, 0, 0, 1)
        _B1("B1", Color) = (1, 1, 1, 1)
        _Scale("Scale",Range(0,10))=1
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
            Cull Off
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
            TEXTURE2D(_Map);
            TEXTURE2D(_Mask);
            SAMPLER(sampler_BaseMap);
            SAMPLER(sampler_Map);
            SAMPLER(sampler_Mask);
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float4 _Map_ST;
                float4 _Mask_ST;
                float _UVStrength;
                float _Power;
                float4 _A0;
                float4 _B1;
                float _Scale;
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
                float2 Uv=IN.uv;
                Uv.y+=IN.Custom.x*_UVStrength;
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, Uv*_Scale) * _BaseColor;
                half3 Map = SAMPLE_TEXTURE2D(_Map, sampler_Map, Uv);
                float Mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, IN.uv).r;
                color.a*=Mask;
                color.a*=IN.Custom.y;
                
                color.rgb=lerp(_A0,_B1*_Power,color.rgb);

                
                //color.rgb*=Map;
   
                return color;
            }
            ENDHLSL
        }
    }
}
