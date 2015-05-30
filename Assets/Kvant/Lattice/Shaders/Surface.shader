//
// Surface shader for Mountain
//
// Vertex format:
// position     = not in use
// texcoord0.xy = uv for position texture
// texcoord1.xy = uv for normal texture
//
Shader "Hidden/Kvant/Lattice/Surface"
{
    Properties
    {
        _PositionTex ("-", 2D)     = ""{}
        _NormalTex   ("-", 2D)     = ""{}
        _Color       ("-", Color)  = (1, 1, 1, 1)
        _PbrParams   ("-", Vector) = (0.9, 0.9, 0, 0) // (metalness, smoothness)
        _UVOffset    ("-", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Offset 1, 1
        
        CGPROGRAM

        #pragma surface surf Standard vertex:vert nolightmap addshadow

        sampler2D _PositionTex;
        sampler2D _NormalTex;
        half4 _Color;
        half2 _PbrParams;
        float2 _UVOffset;

        struct Input
        {
            half dummy;
        };

        void vert(inout appdata_full v)
        {
            float4 uv1 = float4(v.texcoord + _UVOffset, 0, 0);
            float4 uv2 = float4(v.texcoord1 + _UVOffset, 0, 0);
            v.vertex.xyz += tex2Dlod(_PositionTex, uv1).xyz;
            v.normal = tex2Dlod(_NormalTex, uv2).xyz;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color.rgb;
            o.Alpha = _Color.a;
            o.Metallic = _PbrParams.x;
            o.Smoothness = _PbrParams.y;
        }

        ENDCG
    } 
}
