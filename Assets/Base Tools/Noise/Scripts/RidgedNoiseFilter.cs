using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noise
{
    public class RidgedNoiseFilter : NoiseFilter
    {
        public RidgedNoiseFilter(int seed, NoiseSettings settings) : base(seed, settings) { }

        protected override FilterMethod ChooseFilterMethod() => RidgeFilter;

        private float RidgeFilter(float v)
        {
            v = v * 2f - 1;         // Normalize between 1 and -1.
            v = 1 - Mathf.Abs(v);   // Take absolute value to produce valleys, then invert to get ridges.
            v *= v;                 // Square it to sharpen ridges.

            return v;
        }
    }
}
