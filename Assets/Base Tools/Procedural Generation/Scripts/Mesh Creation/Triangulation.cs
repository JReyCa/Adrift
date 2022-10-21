using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralGen
{
    // Computes the index buffer (the "triangles" array in a mesh) for a variety of common meshes.
    public static class Triangulation
    {
        // Return an index buffer for a quad.
        // **********************************************************************
        // index00  ->  Index of bottom-left vertex (when quad is viewed head-on)
        // index01  ->  Index of top-left vertex
        // index11  ->  Index of top-right vertex
        // index10  ->  Index of bottom-right vertex
        public static List<int> Quad(int index00, int index01, int index11, int index10)
        {
            // ************ Diagram ***************
            // index01 -> *___________* <- index11
            //            |         / |
            // triangle #1 ->     /   |
            //            |     /     |
            //            |   /     <- triangle #2
            //            | /         |
            // index00 -> *___________* <- index10
            // ************************************

            return new()
            {
                // First triangle.
                index00,
                index01,
                index11,

                // Second triangle.
                index00,
                index11,
                index10
            };
        }

        // Return the index buffer for a smooth-shaded plane (assumed to be horizontal).
        // *****************************************************************************
        // quadCountX   ->  The number of quads along the x-axis.
        // quadCountZ   ->  The number of quads along the z-axis.
        public static List<int> Plane(int quadCountX, int quadCountZ, int startIndex = 0)
        {
            List<int> indexBuffer = new();

            // Iterate over each quad.
            for (int z = 0; z < quadCountZ; z++)
            {
                for (int x = 0; x < quadCountX; x++)
                {
                    // Find the indices of the four vertices that make up this quad.
                    int index00 = startIndex + x + z * (quadCountX + 1);
                    int index01 = index00 + quadCountX + 1;
                    int index11 = index01 + 1;
                    int index10 = index00 + 1;

                    indexBuffer.AddRange(Quad(index00, index01, index11, index10));
                }
            }

            return indexBuffer;
        }
    }
}
