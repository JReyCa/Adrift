using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noise
{
    public enum NoiseDimensions
    {
        Noise1D = 1,
        Noise2D = 2,
        Noise3D = 3
    }

    public abstract class NoiseFilterBase
    {
        private NoiseMethod noiseMethod;
        private FilterMethod filterMethod;
        private NoiseSettings settings;
        private Vector3[] offsets;

        protected NoiseFilterBase(int seed, NoiseSettings settings)
        {
            noiseMethod = ChooseNoiseMethod(settings.dimensions);
            filterMethod = ChooseFilterMethod();
            this.settings = settings;
            offsets = GenerateOffsets(seed, settings);
        }

        public float Evaluate(Vector3 point)
        {
            // These values get modified and re-used across octaves.
            float sample = 0f;
            float maxValue = 0f;
            float amplitude = 1f;
            float frequency = 1f;

            // Iterate over the octaves.
            for (int i = 0; i < settings.octaves; i++)
            {
                Vector3 input = point / settings.scale * frequency;

                // Push the sample point by an offset.
                if (offsets != null && offsets.Length - 1 >= i)
                {
                    input += offsets[i];
                }

                float v = noiseMethod(input);

                if (filterMethod != null)
                    v = filterMethod(v);

                sample += v * amplitude;

                maxValue += amplitude;              // Keep track of what the largest possible value will be.
                amplitude *= settings.persistence;  // Amplitude is reduced by persistence each octave.
                frequency *= settings.lacunarity;   // Frequency is increased by lacunarity each octave.
            }

            // The sample is normalized at the end by dividing by the largest possible value.
            sample /= maxValue;

            return sample;
        }

        protected virtual FilterMethod ChooseFilterMethod() => null;

        protected abstract NoiseMethod ChooseNoiseMethod(NoiseDimensions dimensions);

        private Vector3[] GenerateOffsets(int seed, NoiseSettings settings)
        {
            System.Random rng = new(seed);
            int dimensions = (int)settings.dimensions;
            Vector3[] offsets = new Vector3[settings.octaves];

            for (int i = 0; i < settings.octaves; i++)
            {
                // 1D is guaranteed.
                float xOffset = rng.Next(-1000, 1000);
                float yOffset = 0f;
                float zOffset = 0f;

                // For 2D
                if (dimensions >= 2)
                    yOffset = rng.Next(-1000, 1000);

                // For 3D
                if (dimensions == 3)
                    zOffset = rng.Next(-1000, 1000);

                offsets[i] = new(xOffset, yOffset, zOffset);
            }

            return offsets;
        }

        protected delegate float NoiseMethod(Vector3 point);
        protected delegate float FilterMethod(float rawValue);
    }
}
