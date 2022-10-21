using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReyToolkit
{
    public static class ColourUtils
    {
        public static Color BlendColours(Color[] colours, float[] weights)
        {
            if (weights.Length != colours.Length)
            {
                throw new System.Exception("When blending colours, the colour array and the weights array must be the same length!");
            }

            Color output = new(0.0f, 0.0f, 0.0f, 0.0f);
            float totalWeight = 0.0f;

            for (int i = 0; i < colours.Length; i++)
            {
                output += colours[i] * weights[i];
                totalWeight += weights[i];
            }

            output /= totalWeight;
            return output;
        }
    }
}
