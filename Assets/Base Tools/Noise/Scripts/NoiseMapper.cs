using ReyToolkit.Hex;
using UnityEngine;

namespace Noise
{
    public static class NoiseMapper
    {
        public static float[,] WhiteNoiseMap(int seed, int rowLength, int rowCount)
        {
            float[,] noiseMap = new float[rowLength, rowCount];
            System.Random rng = new(seed);

            for (int y = 0; y < rowCount; y++)
            {
                for (int x = 0; x < rowLength; x++)
                {
                    noiseMap[x, y] = (float)rng.NextDouble();
                }
            }

            return noiseMap;
        }

        // Generate a map of simplex values, either in a square or a hexagonal grid.
        // ----------------------------------------------------------------------------
        // rowlength    ->  The length of the rows in the map (x-axis).
        // rowCount     ->  How many rows there are in the map (y-axis).
        // mappingType  ->  The type of grid to sample, either square or hexagonal.
        // settings     ->  A pre-made struct of settings for fractal noise generation.
        public static float[,] SimplexMap2D(int seed, int rowLength, int rowCount, MappingType mappingType, NoiseSettings settings)
        {
            float[,] map = new float[rowLength, rowCount];
            NoiseFilter noiseFilter = new(seed, settings);

            float xStep = mappingType == MappingType.Hexagonal ? HexMath.GridRatio : 1.0f;
            float yStep = 1.0f;

            // It's slightly more efficient to pre-calculate these.
            float halfX = rowLength * 0.5f;
            float halfY = rowCount * 0.5f;

            // Iterate over each position in the map...
            for (int y = 0; y < rowCount; y++)
            {
                for (int x = 0; x < rowLength; x++)
                {
                    float xf = (x - halfX) * xStep;
                    float yf = (y - halfY) * yStep;

                    // If the grid is hexagonal, every other row needs to be indented.
                    if (mappingType == MappingType.Hexagonal && y % 2 == 0)
                        xf += xStep * 0.5f;

                    Vector3 point = new(xf, yf, 0f);
                    map[x, y] = noiseFilter.Evaluate(point);
                }
            }

            return map;
        }

        public enum MappingType
        {
            Square,
            Hexagonal
        }
    }
}
