using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralGen.MeshGen
{
    public class IndexBuffer
    {
        private int[] mainTriangles;
        private int[] borderTriangles;

        private int mainIndex = 0;
        private int borderIndex = 0;

        // Properties
        public int this[int i]
        {
            get { return GetIndex(i); }
        }

        public int Size => mainTriangles.Length;
        public int BorderSize => borderTriangles.Length;

        // Constructor
        // -----------------------------------------------------------------
        // size         ->  The size of the main index array.
        // borderSize   ->  Optionally, also set a size for the border array.
        public IndexBuffer(int size, int borderSize = 0)
        {
            mainTriangles = new int[size];
            borderTriangles = new int[borderSize];
        }

        // Getter method
        // Positive indices access the main, or "real" triangle indices,
        // while negative indices access the border triangles (if used).
        // ------------------------------------------------------------
        // i    ->  The index in the vertex array.
        public int GetIndex(int i)
        {
            if (i >= 0)
                return mainTriangles[i];
            else
                return borderTriangles[-i - 1];
        }

        // Setter method
        // Positive indices access the main, or "real" triangle indices,
        // while negative indices access the border triangles (if used).
        // ------------------------------------------------------------
        // index    ->  The index in the vertex array.
        // isBorder ->  Whether this is an index in a border triangle.
        public void SetIndex(int index, bool isBorder = false)
        {
            if (!isBorder)
                mainTriangles[mainIndex++] = index;
            else
                borderTriangles[borderIndex++] = index;
        }

        public void SetTriangle(int indexA, int indexB, int indexC, bool isBorder = false)
        {
            SetIndex(indexA, isBorder);
            SetIndex(indexB, isBorder);
            SetIndex(indexC, isBorder);
        }

        // Return a duplicate of the array of real triangles.
        public int[] CopyTriangles()
        {
            int[] triangles = new int[Size];

            for (int i = 0; i < Size; i++)
                triangles[i] = mainTriangles[i];

            return triangles;
        }
    }
}
