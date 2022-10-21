using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetCreation
{
    public static class IcosphereGeneratorGPU
    {
        private const float GOLDEN_RATIO = 1.618033988749f;
        private const float NORMAL_ANGLE_DELTA = 41.8103149f;

        public static Mesh[] Generate(ComputeShader icoCompute, int resolution, ShapeGenerator shapeGenerator, bool chunkify = false)
        {
            // Predict the size of each buffer based on the resolution number.
            // This is the implementation of a bunch of math that was done on paper.
            int vertCount = 3 + 3 * resolution + resolution * (resolution - 1) / 2;
            int borderVertCount = 3 + 3 * (resolution + 1);
            int indexCount = 3 * (resolution + 1) * (resolution + 1);
            int borderIndexCount = 3 * (6 * resolution + 9);

            // Create mesh data buffers.
            ComputeBuffer vertexBuffer = new(vertCount, 3 * sizeof(float));
            ComputeBuffer borderVertexBuffer = new(borderVertCount, 3 * sizeof(float));
            ComputeBuffer indexBuffer = new(indexCount, sizeof(int));
            ComputeBuffer borderIndexBuffer = new(borderIndexCount, sizeof(int));
            
            // Set buffers to the compute shader, each to the needed kernels.
            icoCompute.SetBuffer(0, "_Vertices", vertexBuffer);
            icoCompute.SetBuffer(0, "_BorderVertices", borderVertexBuffer);

            icoCompute.SetBuffer(1, "_Triangles", indexBuffer);
            icoCompute.SetBuffer(1, "_BorderTriangles", borderIndexBuffer);

            // Figure out how many threads will be needed.               
            int vertsPerEdge = resolution + 2;                              // # of vertices per edge of a face.
            int threadGroupsX = Mathf.CeilToInt((vertsPerEdge + 1) / 8f);   // The GPU threads used will be a threadCountX by threadCountY grid.
            int threadGroupsY = Mathf.CeilToInt(vertsPerEdge / 8f);

            // Set common data.
            icoCompute.SetInt("_Resolution", resolution);
            icoCompute.SetFloat("_XStep", 1f / (resolution + 1));
            icoCompute.SetFloat("_ZStep", (Mathf.Sqrt(3f) * 0.5f) / (resolution + 1));
            
            // Get the vertices and basis vectors (for each face) for a regular icosahedron.
            Face[] faces = PrecalculateIsoFaces();

            Mesh[] meshes = new Mesh[20];

            // Iterate over the faces of the icosahedron.
            for (int i = 0; i < 20; i++)
            {
                Basis[] bases = CalculateBases(faces[i]);
                ComputeBuffer basisBuffer = new(21, sizeof(float) * 3); // 7 bases, 3 vectors each, so 21 vectors.
                basisBuffer.SetData(bases);

                icoCompute.SetBuffer(0, "_BasisVectors", basisBuffer);
                icoCompute.Dispatch(0, threadGroupsX, threadGroupsY, 1);    // Compute vertices.
                icoCompute.Dispatch(1, threadGroupsX, threadGroupsY, 1);    // Compute triangles.

                // Arrays to pull data into from the GPU.
                Vector3[] vertices = new Vector3[vertCount];
                Vector3[] borderVertices = new Vector3[borderVertCount];
                int[] triangles = new int[indexCount];
                int[] borderTriangles = new int[borderIndexCount];

                vertexBuffer.GetData(vertices);
                borderVertexBuffer.GetData(borderVertices);
                indexBuffer.GetData(triangles);
                borderIndexBuffer.GetData(borderTriangles);

                basisBuffer.Dispose();

                for (int j = 0; j < vertCount; j++)
                {
                    vertices[j] = shapeGenerator.Evaluate(vertices[j]);
                }

                for (int j = 0; j < borderVertCount; j++)
                {
                    borderVertices[j] = shapeGenerator.Evaluate(borderVertices[j]);
                }

                Vector3[] normals = CalculateNormals(vertices, borderVertices, triangles, borderTriangles);

                MeshData meshData = new()
                {
                    vertices = vertices,
                    normals = normals,
                    triangles = triangles
                };

                Mesh mesh = new()
                {
                    vertices = vertices,
                    triangles = triangles,
                    normals = normals
                };
                ChunkifyMesh(meshData, resolution);
                meshes[i] = mesh;
            }

            // Throw out all the buffers when we're done.
            vertexBuffer.Dispose();
            borderVertexBuffer.Dispose();
            indexBuffer.Dispose();
            borderIndexBuffer.Dispose();

            return meshes;
        }

        // Calculate all the faces of a regular, convex icosahedron.
        private static Face[] PrecalculateIsoFaces()
        {
            float halfLength = GOLDEN_RATIO * 0.5f;

            // Take three quads whose length/width ratio is the golden ratio. If you line up the quads
            // the right way, their 12 points (3 quads, 4 points each) form the points of an icosahedron.

            // xz quad
            Vector3 negXNegZ = new(-halfLength, 0f, -0.5f);
            Vector3 negXPosZ = new(-halfLength, 0f, 0.5f);
            Vector3 posXPosZ = new(halfLength, 0f, 0.5f);
            Vector3 posXNegZ = new(halfLength, 0f, -0.5f);

            // yz quad
            Vector3 negYNegZ = new(0f, -0.5f, -halfLength);
            Vector3 posYNegZ = new(0f, 0.5f, -halfLength);
            Vector3 posYPosZ = new(0f, 0.5f, halfLength);
            Vector3 negYPosZ = new(0f, -0.5f, halfLength);

            // xy quad
            Vector3 negXNegY = new(-0.5f, -halfLength, 0f);
            Vector3 negXPosY = new(-0.5f, halfLength, 0f);
            Vector3 posXPosY = new(0.5f, halfLength, 0f);
            Vector3 posXNegY = new(0.5f, -halfLength, 0f);

            // Sort the 12 points into the 20 triangles of an icosahedron.
            // (This was painful. Treasure these sorted points like gold.)
            Face[] faces = new Face[20];
            faces[0] = new(posYNegZ, negXPosY, posXPosY);
            faces[1] = new(posXPosY, posXNegZ, posYNegZ);
            faces[2] = new(posXNegZ, posXPosY, posXPosZ);
            faces[3] = new(posXPosZ, posXPosY, posYPosZ);
            faces[4] = new(posYPosZ, posXPosY, negXPosY);
            faces[5] = new(posYPosZ, negXPosY, negXPosZ);
            faces[6] = new(negXPosZ, negXPosY, negXNegZ);
            faces[7] = new(negXNegZ, negXPosY, posYNegZ);
            faces[8] = new(negYNegZ, posYNegZ, posXNegZ);
            faces[9] = new(posXNegZ, posXNegY, negYNegZ);
            faces[10] = new(posXNegZ, posXPosZ, posXNegY);
            faces[11] = new(posXPosZ, negYPosZ, posXNegY);
            faces[12] = new(posXPosZ, posYPosZ, negYPosZ);
            faces[13] = new(negYPosZ, posYPosZ, negXPosZ);
            faces[14] = new(negXNegY, negYNegZ, posXNegY);
            faces[15] = new(negXNegY, posXNegY, negYPosZ);
            faces[16] = new(negYPosZ, negXPosZ, negXNegY);
            faces[17] = new(negXNegY, negXPosZ, negXNegZ);
            faces[18] = new(negXNegZ, posYNegZ, negYNegZ);
            faces[19] = new(negXNegY, negXNegZ, negYNegZ);

            return faces;
        }

        private static Basis[] CalculateBases(Face face)
        {
            Basis localBasis = face.basis;

            Basis[] bases = new Basis[7];
            bases[0] = localBasis;

            // Axes about which we will rotate the current face's basis to get the bases of adjacent faces.
            Vector3 edgeHinge1 = localBasis.tangent;
            Vector3 edgeHinge2 = (face.c - face.a).normalized;
            Vector3 edgeHinge3 = (face.c - face.b).normalized;

            // Bases 1-3 are for the three faces who share an edge with the current face.
            // For orientation, consider the "bottom" of the current face to be the edge aligned with the tangent.
            bases[1] = localBasis.RotateByAxis(-NORMAL_ANGLE_DELTA, edgeHinge1, true);    // For the face "below" the current x axis (or tangent)
            bases[2] = localBasis.RotateByAxis(-NORMAL_ANGLE_DELTA, edgeHinge2, true);    // "Up and to the right"
            bases[2] = bases[2].RotateByNormal(-120);
            bases[3] = localBasis.RotateByAxis(NORMAL_ANGLE_DELTA, edgeHinge3, true);     // "Up and to the left"
            bases[3] = bases[3].RotateByNormal(120);
            bases[3] = new(-bases[3].tangent, bases[3].normal, bases[3].cotangent);

            // Axes about which we will rotate the edge bases to obtain the bases of faces that are only
            // connected to the current face by a point.
            Vector3 cornerHinge1 = Quaternion.AngleAxis(30, bases[1].normal) * bases[1].cotangent;
            Vector3 cornerHinge2 = Quaternion.AngleAxis(30, bases[2].normal) * bases[2].cotangent;
            Vector3 cornerHinge3 = Quaternion.AngleAxis(30, bases[3].normal) * bases[3].cotangent;

            // Bases 4-6 are for three faces who share only a point with the current face.
            // They are needed to finish calculating the normals for those three points.
            bases[4] = bases[1].RotateByAxis(NORMAL_ANGLE_DELTA, cornerHinge1); // "Bottom right" corner
            bases[4] = bases[4].RotateByNormal(60f);
            bases[5] = bases[2].RotateByAxis(NORMAL_ANGLE_DELTA, cornerHinge2); // "Top" corner
            bases[5] = bases[5].RotateByNormal(60f);
            bases[6] = bases[3].RotateByAxis(NORMAL_ANGLE_DELTA, cornerHinge3); // "Bottom left" corner
            bases[6] = bases[6].RotateByNormal(60f);

            return bases;
        }

        private static Vector3[] CalculateNormals(Vector3[] vertices, Vector3[] borderVertices, int[] triangles, int[] borderTriangles)
        {
            int numVerts = vertices.Length;
            Vector3[] normals = new Vector3[numVerts];

            for (int j = 0; j < triangles.Length; j += 3)
            {
                int indexA = triangles[j];
                int indexB = triangles[j + 1];
                int indexC = triangles[j + 2];

                Vector3 a = vertices[indexA];
                Vector3 b = vertices[indexB];
                Vector3 c = vertices[indexC];

                Vector3 normal = Vector3.Cross(b - a, c - a);

                normals[indexA] += normal;
                normals[indexB] += normal;
                normals[indexC] += normal;
            }

            for (int j = 0; j < borderTriangles.Length; j += 3)
            {
                int indexA = borderTriangles[j];
                int indexB = borderTriangles[j + 1];
                int indexC = borderTriangles[j + 2];

                Vector3 a = indexA >= 0 ? vertices[indexA] : borderVertices[-indexA - 1];
                Vector3 b = indexB >= 0 ? vertices[indexB] : borderVertices[-indexB - 1];
                Vector3 c = indexC >= 0 ? vertices[indexC] : borderVertices[-indexC - 1];

                Vector3 normal = Vector3.Cross(b - a, c - a);

                if (indexA >= 0) normals[indexA] += normal;
                if (indexB >= 0) normals[indexB] += normal;
                if (indexC >= 0) normals[indexC] += normal;
            }

            for (int j = 0; j < numVerts; j++)
            {
                normals[j] = normals[j].normalized;
            }

            return normals;
        }

        private static MeshData[] ChunkifyMesh(MeshData originalData, int resolution)
        {
            MeshData[] splitData = new MeshData[4];

            int subTriRes = (resolution + 1) / 2 - 1;
            int numSubVerts = 3 + 3 * subTriRes + subTriRes * (subTriRes - 1) / 2;
            //int numSubIndices
            
            for (int i = 0; i < 4; i++)
            {
                Vector3[] vertices = new Vector3[numSubVerts];
            }

            return splitData;
        }

        // A face of a regular, convex icosahedron, and therefore also an equilateral triangle.
        private struct Face
        {
            // The points "a", "b", and "c" are the three point defining the face.
            public readonly Vector3 a;
            public readonly Vector3 b;
            public readonly Vector3 c;

            // Imagine, if you will, that the triangle below is equilateral.
            // In the basis, the "tangent" vector is like the face's x-axis, aligned with
            // the edge from "b" to "a." The "cotangent" vector is like the face's
            // z-axis, going along from the base to point "c." The normal comes up at you.

            // Try to think of the face like it's lying flat on the ground, from the
            // perspective of its basis vectors.

            //
            //             c
            //            / \
            //           /   \
            //          /     \
            //         /       \
            //        /    ^    \
            //       /     |     \
            //      /      |      \   (x) normal (coming out at you)
            //     /       |       \
            //    /    cotangent    \  Note also that the vertices wind clockwise.
            //   /         |         \
            //  /          |          \
            // b-----------------------a
            // ---------tangent--------->

            public readonly Basis basis;

            public Face(Vector3 a, Vector3 b, Vector3 c)
            {
                this.a = a;
                this.b = b;
                this.c = c;

                Vector3 tangent = (a - b).normalized;
                Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
                basis = new(tangent, normal);
            }

            public Vector3[] Serialize()
            {
                List<Vector3> serialForm = new() { a, b, c };
                serialForm.AddRange(basis.Serialize());

                return serialForm.ToArray();
            }
        }

        private struct Basis
        {
            public readonly Vector3 tangent;    // To the "right", or the "x-axis".
            public readonly Vector3 normal;     // "Up", or the "y-axis".
            public readonly Vector3 cotangent;  // "Forward", or the "z-axis".

            public Basis(Vector3 tangent, Vector3 normal, Vector3 cotangent)
            {
                this.tangent = tangent;
                this.normal = normal;
                this.cotangent = cotangent;
            }

            public Basis(Vector3 tangent, Vector3 normal) : this(tangent, normal, Vector3.Cross(tangent, normal)) { }

            public Basis RotateByAxis(float degrees, Vector3 axis, bool flip = false)
            {
                Quaternion rotation = Quaternion.AngleAxis(degrees, axis);
                Vector3 rotatedNormal = rotation * normal;
                Vector3 rotatedTangent = rotation * tangent;
                Vector3 rotatedCotangent = rotation * cotangent;

                if (flip)
                    rotatedCotangent *= -1;

                return new Basis(rotatedTangent, rotatedNormal, rotatedCotangent);
            }

            public Basis RotateByNormal(float degrees, bool flip = false)
                => RotateByAxis(degrees, normal, flip);

            public Vector3[] Serialize()
                => new Vector3[] { tangent, normal, cotangent };
        }

        private struct MeshData
        {
            public Vector3[] vertices;
            public Vector3[] normals;
            public int[] triangles;
        }
    }
}
