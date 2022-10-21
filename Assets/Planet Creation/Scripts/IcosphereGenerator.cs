using ProceduralGen.MeshGen;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetCreation
{
    public static class IcosphereGenerator
    {
        // The angle in degrees that a vector rotates, going from the normal of one face to an adjacent face.
        private const float NORMAL_ANGLE_DELTA = 41.8103149f;

        // The distance from the centre of the whole shape to the centre of a face for a regular, convex icosahedron of side length 1.
        private const float IN_RADIUS = 0.755761f;

        // Return an array of meshes, each of which representing one "face" of an
        // icosahedron sphere (or icosphere, as they're known).
        public static Mesh[] Generate(int resolution, ShapeGenerator shapeGenerator)
        {
            Face[] faces = GenerateFaces();
            Mesh[] meshes = new Mesh[20];

            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i] = GenerateFaceMesh(resolution, faces[i], shapeGenerator);
            }

            return meshes;
        }

        // Returns the mesh of one triangle of a densified icosahedron that's also been ballooned out
        // into a sphere.
        private static Mesh GenerateFaceMesh(int resolution, Face face, ShapeGenerator shapeGenerator)
        {
            // Construct a basis for this and neighbouring triangles.
            Basis currentBasis = new(face.GetNormal(), (face.a - face.b).normalized, true);
            Basis[] borderingBases = CalculateBorderingBases(currentBasis);

            // Predetermine some handy values.
            float goldenRatio = Mathf.Sqrt(3f) / 2f;        // Helpful for getting the height of an equilateral triangle.

            float xStep = 1f / (resolution + 1);            // The amount that we move "horizontally" or "rightward" for each vertex.
            float zStep = goldenRatio / (resolution + 1);   // The amount that we move "vertically" or "upward" for each vertex.
            float xStart = 0f;

            int numVertRows = resolution + 2;   // The number of vertices per row is predictable.
            int vertIndex = 0;                  // Keep track of how many vertices we've made so far.
            int borderVertIndex = -1;           // Also keep track of vertices on the border, even though we don't render them. The
                                                // reason is that we'll need them to calculate normals properly at the end.

            IcoVertBuffer vertexBuffer = IcoVertBuffer.Create(resolution);

            // Keep track of the indices of vertices that lie past the three edges of the face.
            // The last index of each array corresponds to the corner of the border triangle,
            // which needs to be calculated a bit differently.
            int[] bottomBorderIndices = new int[resolution + 2];
            int bottomBorderCount = 0;

            int[] topRightBorderIndices = new int[resolution + 2];
            int topRightBorderCount = 0;

            int[] topLeftBorderIndices = new int[resolution + 2];
            int topLeftBorderCount = 0;
            
            // Iterate over each row of vertices in the triangle.
            for (int z = 0; z < numVertRows; z++)
            {
                int rowLength = numVertRows - z;    // Each successive row gets smaller as we go up the triangle.

                for (int x = 0; x < rowLength; x++)
                {
                    // Calculate the triangle as if it were laying flat on the ground.
                    float xPos = xStart + x * xStep - 0.5f;     // With a width of 1, -0.5 will centre the triangle.
                    float zPos = z * zStep - goldenRatio / 3f;  // With a height of 2/3 the golden ratio, subtracting a third will centre it.
                    Vector3 point = new(xPos, IN_RADIUS, zPos);

                    // Transform the vertex positions so the triangle faces the correct direction.
                    // Normalizing the point on the icosahedron will project it onto a sphere.
                    Vector3 pointOnIco = currentBasis.TransformPoint(point);
                    Vector3 pointOnSphere = pointOnIco.normalized;
                    vertexBuffer[vertIndex++] = shapeGenerator.Evaluate(pointOnSphere);

                    // Calculate the three corner border vertices.
                    if (x == 1 && z == 0)
                    {
                        // The vertex past the bottom right corner.
                        Vector3 cornerPoint0 = borderingBases[3].TransformPoint(point);
                        Vector3 cornerPointOnSphere0 = cornerPoint0.normalized;
                        bottomBorderIndices[^1] = borderVertIndex;
                        vertexBuffer[borderVertIndex--] = shapeGenerator.Evaluate(cornerPointOnSphere0);

                        // The vertex past the top.
                        Vector3 cornerPoint1 = borderingBases[4].TransformPoint(point);
                        Vector3 cornerPointOnSphere1 = cornerPoint1.normalized;
                        topRightBorderIndices[^1] = borderVertIndex;
                        vertexBuffer[borderVertIndex--] = shapeGenerator.Evaluate(cornerPointOnSphere1);

                        // The vertex past the bottom left corner.
                        Vector3 cornerPoint2 = borderingBases[5].TransformPoint(point);
                        Vector3 cornerPointOnSphere2 = cornerPoint2.normalized;
                        topLeftBorderIndices[^1] = borderVertIndex;
                        vertexBuffer[borderVertIndex--] = shapeGenerator.Evaluate(cornerPointOnSphere2);
                    }

                    // Calculate edge border vertices.
                    if (z == 1)
                    {
                        // The vertices past the bottom.
                        Vector3 borderPoint0 = borderingBases[0].TransformPoint(point);
                        Vector3 borderPointOnSphere0 = borderPoint0.normalized;
                        bottomBorderIndices[bottomBorderCount++] = borderVertIndex;
                        vertexBuffer[borderVertIndex--] = shapeGenerator.Evaluate(borderPointOnSphere0);

                        // The vertices past the top-right edge.
                        Vector3 borderPoint1 = borderingBases[1].TransformPoint(point);
                        Vector3 borderPointOnSphere1 = borderPoint1.normalized;
                        topRightBorderIndices[topRightBorderCount++] = borderVertIndex;
                        vertexBuffer[borderVertIndex--] = shapeGenerator.Evaluate(borderPointOnSphere1);

                        // The vertices past the top-left edge.
                        Vector3 borderPoint2 = borderingBases[2].TransformPoint(point);
                        Vector3 borderPointOnSphere2 = borderPoint2.normalized;
                        topLeftBorderIndices[topLeftBorderCount++] = borderVertIndex;
                        vertexBuffer[borderVertIndex--] = shapeGenerator.Evaluate(borderPointOnSphere2);
                    }
                }

                xStart += xStep * 0.5f; // As we go up the triangle, the row start is pushed to the right bit by bit.
            }
            
            // Compute triangles.
            IcoIndexBuffer indexBuffer = IcoIndexBuffer.Create(resolution);
            int numTriRows = numVertRows - 1;
            vertIndex = 0;  // Reset the vert index so we can go over them again.

            for (int y = 0; y < numTriRows; y++)
            {
                int vertRowLength = numVertRows - y;

                for (int x = 0; x < vertRowLength; x++)
                {
                    // Within this block, "real" (eventually rendered) vertices are considered.
                    if (x < vertRowLength - 1)
                    {
                        // Triangles pointing up
                        indexBuffer.SetTriangle(vertIndex, vertIndex + vertRowLength, vertIndex + 1);

                        // Triangulate bottom border (where triangles point down).
                        if (y == 0)
                        {
                            indexBuffer.SetTriangle(vertIndex, vertIndex + 1, bottomBorderIndices[x], true);
                        }

                        // Triangles pointing down
                        if (x > 0)
                        {
                            indexBuffer.SetTriangle(vertIndex, vertIndex + vertRowLength - 1, vertIndex + vertRowLength);

                            //Triangulate bottom border(where triangles point up).
                            if (y == 0)
                            {

                                indexBuffer.SetTriangle(vertIndex, bottomBorderIndices[x], bottomBorderIndices[x - 1], true);
                            }
                        }
                    }

                    // Triangulate top left border.
                    if (x == 0)
                    {
                        indexBuffer.SetTriangle(vertIndex, topLeftBorderIndices[y], vertIndex + vertRowLength, true);
                        if (y > 0)
                        {
                            indexBuffer.SetTriangle(vertIndex, topLeftBorderIndices[y - 1], topLeftBorderIndices[y], true);
                        }

                        // Triangulate the bottom left corner.
                        if (y == 0)
                        {
                            indexBuffer.SetTriangle(vertIndex, topLeftBorderIndices[^1], topLeftBorderIndices[0], true);
                            indexBuffer.SetTriangle(vertIndex, bottomBorderIndices[0], topLeftBorderIndices[^1], true);
                        }
                    }

                    // Triangulate top right border
                    if (x == vertRowLength - 1)
                    {
                        indexBuffer.SetTriangle(vertIndex, vertIndex + vertRowLength - 1, topRightBorderIndices[y], true);

                        if (y > 0)
                        {
                            indexBuffer.SetTriangle(vertIndex, topRightBorderIndices[y], topRightBorderIndices[y - 1], true);
                        }

                        // Triangulate the bottom right corner.
                        if (y == 0)
                        {
                            indexBuffer.SetTriangle(vertIndex, topRightBorderIndices[y], bottomBorderIndices[^1], true);
                            indexBuffer.SetTriangle(vertIndex, bottomBorderIndices[^1], bottomBorderIndices[x - 1], true);
                        }
                    }

                    vertIndex += 1;
                }
            }

            // Finally, get those last two border triangles at the top.
            indexBuffer.SetTriangle(vertIndex, topRightBorderIndices[^1], topRightBorderIndices[^2], true);
            indexBuffer.SetTriangle(vertIndex, topLeftBorderIndices[^2], topRightBorderIndices[^1], true);

            // Compute normals
            Vector3[] normals = new Vector3[vertexBuffer.Size];
            
            // First deal with the triangles that we'll eventually render.
            for (int i = 0; i < indexBuffer.Size; i += 3)
            {
                int indexA = indexBuffer[i];
                int indexB = indexBuffer[i + 1];
                int indexC = indexBuffer[i + 2];
                
                Vector3 a = vertexBuffer[indexA];
                Vector3 b = vertexBuffer[indexB];
                Vector3 c = vertexBuffer[indexC];

                Vector3 triNormal = GetTriNormal(a, b, c);

                normals[indexA] += triNormal;
                normals[indexB] += triNormal;
                normals[indexC] += triNormal;
            }

            // Now deal with the "imaginary" border triangles.
            // This is only necessary because a list of only "real" triangles will be needed at the end.
            for (int i = -1; i >= -indexBuffer.BorderSize; i -= 3)
            {
                int indexA = indexBuffer[i];
                int indexB = indexBuffer[i - 1];
                int indexC = indexBuffer[i - 2];

                Vector3 a = vertexBuffer[indexA];
                Vector3 b = vertexBuffer[indexB];
                Vector3 c = vertexBuffer[indexC];

                Vector3 triNormal = GetTriNormal(a, b, c);

                if (indexA >= 0) normals[indexA] += triNormal;
                if (indexB >= 0) normals[indexB] += triNormal;
                if (indexC >= 0) normals[indexC] += triNormal;
            }

            // Normalize the normals!
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i].Normalize();
            }

            // Phew, we're done.
            Mesh mesh = new()
            {
                vertices = vertexBuffer.CopyVertices(),
                triangles = indexBuffer.CopyTriangles(),
                normals = normals
            };

            return mesh;
        }

        private static Vector3 GetTriNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;

            return Vector3.Cross(ab, ac).normalized;
        }

        // Determine the vertices of a convex, regular icosahedron,
        // sorting them into an array of faces.
        private static Face[] GenerateFaces()
        {
            float goldenRatio = (1 + Mathf.Sqrt(5f)) * 0.5f;
            float halfRatio = goldenRatio * 0.5f;

            // xz quad
            Vector3 negXNegZ = new(-halfRatio, 0f, -0.5f);
            Vector3 negXPosZ = new(-halfRatio, 0f, 0.5f);
            Vector3 posXPosZ = new(halfRatio, 0f, 0.5f);
            Vector3 posXNegZ = new(halfRatio, 0f, -0.5f);

            // yz quad
            Vector3 negYNegZ = new(0f, -0.5f, -halfRatio);
            Vector3 posYNegZ = new(0f, 0.5f, -halfRatio);
            Vector3 posYPosZ = new(0f, 0.5f, halfRatio);
            Vector3 negYPosZ = new(0f, -0.5f, halfRatio);

            // xy quad
            Vector3 negXNegY = new(-0.5f, -halfRatio, 0f);
            Vector3 negXPosY = new(-0.5f, halfRatio, 0f);
            Vector3 posXPosY = new(0.5f, halfRatio, 0f);
            Vector3 posXNegY = new(0.5f, -halfRatio, 0f);

            // Sort the 12 points into the 20 triangles of an icosahedron.
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

        // Calculate the basis needed for each set of bordering vertices.
        // Index 0-2 are for the edges, and 3-5 for one extra point each at the corners.
        private static Basis[] CalculateBorderingBases(Basis currentBasis)
        {
            Basis[] bases = new Basis[6];

            // Axes about which we will rotate the current face's basis to get the bases of adjacent faces.
            Vector3 edgeHinge0 = currentBasis.tangent;
            Vector3 edgeHinge1 = Quaternion.AngleAxis(-30, currentBasis.normal) * currentBasis.cotangent;
            Vector3 edgeHinge2 = Quaternion.AngleAxis(30, currentBasis.normal) * currentBasis.cotangent;

            // Bases 0-2 are for the three faces who share an edge with the current face.
            // For orientation, consider the "bottom" of the current face to be the edge aligned with the tangent.
            bases[0] = currentBasis.RotateByAxis(-NORMAL_ANGLE_DELTA, edgeHinge0, true);    // For the face "below" the current x axis (or tangent)
            bases[1] = currentBasis.RotateByAxis(-NORMAL_ANGLE_DELTA, edgeHinge1, true);    // "Up and to the right"
            bases[1] = bases[1].RotateByAxis(-120, bases[1].normal);
            bases[2] = currentBasis.RotateByAxis(NORMAL_ANGLE_DELTA, edgeHinge2, true);     // "Up and to the left"
            bases[2] = bases[2].RotateByAxis(120, bases[2].normal);
            bases[2] = new(bases[2].normal, -bases[2].tangent, bases[2].cotangent);

            // Axes about which we will rotate the edge bases to obtain the bases of faces that are only
            // connected to the current face by a point.
            Vector3 cornerHinge0 = Quaternion.AngleAxis(30, bases[0].normal) * bases[0].cotangent;
            Vector3 cornerHinge1 = Quaternion.AngleAxis(30, bases[1].normal) * bases[1].cotangent;
            Vector3 cornerHinge2 = Quaternion.AngleAxis(30, bases[2].normal) * bases[2].cotangent;

            // Bases 3-5 are for three faces who share only a point with the current face.
            // They are needed to finish calculating the normals for those three points.
            bases[3] = bases[0].RotateByAxis(NORMAL_ANGLE_DELTA, cornerHinge0); // "Bottom right" corner
            bases[3] = bases[3].RotateByAxis(60, bases[3].normal);
            bases[4] = bases[1].RotateByAxis(NORMAL_ANGLE_DELTA, cornerHinge1); // "Top" corner
            bases[4] = bases[4].RotateByAxis(60, bases[4].normal);
            bases[5] = bases[2].RotateByAxis(NORMAL_ANGLE_DELTA, cornerHinge2); // "Bottom left" corner
            bases[5] = bases[5].RotateByAxis(240, bases[5].normal, true);

            return bases;
        }

        private class IcoVertBuffer : VertexBuffer
        {
            public int[] border0Indices;
            public int[] border1Indices;
            public int[] border2Indices;

            private IcoVertBuffer(int size, int borderSize) : base(size, borderSize) { }

            public static IcoVertBuffer Create(int resolution)
            {
                // # of vertices = 3 + 3n + (n - 1)!
                // where n is the resolution number.
                int vertCount = 3 + 3 * resolution;
                for (int i = 1; i <= resolution - 1; i++)
                    vertCount += i;

                // # of border vertices = 6(n + 1)
                int borderVertCount = 6 * (resolution + 1);

                return new IcoVertBuffer(vertCount, borderVertCount);
            }
        }

        private class IcoIndexBuffer : IndexBuffer
        {
            private IcoIndexBuffer(int size, int borderSize) : base(size, borderSize) { }

            public static IcoIndexBuffer Create(int resolution)
            {
                // # of indices = 3 * sumnation[t=1, t=n+1](2t - 1)
                // where n is the resolution number, t is the current sumnation index,
                // and the brackets indicate the inclusive range.
                int indexCount = 0;

                for (int i = 1; i <= resolution + 1; i++)
                    indexCount += 2 * i - 1;

                indexCount *= 3;

                // # of border indices = 3(9 + 6n)
                int borderCount = 3 * (9 + 6 * resolution);

                return new IcoIndexBuffer(indexCount, borderCount);
            }
        }

        public struct Basis
        {
            public readonly Vector3 normal;
            public readonly Vector3 tangent;
            public readonly Vector3 cotangent;

            public Basis(Vector3 normal, Vector3 tangent, Vector3 cotangent)
            {
                this.normal = normal;
                this.tangent = tangent;
                this.cotangent = cotangent;
            }

            public Basis(Vector3 normal, Vector3 tangent, bool flip = false)
                : this(normal, tangent, flip ? Vector3.Cross(tangent, normal) : Vector3.Cross(normal, tangent)) { }

            public Vector3 TransformPoint(Vector3 point)
            {
                Vector3 rightPos = point.x * tangent;
                Vector3 forwardPos = point.z * cotangent;
                Vector3 upPos = point.y * normal;

                return forwardPos + rightPos + upPos;
            }

            public Basis RotateByAxis(float degrees, Vector3 axis, bool flip = false)
            {
                Quaternion rotation = Quaternion.AngleAxis(degrees, axis);
                Vector3 rotatedNormal = rotation * normal;
                Vector3 rotatedTangent = rotation * tangent;
                Vector3 rotatedCotangent = rotation * cotangent;

                if (flip)
                    rotatedCotangent *= -1;

                return new Basis(rotatedNormal, rotatedTangent, rotatedCotangent);
            }
        }

        private struct Face
        {
            public Vector3 a;
            public Vector3 b;
            public Vector3 c;

            public Vector3 Centre => (a + b + c) / 3f;

            public Face(Vector3 a, Vector3 b, Vector3 c)
            {
                this.a = a;
                this.b = b;
                this.c = c;
            }

            public Vector3 GetNormal()
            {
                Vector3 ab = b - a;
                Vector3 ac = c - a;
                
                return Vector3.Cross(ab, ac).normalized;
            }

            public override string ToString()
            {
                return "a: " + a + ", b: " + b + ", c:" + c;
            }
        }
    }
}
