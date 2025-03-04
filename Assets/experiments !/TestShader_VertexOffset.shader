Shader "Unlit/TestShader_VertexOffset"
{
    // a note:
    // for possible functions n such, https://developer.download.nvidia.com/cg/index_stdlib.html
    // this is what we can use

    Properties
    {
        _Value ("Value", Float) = 1.0
        _Color ("Color", Color) = (1,1,1,1)
        _ColorA ("ColorA", Color) = (1,1,1,1)
        _ColorB ("ColorB", Color) = (1,1,1,1)
        _ColorStart ("Color Start", Range(0, 1)) = 0
        _ColorEnd ("Color End", Range(0, 1)) = 1

        _ZigZag ("Zig Zag Strength", Range(0, 0.5)) = 0.05
        _YScale ("Y Scale", Float) = 8
        _XScale ("X Scale", Float) = 8
        _LineBlur ("Line Blur", Float) = 0.5
        _LineIntensity ("Line Intensity", Float) = 0.5

        _WaveAmp ("Wave Amplitude", Range(0, 0.2)) = 0.1
    }
    SubShader
    {
        Tags { 
            "RenderType"="Opaque"
            "Queue"="Geometry"
            }

        Pass
        {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define TAU 6.28318530718

            float4 _Color;
            float4 _ColorA;
            float4 _ColorB;
            float _ColorStart;
            float _ColorEnd;
            float _ZigZag;
            float _YScale;
            float _XScale;
            float _LineBlur;
            float _LineIntensity;
            float _WaveAmp;

            struct MeshData
            {
                float4 vertex : POSITION;
                float3 normals : NORMAL;
                float2 uv0 : TEXCOORD0;
            };
        

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1; 
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;

                float wave = cos( (v.uv0.y - (_Time.y * 0.1)) * TAU * _XScale);
                float wave2 = cos( (v.uv0.x - (_Time.y * 0.1)) * TAU * _XScale);


                v.vertex.y = wave * wave2 * _WaveAmp;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv0;
                o.normal = v.normals;
                return o;
            }

            float InverseLerp(float a, float b, float v) {
                return (v-a)/(b-a);
            }


            float4 frag (Interpolators i) : SV_Target
            {

                float t = cos( (i.uv.y - (_Time.y * 0.1)) * TAU * _XScale) * _LineBlur + _LineIntensity;

                return t;

                // the abs() part  gets rid of the top parts -- basically means we dont want anything with a normal that faces up or down
                float topBottomRemover = (abs(i.normal.y) < 0.999);
                float waves = t * topBottomRemover;
                float4 gradient = lerp(_ColorA, _ColorB, i.uv.y);
                return gradient * waves;
            }
            ENDCG
        }
    }
}
