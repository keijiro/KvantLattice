//
// Line shader for Lattice
//
// Vertex format:
// position     = not in use
// texcoord0.xy = uv for position texture
// texcoord1.xy = uv for normal texture
//
Shader "Hidden/Kvant/Lattice/Line"
{
    Properties
    {
        _PositionBuffer ("-", 2D) = "black"{}
        [HDR] _Color    ("-", Color) = (1, 1, 1, 0.5)
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    #pragma multi_compile_fog

    struct v2f
    {
        float4 position : SV_POSITION;
        UNITY_FOG_COORDS(0)
    };

    sampler2D _PositionBuffer;
    half4 _Color;
    float2 _BufferOffset;

    v2f vert(appdata_base v)
    {
        float4 uv = float4(v.texcoord + _BufferOffset, 0, 0);
        v.vertex.xyz = tex2Dlod(_PositionBuffer, uv).xyz;

        v2f o;
        o.position = mul(UNITY_MATRIX_MVP, v.vertex);
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
        Tags { "Queue" = "Geometry+1" }
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
