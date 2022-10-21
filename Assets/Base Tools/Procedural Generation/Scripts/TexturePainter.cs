using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralGen
{
    public static class TexturePainter
    {
        public static Texture2D PaintWhiteNoise(int width, int height, int seed)
        {
            Texture2D tex = new(width, height)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Point
            };

            System.Random rng = new(seed);
            Color32[] pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float c = (float)rng.NextDouble();
                    pixels[y * width + x] = new Color(c, c, c);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            return tex;
        }

        public static Texture2D PaintMap(float[,] map, Gradient gradient)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            Texture2D tex = new(width, height)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            Color32[] pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[x + y * width] = gradient.Evaluate(map[x, y]);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            return tex;
        }

        // Paint some data into a 1D texture.
        // This is a decent method for saving array data to a material in a way that will be serialized.
        public static Texture2D PaintChannels1D(float[] channelA, float[] channelB = null, float[] channelC = null, float[] channelD = null, float compression = 100.0f)
        {
            // Validation of input.
            if (channelA == null)
            {
                Debug.LogError("Missing channel A!");
                return null;
            }

            int size = channelA.Length;
            bool inputIsValid = ExtraChannelIsValid(channelB, size) && ExtraChannelIsValid(channelC, size) && ExtraChannelIsValid(channelD, size);

            if (!inputIsValid)
                return null;

            // Construct the texture.
            Texture2D tex = new(size, 1)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Point
            };

            Color[] pixels = new Color[size];

            for (int i = 0; i < size; i++)
            {
                float a = channelA[i] / compression;
                float b = channelB != null ? channelB[i] / compression : 0;
                float c = channelC != null ? channelC[i] / compression : 0;
                float d = channelD != null ? channelD[i] / compression : 0;

                pixels[i] = new(a, b, c, d);
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return tex;
        }

        // Convert a Vector2 to two float arrays before using them to paint a texture.
        public static Texture2D PaintChannels1D(Vector2[] vectors, float compression = 100)
        {
            int size = vectors.Length;
            float[] channelA = new float[size];
            float[] channelB = new float[size];

            for (int i = 0; i < size; i++)
            {
                channelA[i] = vectors[i].x;
                channelB[i] = vectors[i].y;
            }

            return PaintChannels1D(channelA, channelB, null, null, compression);
        }

        public static Texture2D PaintChannels1D(Vector3[] vectors, float compression = 100)
        {
            int size = vectors.Length;
            float[] channelA = new float[size];
            float[] channelB = new float[size];
            float[] channelC = new float[size];

            for (int i = 0; i < size; i++)
            {
                channelA[i] = vectors[i].x;
                channelB[i] = vectors[i].y;
                channelC[i] = vectors[i].z;
            }

            return PaintChannels1D(channelA, channelB, channelC, null, compression);
        }

        private static bool ExtraChannelIsValid(float[] channel, int size)
        {
            if (channel != null && channel.Length != size)
            {
                Debug.LogError("This channel is invalid! Check whether it's a different length than the other channels.");
                return false;
            }

            return true;
        }
    }
}
