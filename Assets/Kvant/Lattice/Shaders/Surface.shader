//
// Surface shader for Lattice
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
        _PositionTex       ("-", 2D)     = ""{}
        _NormalTex         ("-", 2D)     = ""{}

        _Color             ("-", Color)  = (1, 1, 1, 1)
        _PbrParams         ("-", Vector) = (0.9, 0.9, 0, 0) // (metalness, smoothness)
        _UVOffset          ("-", Vector) = (0, 0, 0, 0)

        _MainTex           ("-", 2D)     = "white"{}
		_BumpMap           ("-", 2D)     = "bump"{}
		_OcclusionMap      ("-", 2D)     = "white"{}
		_OcclusionStrength ("-", Float)  = 1.0
        _MapParams         ("-", Vector) = (0, 0, 0, 1) // (offset x, y, z, scale)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Offset 1, 1
        
        CGPROGRAM

        #pragma surface surf Standard vertex:vert nolightmap addshadow
        #pragma target 3.0

        #pragma multi_compile _ _ALBEDOMAP
        #pragma multi_compile _ _NORMALMAP
        #pragma multi_compile _ _OCCLUSIONMAP

        sampler2D _PositionTex;
        sampler2D _NormalTex;

        half4 _Color;
        half2 _PbrParams;
        float2 _UVOffset;

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _OcclusionMap;
        half _OcclusionStrength;
        float4 _MapParams;

        struct Input
        {
        #if _ALBEDOMAP || _NORMALMAP || _OCCLUSIONMAP
            float3 localCoord;
            float3 localNormal;
        #else
            half dummy;
        #endif
        };

        void vert(inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);

            float4 uv1 = float4(v.texcoord  + _UVOffset, 0, 0);
            float4 uv2 = float4(v.texcoord1 + _UVOffset, 0, 0);

            v.vertex.xyz += tex2Dlod(_PositionTex, uv1).xyz;
            v.normal = tex2Dlod(_NormalTex, uv2).xyz;

        #if _NORMALMAP
            v.tangent = float4(normalize(cross(float3(1, 0, 0), v.normal)), 1);
        #endif

        #if _ALBEDOMAP || _NORMALMAP || _OCCLUSIONMAP
            data.localCoord = (v.vertex.xyz + float3(_MapParams.xyz)) * _MapParams.w;
            data.localNormal = v.normal.xyz;
        #endif
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
        #if _ALBEDOMAP || _NORMALMAP || _OCCLUSIONMAP
            // Calculate a blend factor for triplanar mapping.
            float3 blend = normalize(abs(IN.localNormal));
            blend /= dot(blend, (float3)1);

            // Get texture coordinates.
            float2 pmx = IN.localCoord.yz;
            float2 pmy = IN.localCoord.zx;
            float2 pmz = IN.localCoord.xy;
        #endif

        #if _ALBEDOMAP
            // Base color
            half4 cx = tex2D(_MainTex, pmx) * blend.x;
            half4 cy = tex2D(_MainTex, pmy) * blend.y;
            half4 cz = tex2D(_MainTex, pmz) * blend.z;
            half4 color = (cx + cy + cz) * _Color;
            o.Albedo = color.rgb;
            o.Alpha = color.a;
        #else
            o.Albedo = _Color.rgb;
            o.Alpha = _Color.a;
        #endif

        #if _NORMALMAP
            // Normal map
            half4 nx = tex2D(_BumpMap, pmx) * blend.x;
            half4 ny = tex2D(_BumpMap, pmy) * blend.y;
            half4 nz = tex2D(_BumpMap, pmz) * blend.z;
            o.Normal = UnpackNormal(nx + ny + nz);
        #endif

        #if _OCCLUSIONMAP
            // Occlusion map
            half ox = tex2D(_OcclusionMap, pmx).g * blend.x;
            half oy = tex2D(_OcclusionMap, pmy).g * blend.y;
            half oz = tex2D(_OcclusionMap, pmz).g * blend.z;
            o.Occlusion = lerp((half4)1, ox + oy + oz, _OcclusionStrength);
        #endif

            o.Metallic = _PbrParams.x;
            o.Smoothness = _PbrParams.y;
        }

        ENDCG
    } 
}
