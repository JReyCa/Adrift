using Noise;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetCreation
{
    // The job of the shape generator is to take a point on a planet and apply noise and
    // whatever else we want, creating more interesting topography.
    public class ShapeGenerator
    {
        private ShapeSettings settings;
        private NoiseFilter[] noiseFilters;

        // Constructor
        public ShapeGenerator(ShapeSettings settings)
        {
            int numLayers = settings.noiseLayers.Length;
            noiseFilters = new NoiseFilter[numLayers];

            for (int i = 0; i < numLayers; i++)
            {
                var layer = settings.noiseLayers[i];

                // Don't use a layer if the user is hiding it.
                if (layer.hide)
                    continue;

                // For now we're using two different kinds of noise filters.
                NoiseFilter nf;
                if (layer.useRidgedNoise)
                    nf = new RidgedNoiseFilter(layer.seed, layer.noiseSettings);
                else
                    nf = new(layer.seed, layer.noiseSettings);

                noiseFilters[i] = nf;
            }

            this.settings = settings;
        }

        public Vector3 Evaluate(Vector3 point)
        {
            float noise = 0f;
            float baseNoise = 0f;

            // Iterate over the noise filters...
            for (int i = 0; i < noiseFilters.Length; i++)
            {
                // The noise filter should be null if the user wants to hide that noise layer.
                if (noiseFilters[i] == null)
                    continue;

                var layer = settings.noiseLayers[i];
                float addedNoise = noiseFilters[i].Evaluate(point);
                addedNoise = layer.curve.Evaluate(addedNoise);

                if (i == 0)
                    baseNoise = addedNoise;

                // If we're using the first noise layer as a mask, use it to hide other layers.
                if (i != 0)
                {
                    if (settings.useFirstLayerAsMask && !settings.noiseLayers[0].hide)
                    {
                        addedNoise *= settings.maskCurve.Evaluate(baseNoise);
                    }
                }
                
                noise += addedNoise * layer.strength;
            }

            return settings.radius * (noise + 1) * point;
        }
    }
}
