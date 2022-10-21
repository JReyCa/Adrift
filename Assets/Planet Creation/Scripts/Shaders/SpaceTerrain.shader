Shader "Planet Creation/SpaceTerrain"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)

        [NoScaleOffset]
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset]
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Scale ("Scale", Float) = 1

        _Smoothness ("Smoothness", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        struct Input
        {
            half3 worldNormal;
            half3 objectNormal;
            float3 objectPos;
            INTERNAL_DATA
        };

        sampler2D _MainTex;
        sampler2D _NormalMap;
        half _Scale;

        half _Smoothness;
        fixed4 _Color;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
            float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
            float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.objectPos = v.vertex.xyz;
            o.objectNormal = v.normal;
        }

        half3 objectToWorldNormal(half3 objectNormal)
        {
            return mul(unity_ObjectToWorld, half4(objectNormal, 0)).xyz;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            half3 triblend = pow(abs(IN.objectNormal), 4);
            triblend /= triblend.x + triblend.y + triblend.z;
            
            half3 axisSign = IN.objectNormal < 0 ? -1 : 1;

            // Get uv coords for triplanar sampling.
            float2 uvX = IN.objectPos.zy / _Scale;
            float2 uvY = IN.objectPos.xz / _Scale;
            float2 uvZ = IN.objectPos.xy / _Scale;

            uvX.x *= -1;
            uvY.x *= -axisSign.y;
            uvZ.x *= -axisSign.z;

            // Sample and unpack normal textures (which are in tangent space).
            half3 trinormalX = UnpackNormal(tex2D(_NormalMap, uvX));
            half3 trinormalY = UnpackNormal(tex2D(_NormalMap, uvY));
            half3 trinormalZ = UnpackNormal(tex2D(_NormalMap, uvZ));

            // Swizzle world normals to tangent space and apply Whiteout blend.
            trinormalX = half3(trinormalX.xy + IN.objectNormal.zy, abs(trinormalX.z) * IN.objectNormal.x);
            trinormalY = half3(trinormalY.xy + IN.objectNormal.xz, abs(trinormalY.z) * IN.objectNormal.y);
            trinormalZ = half3(trinormalZ.xy + IN.objectNormal.xy, abs(trinormalZ.z) * IN.objectNormal.z);

            half3 triplanarObjectNormal = normalize(trinormalX.zyx * triblend.x + trinormalY.xzy * triblend.y + trinormalZ.xyz * triblend.z);
            half3 triplanarWorldNormal = mul(unity_ObjectToWorld, half4(triplanarObjectNormal, 0)).xyz;

            //o.Albedo = 1;
            //o.Albedo = WorldToTangentNormalVector(IN, triplanarWorldNormal);
            //o.Albedo = WorldToTangentNormalVector(IN, trinormalY.xzy);
            o.Albedo = WorldToTangentNormalVector(IN, mul(unity_ObjectToWorld, half4(trinormalZ, 0)).xyz);
            o.Smoothness = _Smoothness;
            //o.Normal = WorldToTangentNormalVector(IN, triplanarWorldNormal);
            o.Normal = half3(0,0,1);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
