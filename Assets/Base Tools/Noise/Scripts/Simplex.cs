using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noise
{
    public static class Simplex
    {
        private static int[] hash = {
            151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,    // 0-255 in random order, as a random number selector.
            140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,    // The numbers are repeated for efficient overflow.
            247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
             57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
             74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
             60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
             65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
            200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
             52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
            207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
            119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
            129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
            218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
             81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
            184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
            222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180,

            151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
            140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
            247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
             57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
             74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
             60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
             65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
            200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
             52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
            207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
            119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
            129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
            218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
             81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
            184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
            222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
        };

        private const int hashMask = 255;

        // *********************************** 1D ************************************
        // The 1D gradients are just the positive and negative directions.
        private static float[] gradients1D = new float[]
        {
            1f, -1f
        };

        private const int gradientsMask1D = 1;

        // Compute the one dimensional simplex noise value at a point in the number line.
        public static float Sample1D(float x)
        {
            int ix = Mathf.FloorToInt(x);

            // Return the sum of the contributions of the points before and after the sample.
            float sample = SimplexValue1DPart(x, ix);
            sample += SimplexValue1DPart(x, ix + 1);
            
            // The sample is in range 0-27/64, so we divide by 27/64.
            return (sample * (64f / 27f) + 1) * 0.5f;
        }

        public static float Sample1D(Vector3 point) => Sample1D(point.x);

        // Compute the contribution of a simplex point to the sample.
        // Simplices in 1D are just line segments.
        private static float SimplexValue1DPart(float x, int ix)
        {
            // Get a random gradient vector and check how much it aligns with our direction from it.
            float gradient = gradients1D[hash[ix & hashMask] & gradientsMask1D];
            float influence = gradient * (x - ix);

            // Multiply influence with the falloff from point "ix" on the line.
            return influence * Falloff1D(x - ix);
        }

        // The smooth falloff function for one dimension:
        // (1 - x^2)^3
        private static float Falloff1D(float x)
        {
            float t = 1 - x * x;
            return t * t * t;
        }

        // *********************************** 2D ************************************
        // The four orthogonal and the four diagonal directions.
        private static Vector2[] gradients2D = {
            new Vector2( 1f, 0f),
            new Vector2(-1f, 0f),
            new Vector2( 0f, 1f),
            new Vector2( 0f,-1f),
            new Vector2( 1f, 1f).normalized,
            new Vector2(-1f, 1f).normalized,
            new Vector2( 1f,-1f).normalized,
            new Vector2(-1f,-1f).normalized
        };

        private const int gradientsMask2D = 7;

        private static float squaresToTriangles = (3.0f - Mathf.Sqrt(3.0f)) / 6.0f;
        private static float trianglesToSquares = (Mathf.Sqrt(3.0f) - 1.0f) / 2.0f;
        private static float simplexScale2D = 2916f * Mathf.Sqrt(2f) / 125f;

        // Compute the two dimensional simplex noise value at a point in a plane.
        public static float Sample2D(float x, float y)
        {
            float skew = (x + y) * trianglesToSquares;

            // Adding this amount to x & y skews our coordinates from a grid of equilateral triangles
            // to a grid of squares. We do this to obtain the corner points (before unskewing them).
            // These corner points are actually the corners of a rhombus in unskewed space.
            float sx = x + skew;
            float sy = y + skew;

            // Find the bottom-left corner of the square that we're in.
            int ix = Mathf.FloorToInt(sx);
            int iy = Mathf.FloorToInt(sy);

            // Add the influences of the bottom-left and top-right corners of the rhombus.
            float sample = SimplexValue2DPart(x, y, ix, iy);
            sample += SimplexValue2DPart(x, y, ix + 1, iy + 1);

            // Check whether we need the bottom-right corner or the top-left corner.
            if (sx - ix >= sy - iy)
                sample += SimplexValue2DPart(x, y, ix + 1, iy);
            else
                sample += SimplexValue2DPart(x, y, ix, iy + 1);

            // Divide by the maximum value and shift range from [-1,1] to [0,1].
            return (sample * simplexScale2D + 1) * 0.5f;
        }

        public static float Sample2D(Vector3 point) => Sample2D(point.x, point.y);

        // Compute the contribution of a simplex point to the sample.
        // Simplices in 2D are triangles.
        private static float SimplexValue2DPart(float x, float y, int ix, int iy)
        {
            float unskew = (ix + iy) * squaresToTriangles;

            // Get x and y relative to the simplex point.
            float relX = x - ix + unskew;
            float relY = y - iy + unskew;

            // Influence is the dot product of the vector to the simplex point and a random gradient.
            Vector2 gradient = gradients2D[hash[hash[ix & hashMask] + iy & hashMask] & gradientsMask2D];
            float influence = Dot(gradient, relX, relY);

            return influence * Falloff2D(relX, relY);
        }

        // The falloff function for two dimensions:
        // ((1/2) - x^2 - y^2)^3
        // This function is essentially 1/2 minus the squared distance, and then cubed.
        // The maximum squared distance we get is 1/2, so we get a range of 0 to 1/8.
        private static float Falloff2D(float x, float y)
        {
            float t = 0.5f - x * x - y * y;

            return Mathf.Max(t * t * t, 0f);
        }

        // *********************************** 3D ************************************
        // The 3D gradients include repeats, since we need the number of gradients to be a power of
        // two in order for bit-masking to work. This doesn't affect the result all that much.
        private static Vector3[] simplexGradients3D = {
            new Vector3( 1f, 1f, 0f).normalized,
            new Vector3(-1f, 1f, 0f).normalized,
            new Vector3( 1f,-1f, 0f).normalized,
            new Vector3(-1f,-1f, 0f).normalized,
            new Vector3( 1f, 0f, 1f).normalized,
            new Vector3(-1f, 0f, 1f).normalized,
            new Vector3( 1f, 0f,-1f).normalized,
            new Vector3(-1f, 0f,-1f).normalized,
            new Vector3( 0f, 1f, 1f).normalized,
            new Vector3( 0f,-1f, 1f).normalized,
            new Vector3( 0f, 1f,-1f).normalized,
            new Vector3( 0f,-1f,-1f).normalized,

            new Vector3( 1f, 1f, 0f).normalized,
            new Vector3(-1f, 1f, 0f).normalized,
            new Vector3( 1f,-1f, 0f).normalized,
            new Vector3(-1f,-1f, 0f).normalized,
            new Vector3( 1f, 0f, 1f).normalized,
            new Vector3(-1f, 0f, 1f).normalized,
            new Vector3( 1f, 0f,-1f).normalized,
            new Vector3(-1f, 0f,-1f).normalized,
            new Vector3( 0f, 1f, 1f).normalized,
            new Vector3( 0f,-1f, 1f).normalized,
            new Vector3( 0f, 1f,-1f).normalized,
            new Vector3( 0f,-1f,-1f).normalized,

            new Vector3( 1f, 1f, 1f).normalized,
            new Vector3(-1f, 1f, 1f).normalized,
            new Vector3( 1f,-1f, 1f).normalized,
            new Vector3(-1f,-1f, 1f).normalized,
            new Vector3( 1f, 1f,-1f).normalized,
            new Vector3(-1f, 1f,-1f).normalized,
            new Vector3( 1f,-1f,-1f).normalized,
            new Vector3(-1f,-1f,-1f).normalized
        };

        private const int gradientsMask3D = 31;

        private static float simplexScale3D = 8192f * Mathf.Sqrt(3f) / 375f;

        // Compute the three dimensional simplex noise value at a point in space.
        public static float Sample3D(float x, float y, float z)
        {
            float skew = (1f / 3f) * (x + y + z);

            // Skew our coordinates to an approximation of cubed space.
            // Tetrahedra can't actually tile, so this is only decently close.
            float sx = x + skew;
            float sy = y + skew;
            float sz = z + skew;

            // Get the bottom-left-close corner of the cube.
            int ix = Mathf.FloorToInt(sx);
            int iy = Mathf.FloorToInt(sy);
            int iz = Mathf.FloorToInt(sz);

            // Get our coordinates inside the cube.
            float cubeX = sx - ix;
            float cubeY = sy - iy;
            float cubeZ = sz - iz;

            // Add influence from the bottom-left-close and top-right-far corners of the tetrahedron.
            float sample = SimplexValue3DPart(x, y, z, ix, iy, iz);
            sample += SimplexValue3DPart(x, y, z, ix + 1, iy + 1, iz + 1);

            // Add influence from the two closest corners.
            if (cubeX >= cubeY)
            {
                if (cubeX >= cubeZ)
                {
                    sample += SimplexValue3DPart(x, y, z, ix + 1, iy, iz);
                    if (cubeY >= cubeZ)
                        sample += SimplexValue3DPart(x, y, z, ix + 1, iy + 1, iz);
                    else
                        sample += SimplexValue3DPart(x, y, z, ix + 1, iy, iz + 1);
                }
                else
                {
                    sample += SimplexValue3DPart(x, y, z, ix, iy, iz + 1);
                    sample += SimplexValue3DPart(x, y, z, ix + 1, iy, iz + 1);
                }
            }
            else
            {
                if (cubeY >= cubeZ)
                {
                    sample += SimplexValue3DPart(x, y, z, ix, iy + 1, iz);
                    if (cubeX >= cubeZ)
                        sample += SimplexValue3DPart(x, y, z, ix + 1, iy + 1, iz);
                    else
                        sample += SimplexValue3DPart(x, y, z, ix, iy + 1, iz + 1);
                }
                else
                {
                    sample += SimplexValue3DPart(x, y, z, ix, iy, iz + 1);
                    sample += SimplexValue3DPart(x, y, z, ix, iy + 1, iz + 1);
                }
            }

            // The falloff function has a range of 0 to 1/8, so we normalize by multiplying by 8.
            // We also multiplied by a hash value, range 0 - 255, so we need to divide by 255 (the hash mask).
            return (sample * simplexScale3D + 1) * 0.5f;
        }

        public static float Sample3D(Vector3 point) => Sample3D(point.x, point.y, point.z);

        // Compute the contribution of a simplex point to the sample.
        // Simplices in 3D are tetrehedra.
        private static float SimplexValue3DPart(float x, float y, float z, int ix, int iy, int iz)
        {
            float unskew = (ix + iy + iz) * (1f / 6f);

            // Get x, y, and z relative to the simplex point.
            float relX = x - ix + unskew;
            float relY = y - iy + unskew;
            float relZ = z - iz + unskew;

            Vector3 gradient = simplexGradients3D[hash[hash[hash[ix & hashMask] + iy & hashMask] + iz & hashMask] & gradientsMask3D];
            float influence = Dot(gradient, relX, relY, relZ);

            return influence * Falloff3D(relX, relY, relZ);
        }

        // The falloff function for three dimensions:
        // (1/2 - x^2 - y^2 - z^2)^3
        private static float Falloff3D(float x, float y, float z)
        {
            float t = 0.5f - x * x - y * y - z * z;

            return Mathf.Max(t * t * t, 0f);
        }

        // Just a shorthand for dot product when one of the vectors is left as two floats.
        private static float Dot(Vector2 gradient, float x, float y)
        {
            return gradient.x * x + gradient.y * y;
        }

        private static float Dot(Vector3 gradient, float x, float y, float z)
        {
            return gradient.x * x + gradient.y * y + gradient.z * z;
        }
    }
}
