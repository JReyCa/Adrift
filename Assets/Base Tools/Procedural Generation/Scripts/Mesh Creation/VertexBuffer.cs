using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralGen.MeshGen
{
    public class VertexBuffer
    {
        private Vector3[] mainVertices;
        private Vector3[] borderVertices;

        // Properties
        public Vector3 this[int i]
        {
            get { return GetVertex(i); }
            set { SetVertex(i, value); }
        }

        public int Size => mainVertices.Length;
        public int BorderSize => borderVertices.Length;

        // Constructor
        // -----------------------------------------------------------------
        // size         ->  The size of the main vertex array.
        // borderSize   ->  Optionally, also set a size for the border array.
        public VertexBuffer(int size, int borderSize = 0)
        {
            mainVertices = new Vector3[size];
            borderVertices = new Vector3[borderSize];
        }

        // Getter method
        // Positive indices access the main, or "real" vertices,
        // while negative indices access the border vertices (if used).
        // ------------------------------------------------------------
        // i    ->  The index in the vertex array.
        public Vector3 GetVertex(int i)
        {
            if (i >= 0)
                return mainVertices[i];
            else
                return borderVertices[-i - 1];
        }

        // Setter method
        // Positive indices access the main, or "real" vertices,
        // while negative indices access the border vertices (if used).
        // ------------------------------------------------------------
        // i    ->  The index in the vertex array.
        // v    ->  The vertex position to set.
        public void SetVertex(int i, Vector3 v)
        {
            if (i >= 0)
                mainVertices[i] = v;
            else
                borderVertices[-i - 1] = v;
        }

        // Return a duplicate of the array of real vertices.
        public Vector3[] CopyVertices()
        {
            Vector3[] vertices = new Vector3[Size];

            for (int i = 0; i < Size; i++)
                vertices[i] = mainVertices[i];

            return vertices;
        }
    }
}
