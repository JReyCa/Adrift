using SaveTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProceduralGen
{
    public abstract class ChunkedMeshGenerator : ProceduralGeneratorBase
    {
        #region Inspector Variables
        [Header("Chunks")]
        [SerializeField]
        [Tooltip("The number of chunks along the x, y, and z axes.")]
        [Min(1)]
        protected Vector2Int chunkCounts = new(3, 3);
        #endregion

        public override void Generate()
        {
            base.Generate();

            Vector3 chunkOffset = CalculateSpawnDistance();
            Vector3 centreOffset = new Vector3(chunkOffset.x * (chunkCounts.x - 1), 0.0f,
                                               chunkOffset.y * (chunkCounts.y - 1)) * 0.5f;

            for (int z = 0; z < chunkCounts.y; z++)
            {
                for (int x = 0; x < chunkCounts.x; x++)
                {
                    Vector2Int gridPos = new(x, z);
                    GameObject chunk = new("Chunk" + gridPos);

                    Transform chunkT = chunk.transform;
                    chunkT.SetParent(preview.transform);
                    chunkT.localPosition = new Vector3(chunkOffset.x * x, 0.0f, chunkOffset.z * z) - centreOffset;

                    MeshFilter chunkFilter = chunk.AddComponent<MeshFilter>();
                    MeshRenderer chunkRenderer = chunk.AddComponent<MeshRenderer>();

                    object data = GenerateDataInput();
                    chunkFilter.sharedMesh = GenerateChunkMesh(gridPos, data);
                    chunkRenderer.sharedMaterial = GenerateChunkMaterial(gridPos, data);

                    ModifyChunk(chunk);
                }
            }
        }

        protected override GameObject ClonePreview()
        {
            GameObject clone = base.ClonePreview();

            Transform previewT = preview.transform;
            for (int i = 0; i < previewT.childCount; i++)
            {
                Transform chunkPreviewT = previewT.GetChild(i);
                GameObject chunkClone = new(chunkPreviewT.name);

                Transform chunkCloneT = chunkClone.transform;
                chunkCloneT.SetParent(clone.transform);
                chunkCloneT.localPosition = chunkPreviewT.localPosition;

                // Clone the chunk's mesh.
                Mesh meshPreview = chunkPreviewT.GetComponent<MeshFilter>().sharedMesh;
                Mesh meshClone = new()
                {
                    vertices = meshPreview.vertices,
                    uv = meshPreview.uv,
                    triangles = meshPreview.triangles,
                    normals = meshPreview.normals,
                    bounds = meshPreview.bounds
                };

                chunkClone.AddComponent<MeshFilter>().sharedMesh = meshClone;

                // Clone the chunk's material.
                Material materialPreview = chunkPreviewT.GetComponent<MeshRenderer>().sharedMaterial;
                Material materialClone = new(materialPreview.shader);
                materialClone.CopyPropertiesFromMaterial(materialPreview);

                chunkClone.AddComponent<MeshRenderer>().sharedMaterial = materialClone;
            }

            return clone;
        }

        // Override to save the meshes and materials of each chunk.
        protected override void SaveAssets(GameObject clone, FilePath path)
        {
            FilePath meshFolderPath = path.CreateExtendedPath("Meshes");
            AssetDatabase.CreateFolder(path.ToString(), meshFolderPath.End);

            FilePath materialFolderPath = path.CreateExtendedPath("Materials");
            AssetDatabase.CreateFolder(path.ToString(), materialFolderPath.End);

            // Iterate over each chunk.
            int chunkCount = clone.transform.childCount;
            for (int i = 0; i < chunkCount; i++)
            {
                Transform chunkT = clone.transform.GetChild(i);

                // Save the mesh.
                Mesh mesh = chunkT.GetComponent<MeshFilter>().sharedMesh;
                FilePath meshPath = meshFolderPath.CreateExtendedPath(saveName + "_" + chunkT.name + "_mesh.asset");
                
                string rawMeshPath = AssetDatabase.GenerateUniqueAssetPath(meshPath.ToString());
                AssetDatabase.CreateAsset(mesh, rawMeshPath);

                // Save the material.
                Material material = chunkT.GetComponent<MeshRenderer>().sharedMaterial;
                FilePath materialPath = materialFolderPath.CreateExtendedPath(saveName + "_" + chunkT.name + "_material.mat");
                string rawMaterialPath = AssetDatabase.GenerateUniqueAssetPath(materialPath.ToString());
                AssetDatabase.CreateAsset(material, rawMaterialPath);
            }

            FilePath prefabPath = path.CreateExtendedPath(saveName + "_prefab.prefab");
            string rawPrefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath.ToString());
            PrefabUtility.SaveAsPrefabAsset(clone, rawPrefabPath);
        }

        protected abstract Vector3 CalculateSpawnDistance();

        protected abstract Mesh GenerateChunkMesh(Vector2Int gridPos, object dataInput);

        protected virtual void ModifyChunk(GameObject chunk) { }

        protected virtual Material GenerateChunkMaterial(Vector2Int gridPos, object dataInput) => new(Shader.Find("Standard"));

        protected virtual object GenerateDataInput() => null;
    }
}
