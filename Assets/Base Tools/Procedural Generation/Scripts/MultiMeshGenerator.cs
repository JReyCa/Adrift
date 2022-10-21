using SaveTools;
using UnityEngine;
using UnityEditor;

namespace ProceduralGen
{
    public abstract class MultiMeshGenerator : ProceduralGeneratorBase
    {
        public override void Generate()
        {
            base.Generate();
            Mesh[] meshes = GenerateMeshes();

            // Make sure the preview has enough children to hold all the meshes.
            Transform previewT = preview.transform;
            for (int i = 0; i < meshes.Length; i++)
            {
                if (previewT.childCount < i + 1)
                {
                    GameObject childObject = new("Multi Mesh Part " + i);
                    childObject.transform.SetParent(previewT);
                    childObject.transform.localPosition = Vector3.zero;
                    childObject.AddComponent<MeshFilter>();
                    childObject.AddComponent<MeshRenderer>();
                }
            }

            UpdateMeshes(meshes);
            UpdateMaterials();

            while (previewT.childCount > meshes.Length)
            {
                DestroyImmediate(previewT.GetChild(previewT.childCount - 1));
            }
        }

        public void UpdateMeshes(Mesh[] meshes)
        {
            Transform previewT = preview.transform;

            for (int i = 0; i < previewT.childCount; i++)
            {
                MeshFilter meshFilter = previewT.GetChild(i).GetComponent<MeshFilter>();

                if (meshes.Length > i)
                    meshFilter.sharedMesh = meshes[i];
                else if (meshes.Length > 0)
                    meshFilter.sharedMesh = meshes[^1];
            }
        }

        public void UpdateMaterials()
        {
            Material[] materials = GenerateMaterials();
            Transform previewT = preview.transform;

            for (int i = 0; i < previewT.childCount; i++)
            {
                MeshRenderer renderer = previewT.GetChild(i).GetComponent<MeshRenderer>();

                if (materials.Length > i)
                    renderer.sharedMaterial = materials[i];
                else if (materials.Length > 0)
                    renderer.sharedMaterial = materials[0];
                else
                    renderer.sharedMaterial = new(Shader.Find("Standard"));
            }
        }

        protected override GameObject ClonePreview()
        {
            GameObject clone = base.ClonePreview();
            Transform previewT = preview.transform;

            for (int i = 0; i < previewT.childCount; i++)
            {
                Transform childT = previewT.GetChild(i);

                // Create a clone of the child.
                GameObject childClone = new(childT.name);
                childClone.transform.SetParent(clone.transform);
                childClone.transform.localPosition = Vector3.zero;

                // Clone mesh filter and mesh renderer.
                CloneMeshFilterTo(childClone, childT.gameObject.GetComponent<MeshFilter>());
                CloneMeshRendererTo(childClone, childT.gameObject.GetComponent<MeshRenderer>());
            }

            return clone;
        }

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

        protected abstract Mesh[] GenerateMeshes();

        protected abstract Material[] GenerateMaterials();
    }
}
