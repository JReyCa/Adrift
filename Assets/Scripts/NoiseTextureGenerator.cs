using Noise;
using ProceduralGen;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseTextureGenerator : TextureGenerator
{
    #region Inspector Variables
    [SerializeField]
    private int seed;

    [SerializeField]
    private Gradient gradient;

    [SerializeField]
    private NoiseSettings noiseSettings = NoiseSettings.Default();
    #endregion

    protected override Texture2D GenerateTexture()
    {
        float[,] map = new float[width, height];
        NoiseFilter noiseFilter = new(seed, noiseSettings);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                map[x, y] = noiseFilter.Evaluate(new Vector3(x, y, 0f));
            }
        }

        return TexturePainter.PaintMap(map, gradient);
    }
}
