using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetCreation
{
    [CreateAssetMenu(fileName = "MaterialSettings", menuName = "Planet Creation/Material Settings")]
    public class MaterialSettings : ScriptableObject
    {
        public Level[] levels = { new() };

        public Texture2D GetThresholdData() => GetDataTexture(EncodeThresholds);

        public Texture2DArray GetAlbedoTextures()
        {
            if (levels.Length == 0)
                return null;

            // Assume that all textures use the format of the first one found.
            // Also assume that textures are square and all the same size.
            TextureFormat textureFormat = TextureFormat.RGBA32;
            int size = 1024;

            foreach (var l in levels)
            {
                if (l.albedoTexture)
                {
                    textureFormat = l.albedoTexture.format;
                    size = l.albedoTexture.width;
                    break;
                }
            }

            Texture2DArray textures = new(size, size, levels.Length, textureFormat, false);
            
            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i].albedoTexture)
                    Graphics.CopyTexture(levels[i].albedoTexture, 0, textures, i);
            }

            return textures;
        }

        public Texture2DArray GetNormalTextures()
        {
            if (levels.Length == 0)
                return null;

            // Assume that all textures use the format of the first one found.
            // Also assume that textures are square and all the same size.
            TextureFormat textureFormat = TextureFormat.RGBA32;
            int size = 1024;

            foreach (var l in levels)
            {
                if (l.normalTexture)
                {
                    textureFormat = l.normalTexture.format;
                    size = l.normalTexture.width;
                    break;
                }
            }

            Texture2DArray textures = new(size, size, levels.Length, textureFormat, false);

            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i].normalTexture)
                    Graphics.CopyTexture(levels[i].normalTexture, 0, textures, i);
            }

            return textures;
        }

        private Texture2D GetDataTexture(EncodingMethod encodingMethod)
        {
            Texture2D dataTexture = new(levels.Length, 1)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            dataTexture.SetPixels32(encodingMethod());
            dataTexture.Apply();

            return dataTexture;
        }

        private Color32[] EncodeThresholds()
        {
            Color32[] colours = new Color32[levels.Length];

            for (int i = 0; i < levels.Length; i++)
            {
                float t = levels[i].threshold;
                float b = levels[i].blend;
                float s = levels[i].scale;
                colours[i] = new Color(t, b, s);
            }

            return colours;
        }

        [System.Serializable]
        public class Level
        {
            public string name = "Terrain Level";

            public Texture2D albedoTexture = null;
            public Texture2D normalTexture = null;

            [Range(0f, 1f)]
            public float threshold = 1f;

            [Range(0f, 1f)]
            public float blend = 0f;

            [Range(0.01f, 1f)]
            public float scale = 1f;
        }

        private delegate Color32[] EncodingMethod();
    }
}
