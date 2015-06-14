//
// Line shader for Lattice
//
// Vertex format:
// position     = not in use
// texcoord0.xy = uv for position texture
//
Shader "Hidden/Kvant/Lattice/Line"
{
    Properties
    {
        _PositionTex ("-", 2D)    = ""{}
        [HDR] _Color ("-", Color) = (1, 1, 1, 0.5)
        _UVOffset    ("-", Vector) = (0, 0, 0, 0)
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    #pragma multi_compile_fog

    struct v2f
    {
        float4 position : SV_POSITION;
        UNITY_FOG_COORDS(0)
    };

    sampler2D _PositionTex;
    half4 _Color;
    float2 _UVOffset;

    v2f vert(appdata_base v)
    {
        float4 uv = float4(v.texcoord + _UVOffset, 0, 0);

        float4 pos = v.vertex;
        pos.xyz += tex2Dlod(_PositionTex, uv).xyz;

        v2f o;
        o.position = mul(UNITY_MATRIX_MVP, pos);

        UNITY_TRANSFER_FOG(o, o.position);

        return o;
    }

    half4 frag(v2f i) : COLOR
    {
        half4 c = _Color;
        UNITY_APPLY_FOG(i.fogCoord, c);
        return c;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    } 
}
