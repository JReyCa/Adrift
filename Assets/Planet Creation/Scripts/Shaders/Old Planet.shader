Shader "Planet Creation/Old Planet"
{
    Properties
    {
        _NumTerrainLevels ("Number of Terrain Levels", Integer) = 1
        _MinRadius ("Min Radius", Float) = 1
        _MaxRadius ("Max Radius", Float) = 1

        [Header(Data Textures)]
        [NoScaleOffset]
        _AlbedoTextures ("Albedo Textures", 2DArray) = "" {}

        [NoScaleOffset]
        _NormalTextures ("Normal Textures", 2DArray) = "" {}

        [NoScaleOffset]
        _ThresholdData ("Threshold Data", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.5

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        static const float epsilon = 0.0000000001;

        int _NumTerrainLevels;
        float _MinRadius;
        float _MaxRadius;

        sampler2D _ThresholdData;
        float4 _ThresholdData_TexelSize;

        UNITY_DECLARE_TEX2DARRAY(_AlbedoTextures);
        UNITY_DECLARE_TEX2DARRAY(_NormalTextures);

        float inverseLerp(float min, float max, float value)
        {
            return saturate((value - min) / (max - min));
        }

        half3 blend_rnm(half3 n1, half3 n2)
        {
            n1.z += 1;
            n2.xy = -n2.xy;

            return n1 * dot(n1, n2) / n1.z - n2;
        }

        // Convert a normal in tangent space to world space.
        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
            float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
            float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        // Surface function
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Use object position for triplanar uvs so that texture coords don't change as the planet moves.
            float3 objectPos = mul(unity_WorldToObject, float4(IN.worldPos, 1));

            // Get the height of the terrain, normalized 0-1.
            float radius = length(objectPos);
            float height = inverseLerp(_MinRadius - epsilon, _MaxRadius, radius);

            // There's a bug where IN.worldNormal doesn't get set correctly.
            // This is a workaround.
            IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

            // The 3 components of "triblend" are the blending weights for the three planes.
            float3 triblend = pow(IN.worldNormal, 4);
            triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

            // This will hold the final colour value.
            fixed4 colour = fixed4(0,0,0,1);
            float3 normal = float3(0,0,1);

            // Iterate over terrain levels and mix the right colours and normals.
            for (int i = 0; i < _NumTerrainLevels; i++)
            {
                //// Extract threshold data.
                //float2 dataCoords = float2(_ThresholdData_TexelSize.x * i, 0);
                //fixed4 thresholdData = tex2Dlod(_ThresholdData, float4(dataCoords, 0, 0));
                //float threshold = thresholdData[0];
                //float levelBlend = thresholdData[1] * 0.5;
                //float scale = thresholdData[2] * 100;

                //// Get triplanar uvs.
                //float2 uvX = objectPos.zy / scale;  // X-facing plane.
                //float2 uvY = objectPos.xz / scale;  // Y-facing plane.
                //float2 uvZ = objectPos.xy / scale;  // Z-facing plane.

                //uvY += 0.33;
                //uvZ += 0.67;

                //half3 axisSign = IN.worldNormal < 0 ? -1 : 1;
                //uvX.y *= axisSign.x;
                //uvY.y *= axisSign.y;
                //uvZ.y *= axisSign.z;

                //// Get colour projections and blend.
                //fixed4 tricolourX = UNITY_SAMPLE_TEX2DARRAY(_AlbedoTextures, float3(uvX, i));
                //fixed4 tricolourY = UNITY_SAMPLE_TEX2DARRAY(_AlbedoTextures, float3(uvY, i));
                //fixed4 tricolourZ = UNITY_SAMPLE_TEX2DARRAY(_AlbedoTextures, float3(uvZ, i));
                //fixed4 currentColour = tricolourX * triblend.x + tricolourY * triblend.y + tricolourZ * triblend.z;

                //// Get normal projections in tangent space.
                //half3 trinormalX = UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(uvX, i)));
                //half3 trinormalY = UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(uvY, i)));
                //half3 trinormalZ = UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(uvZ, i)));

                //// Correct y component to match flipped UV.
                //trinormalX.y *= -axisSign.x;
                //trinormalY.y *= -axisSign.y;
                //trinormalZ.y *= -axisSign.z;
                
                //// Flip z so that it matches world space direction after conversion to world space.
                //trinormalX.z *= axisSign.x;
                //trinormalY.z *= axisSign.y;
                //trinormalZ.z *= axisSign.z;

                //half3 worldNormal = normalize(trinormalX.zyx * triblend.x +
                //                              trinormalY.xzy * triblend.y +
                //                              trinormalZ.xyz * triblend.z);

                //half3 testWorldNormal = trinormalX.zyx;

                //// Mix colours.
                //float drawStrength = inverseLerp(threshold - levelBlend - epsilon, threshold + levelBlend, height);
                //colour = lerp(colour, currentColour, drawStrength);
                ////normal = lerp(normal, WorldToTangentNormalVector(IN, worldNormal), drawStrength);
                //normal = lerp(normal, WorldToTangentNormalVector(IN, testWorldNormal), drawStrength);
                ////normal = lerp(normal, trinormalX, drawStrength);
            }

            // Extract threshold data.
            float2 dataCoords = float2(0, 0);
            fixed4 thresholdData = tex2Dlod(_ThresholdData, float4(dataCoords, 0, 0));
            float threshold = thresholdData[0];
            float levelBlend = thresholdData[1] * 0.5;
            float scale = thresholdData[2] * 100;

            // Get triplanar uvs.
            float2 uvX = objectPos.zy / scale;  // X-facing plane.
            float2 uvY = objectPos.xz / scale;  // Y-facing plane.
            float2 uvZ = objectPos.xy / scale;  // Z-facing plane.

            uvY += 0.33;
            uvZ += 0.67;

            half3 axisSign = IN.worldNormal < 0 ? -1 : 1;
            uvX.y *= axisSign.x;
            uvY.y *= axisSign.y;
            uvZ.y *= axisSign.z;

            // Get colour projections and blend.
            fixed4 tricolourX = UNITY_SAMPLE_TEX2DARRAY(_AlbedoTextures, float3(uvX, 0));
            fixed4 tricolourY = UNITY_SAMPLE_TEX2DARRAY(_AlbedoTextures, float3(uvY, 0));
            fixed4 tricolourZ = UNITY_SAMPLE_TEX2DARRAY(_AlbedoTextures, float3(uvZ, 0));
            fixed4 currentColour = tricolourX * triblend.x + tricolourY * triblend.y + tricolourZ * triblend.z;

            // Get normal projections in tangent space.
            half3 trinormalX = UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(uvX, 0)));
            half3 trinormalY = UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(uvY, 0)));
            half3 trinormalZ = UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(uvZ, 0)));

            // Correct y component to match flipped UV.
            trinormalX.y *= -axisSign.x;
            trinormalY.y *= -axisSign.y;
            trinormalZ.y *= -axisSign.z;
                
            // Flip z so that it matches world space direction after conversion to world space.
            trinormalX.z *= axisSign.x;
            trinormalY.z *= axisSign.y;
            trinormalZ.z *= axisSign.z;

            half3 worldNormal = normalize(trinormalX.zyx * triblend.x +
                                            trinormalY.xzy * triblend.y +
                                            trinormalZ.xyz * triblend.z);

            half3 testWorldNormal = trinormalX.zyx;

            // Mix colours.
            colour = currentColour;
            //normal = lerp(normal, WorldToTangentNormalVector(IN, worldNormal), drawStrength);
            normal = WorldToTangentNormalVector(IN, testWorldNormal);
            //normal = trinormalX;

            o.Albedo = colour;
            o.Smoothness = 0.0;
            o.Normal = normal;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
