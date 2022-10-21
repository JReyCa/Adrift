using Noise;
using UnityEngine;

namespace PlanetCreation
{
    // Meant to be interpreted by a ShapeGenerator object.
    [CreateAssetMenu(fileName = "ShapeSettings", menuName = "Planet Creation/Shape Settings")]
    public class ShapeSettings : ScriptableObject
    {
        [Min(0f)]
        public float radius = 50f;

        public bool useFirstLayerAsMask = true;
        public AnimationCurve maskCurve = AnimationCurve.Constant(0f, 1f, 1f);

        public NoiseLayer[] noiseLayers = { NoiseLayer.Default() };

        // Get the farthest out from the centre a point on the planet can be.
        public float GetMaxRadius()
        {
            float maxRadius = radius;

            foreach (var layer in noiseLayers)
            {
                if (!layer.hide)
                    maxRadius += radius * layer.strength;
            }
            
            return maxRadius;
        }

        [System.Serializable]
        public class NoiseLayer
        {
            public string name = "Base Layer";
            public int seed = 0;

            [Range(0f, 1f)]
            public float strength = 1f;

            public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            public NoiseSettings noiseSettings = NoiseSettings.Default(NoiseDimensions.Noise3D);
            public bool useRidgedNoise = false;
            public bool hide = false;

            public NoiseLayer(NoiseSettings noiseSettings)
            {
                this.noiseSettings = noiseSettings;
            }

            public static NoiseLayer Default() => new(NoiseSettings.Default(NoiseDimensions.Noise3D));
        }
    }
}
