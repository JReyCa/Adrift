using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReyToolkit.Hex
{
    public static class HexMath
    {
        // Multiply this by the circumradius of the hexagon to get the inner radius (distance from centre to midpoint of edge).
        public static float CircumToInner => Mathf.Cos(30.0f * Mathf.Deg2Rad);

        public static float InnerToCircum => 1.0f / CircumToInner;

        // The ratio of the x and y distances between two hexagons in a grid.
        public static float GridRatio => 4.0f * CircumToInner / 3.0f;
    }

}
