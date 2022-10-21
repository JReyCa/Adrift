using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noise
{
    public class NoiseFilter : NoiseFilterBase
    {
        public NoiseFilter(int seed, NoiseSettings settings) : base(seed, settings) { }

        protected override NoiseMethod ChooseNoiseMethod(NoiseDimensions dimensions)
        {
            return dimensions switch
            {
                NoiseDimensions.Noise1D => Simplex.Sample1D,
                NoiseDimensions.Noise2D => Simplex.Sample2D,
                NoiseDimensions.Noise3D => Simplex.Sample3D,
                _ => Simplex.Sample2D
            };
        }
    }
}
