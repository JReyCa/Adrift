using SaveTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProceduralGen
{
    public abstract class MeshGenerator : ProceduralGeneratorBase
    {
        // Add a mesh and a material to the game object.
        public override void Generate()
        {
            base.Generate();

            MeshFilter meshFilter = ConfirmComponent<MeshFilter>(preview);
            meshFilter.sharedMesh = GenerateMesh();

            MeshRenderer renderer = ConfirmComponent<MeshRenderer>(preview);
            renderer.sharedMaterial = GenerateMaterial();
        }

        // Create a deep clone of the preview object, including its mesh and material data.
        protected override GameObject ClonePreview()
        {
            GameObject clone = base.ClonePreview();
            MeshFilter filterClone = clone.AddComponent<MeshFilter>();
            MeshRenderer rendererClone = clone.AddComponent<MeshRenderer>();

            // Clone the preview mesh.
            Mesh meshPreview = preview.GetComponent<MeshFilter>().sharedMesh;
            Mesh meshClone = new()
            {
                vertices = meshPreview.vertices,
                uv = meshPreview.uv,
                triangles = meshPreview.triangles,
                normals = meshPreview.normals,
                bounds = meshPreview.bounds
            };

            // Clone the preview material.
            Material materialPreview = preview.GetComponent<MeshRenderer>().sharedMaterial;
            Material materialClone = new(materialPreview.shader);
            materialClone.CopyPropertiesFromMaterial(materialPreview);

            // Assign mesh and material to the prefab.
            filterClone.sharedMesh = meshClone;
            rendererClone.sharedMaterial = materialClone;

            return clone;
        }

        protected override void SaveAssets(GameObject clone, FilePath path)
        {
            // Save the mesh.
            FilePath meshPath = path.CreateExtendedPath(saveName + "_mesh.asset");
            string rawMeshPath = AssetDatabase.GenerateUniqueAssetPath(meshPath.ToString());
            AssetDatabase.CreateAsset(clone.GetComponent<MeshFilter>().sharedMesh, rawMeshPath);

            // Save the material.
            FilePath materialPath = path.CreateExtendedPath(saveName + "_material.mat");
            string rawMaterialPath = AssetDatabase.GenerateUniqueAssetPath(materialPath.ToString());
            AssetDatabase.CreateAsset(clone.GetComponent<MeshRenderer>().sharedMaterial, rawMaterialPath);

            // Save the prefab.
            FilePath prefabPath = path.CreateExtendedPath(saveName + "_prefab.prefab");
            string rawPrefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath.ToString());
            PrefabUtility.SaveAsPrefabAsset(clone, rawPrefabPath);
        }

        protected abstract Mesh GenerateMesh();

        protected virtual Material GenerateMaterial() => new(Shader.Find("Standard"));
    }
}
