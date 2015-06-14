//
// GPGPU kernels for Lattice
//
Shader "Hidden/Kvant/Lattice/Kernels"
{
    Properties
    {
        _MainTex ("-", 2D) = ""{}
    }

    CGINCLUDE

    #pragma multi_compile DEPTH1 DEPTH2 DEPTH3 DEPTH4 DEPTH5
    #pragma multi_compile _ ENABLE_WARP

    #include "UnityCG.cginc"
    #include "ClassicNoise2D.cginc"

    sampler2D _MainTex;
    float2 _MainTex_TexelSize;

    float2 _Extent;
    float2 _Offset;
    float _Frequency;
    float3 _Amplitude;
    float2 _ClampRange;

    // Pass 0: Calculates vertex positions
    float4 frag_position(v2f_img i) : SV_Target 
    {
        float2 vp = (i.uv.xy - (float2)0.5) * _Extent;

        float2 nc1 = (vp + _Offset) * _Frequency;
    #if ENABLE_WARP
        float2 nc2 = nc1 + float2(124.343, 311.591);
        float2 nc3 = nc1 + float2(273.534, 178.392);
    #endif

        float2 np = float2(100000, 100000);

        float n1 = pnoise(nc1, np);
    #if ENABLE_WARP
        float n2 = pnoise(nc2, np);
        float n3 = pnoise(nc3, np);
    #endif

    #if DEPTH2 || DEPTH3 || DEPTH4 || DEPTH5
        n1 += pnoise(nc1 * 2, np * 2) * 0.5;
    #if ENABLE_WARP
        n2 += pnoise(nc2 * 2, np * 2) * 0.5;
        n3 += pnoise(nc3 * 2, np * 2) * 0.5;
    #endif
    #endif

    #if DEPTH3 || DEPTH4 || DEPTH5
        n1 += pnoise(nc1 * 4, np * 4) * 0.25;
    #if ENABLE_WARP
        n2 += pnoise(nc2 * 4, np * 4) * 0.25;
        n3 += pnoise(nc3 * 4, np * 4) * 0.25;
    #endif
    #endif

    #if DEPTH4 || DEPTH5
        n1 += pnoise(nc1 * 8, np * 8) * 0.125;
    #if ENABLE_WARP
        n2 += pnoise(nc1 * 8, np * 8) * 0.125;
        n3 += pnoise(nc1 * 8, np * 8) * 0.125;
    #endif
    #endif

    #if DEPTH5
        n1 += pnoise(nc1 * 16, np * 16) * 0.0625;
    #if ENABLE_WARP
        n2 += pnoise(nc1 * 16, np * 16) * 0.0625;
        n3 += pnoise(nc1 * 16, np * 16) * 0.0625;
    #endif
    #endif

        float3 op = float3(vp.x, 0, vp.y);

    #if ENABLE_WARP
        float3 d = float3(n2, n1, n3);
    #else
        float3 d = float3(0, n1, 0);
    #endif

        op += clamp(d, _ClampRange.x, _ClampRange.y) * _Amplitude;

        return float4(op, 1);
    }

    // Pass 1: Calculates normal vectors for the 1st submesh
    float4 frag_normal1(v2f_img i) : SV_Target 
    {
        float2 duv = _MainTex_TexelSize;

        float3 v1 = tex2D(_MainTex, i.uv + float2(0, 0) * duv).xyz;
        float3 v2 = tex2D(_MainTex, i.uv + float2(1, 1) * duv).xyz;
        float3 v3 = tex2D(_MainTex, i.uv + float2(2, 0) * duv).xyz;

        float3 n = normalize(cross(v2 - v1, v3 - v1));

        return float4(n, 0);
    }

    // Pass 2: Calculates normal vectors for the 2nd submesh
    float4 frag_normal2(v2f_img i) : SV_Target 
    {
        float2 duv = _MainTex_TexelSize;

        float3 v1 = tex2D(_MainTex, i.uv + float2( 0, 0) * duv).xyz;
        float3 v2 = tex2D(_MainTex, i.uv + float2(-1, 1) * duv).xyz;
        float3 v3 = tex2D(_MainTex, i.uv + float2( 1, 1) * duv).xyz;

        float3 n = normalize(cross(v2 - v1, v3 - v1));

        return float4(n, 0);
    }

    ENDCG

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_position
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_normal1
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_normal2
            ENDCG
        }
    }
}
