using Noise;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralGen
{
    public static class MeshWeaver
    {
        // Create a mesh for a quad.
        // -----------------------------------------------
        // width    ->  The length along the local x-axis.
        // height   ->  The length along the local y-axis.
        public static Mesh Quad(float width, float height)
        {
            Vector3[] vertices = new Vector3[4];
            Vector2[] uv = new Vector2[4];

            Vector3 offset = new(-width * 0.5f, -height * 0.5f, 0.0f);
            
            vertices[0] = new Vector3(0.0f, 0.0f, 0.0f) + offset;
            vertices[1] = new Vector3(0.0f, height, 0.0f) + offset;
            vertices[2] = new Vector3(width, height, 0.0f) + offset;
            vertices[3] = new Vector3(width, 0.0f, 0.0f) + offset;

            uv[0] = new(0.0f, 0.0f);
            uv[1] = new(0.0f, 1.0f);
            uv[2] = new(1.0f, 1.0f);
            uv[3] = new(1.0f, 0.0f);

            Mesh mesh = new()
            {
                vertices = vertices,
                uv = uv,
                triangles = Triangulation.Quad(0, 1, 2, 3).ToArray()
            };
            
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        // Create the mesh for a plane.
        // -----------------------------------------------------------------------
        // quadCountX       ->  The number of quads in the plane along the x-axis.
        // quadCountZ       ->  The number of quads in the plane along the z-axis.
        // vertexDistance   ->  How far apart the vertices are from each other.
        public static Mesh Plane(int quadCountX, int quadCountZ, float vertexDistance = 1.0f)
        {
            List<Vector3> vertices = new();
            List<Vector2> uv = new();

            float totalWidth = quadCountX * vertexDistance;
            float totalLength = quadCountZ * vertexDistance;

            Vector3 offset = new(totalWidth * 0.5f, 0.0f, totalLength * 0.5f);

            // Compute vertex positions and uv.
            for (int z = 0; z <= quadCountZ; z++)
            {
                for (int x = 0; x <= quadCountX; x++)
                {
                    float xPos = x * vertexDistance;
                    float zPos = z * vertexDistance;

                    vertices.Add(new Vector3(xPos, 0.0f, zPos) - offset);
                    uv.Add(new Vector2(xPos / totalWidth, zPos / totalLength));
                }
            }

            Mesh mesh = new()
            {
                vertices = vertices.ToArray(),
                uv = uv.ToArray(),
                triangles = Triangulation.Plane(quadCountX, quadCountZ).ToArray()
            };
            
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
