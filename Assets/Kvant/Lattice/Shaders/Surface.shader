//
// Opaque surface shader for Lattice
//
// Vertex format:
// position     = not in use
// texcoord0.xy = uv for position texture
// texcoord1.xy = uv for normal texture
//
Shader "Kvant/Lattice/Surface"
{
    Properties
    {
        _PositionBuffer ("-", 2D) = "black"{}
        _NormalBuffer   ("-", 2D) = "black"{}

        _Color          ("-", Color) = (1, 1, 1, 1)
        _Metallic       ("-", Range(0,1)) = 0.5
        _Smoothness     ("-", Range(0,1)) = 0.5

        _MainTex        ("-", 2D) = "white"{}
        _NormalMap      ("-", 2D) = "bump"{}
        _NormalScale    ("-", Range(0,2)) = 1
        _OcclusionMap   ("-", 2D) = "white"{}
        _OcclusionStr   ("-", Float) = 1.0
        _MapScale       ("-", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Offset 1, 1

        CGPROGRAM

        #pragma surface surf Standard vertex:vert nolightmap addshadow
        #pragma shader_feature _ALBEDOMAP
        #pragma shader_feature _NORMALMAP
        #pragma shader_feature _OCCLUSIONMAP
        #pragma target 3.0

        sampler2D _PositionBuffer;
        sampler2D _NormalBuffer;
        float2 _BufferOffset; // hidden on inspector

        half4 _Color;
        half _Metallic;
        half _Smoothness;

        sampler2D _MainTex;
        sampler2D _NormalMap;
        half _NormalScale;
        sampler2D _OcclusionMap;
        half _OcclusionStr;
        half _MapScale;
        float3 _MapOffset; // hidden on inspector
        half _UseBuffer;   // hidden on inspector

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

            float4 uv1 = float4(v.texcoord  + _BufferOffset, 0, 0);
            float4 uv2 = float4(v.texcoord1 + _BufferOffset, 0, 0);

            float4 p = tex2Dlod(_PositionBuffer, uv1);
            float3 n = tex2Dlod(_NormalBuffer, uv2).xyz;

            v.vertex = lerp(v.vertex, p, _UseBuffer);
            v.normal = lerp(v.normal, n, _UseBuffer);

        #if _NORMALMAP
            v.tangent = float4(normalize(cross(float3(1, 0, 0), v.normal)), 1);
        #endif

        #if _ALBEDOMAP || _NORMALMAP || _OCCLUSIONMAP
            data.localCoord = (v.vertex.xyz + float3(_MapOffset)) * _MapScale;
            data.localNormal = v.normal;
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
            half4 nx = tex2D(_NormalMap, pmx) * blend.x;
            half4 ny = tex2D(_NormalMap, pmy) * blend.y;
            half4 nz = tex2D(_NormalMap, pmz) * blend.z;
            o.Normal = UnpackScaleNormal(nx + ny + nz, _NormalScale);
        #endif

        #if _OCCLUSIONMAP
            // Occlusion map
            half ox = tex2D(_OcclusionMap, pmx).g * blend.x;
            half oy = tex2D(_OcclusionMap, pmy).g * blend.y;
            half oz = tex2D(_OcclusionMap, pmz).g * blend.z;
            o.Occlusion = lerp((half4)1, ox + oy + oz, _OcclusionStr);
        #endif

            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
        }

        ENDCG
    }
    CustomEditor "Kvant.LatticeMaterialEditor"
}
