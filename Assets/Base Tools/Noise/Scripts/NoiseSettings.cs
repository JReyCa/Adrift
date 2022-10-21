using System;
using UnityEngine;

namespace Noise
{
    [Serializable]
    public struct NoiseSettings
    {
        #region Inspector Variables
        public NoiseDimensions dimensions;

        [Tooltip("How many iterations of noise are layered on each other.")]
        [Min(1)]
        public int octaves;

        [Tooltip("How zoomed in or out of the map we are (larger values are more zoomed in).")]
        [Min(0.01f)]
        public float scale;

        [Tooltip("How much does amplitude persist on each successive octave (0-1).")]
        [Range(0.001f, 1.0f)]
        public float persistence;

        [Tooltip("How fast does frequency increase on each successive octave (>=1).")]
        [Min(1f)]
        public float lacunarity;
        #endregion

        public static NoiseSettings Default(NoiseDimensions dimensions = NoiseDimensions.Noise2D)
            => new()
            {
                dimensions = dimensions,
                octaves = 1,
                scale = 1.0f,
                persistence = 1.0f,
                lacunarity = 1.0f
            };
    }
}
