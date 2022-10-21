Shader "Planet Creation/SpaceTerrainOld"
{
    Properties
    {
        _Colour ("Colour", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "LightMode"="ForwardBase" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"

            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : Normal;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed3 diffuse : COLOR0;
                fixed3 ambient : COLOR1;
                float2 uv : TEXCOORD0;
                half3 worldRefl : TEXCOORD2;    // The "SHADOW_COORDS()" thing is internally using TEXCOORD1.
                SHADOW_COORDS(1)                // Shadow magic
            };

            fixed4 _Colour;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Light
            float3 _DirectionalLight;
            float _DirectionalLight_Intensity;
            float4 _DirectionalLight_Colour;

            float _MinLightIntensity;

            v2f vert (appdata v)
            {
                v2f o;

                // The basics
                o.pos = UnityObjectToClipPos(v.vertex); // Convert object space to homogenous clip space.
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);   // Pull uv coords from texture.

                // World space info
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;           // Convert object space position to world space position.
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);             // Convert object space normal to world space normal.
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));  // Get the direction from the camera to the vertex.

                // Light calculations
                half lambertDiffuse = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));   // Get diffuse lighting.
                o.diffuse = lambertDiffuse * _LightColor0;                                  // Factor in the light colour.
                o.ambient = ShadeSH9(half4(worldNormal, 1));                                // Include ambient light.
                o.worldRefl = reflect(-worldViewDir, worldNormal);

                TRANSFER_SHADOW(o)  // Shadow magic

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);
                half3 skyColour = DecodeHDR (skyData, unity_SpecCube0_HDR);

                fixed4 colour = tex2D(_MainTex, i.uv) * _Colour;
                fixed shadow = SHADOW_ATTENUATION(i);
                fixed3 lighting = (i.diffuse * skyColour) * shadow + i.ambient;

                colour.rgb *= lighting;

                return colour;
            }
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER" // So much shadow magic
    }
}
