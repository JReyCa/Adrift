using SaveTools;
using System.Collections;
using UnityEngine;
using UnityEditor;

namespace ProceduralGen
{
    public abstract class ProceduralGeneratorBase : MonoBehaviour
    {
        #region Inspector Variables
        [Header("Saving")]
        [SerializeField]
        [Tooltip("The path, relative to the Assets folder, where saved game objects will go.")]
        protected string saveFolder = "";

        [SerializeField]
        [Tooltip("The name of the subfolder where a specific save will go. This is also used " +
            "to name the newly created assets.")]
        protected string saveName = "Unnamed";

        [Header("Generation Settings")]
        [Tooltip("Toggle this on if you want to re-generate whenever the inspector is changed. " +
            "This can be slow if the generation method is intensive.")]
        public bool autoUpdate = false;
        #endregion

        [HideInInspector]
        [SerializeField]
        protected GameObject preview;

        // This method generates a game object, and should be overridden (i.e. added to).
        public virtual void Generate()
        {
            if (!preview)
            {
                preview = new GameObject("Generated Preview");
                preview.transform.SetParent(transform);
            }
        }

        // This method saves the generated game object as a prefab.
        // -------------------------------------------------------------------
        // path ->  The path to the folder where the game object will be saved
        public virtual void Save(FilePath path)
        {
            if (!preview)
            {
                Debug.LogWarning("You need to generate a preview before you can save it!");
                return;
            }

            GameObject clone = ClonePreview();
            SaveAssets(clone, path);
            DestroyImmediate(clone);

            Debug.Log("\"" + saveName + "\" was saved to " + path);
        }

        // Generate a path to save the previewed object at as a new prefab.
        // --------------------------------------------------------------------------------
        // overwrite    ->  Set this to true if you want to overwrite an existing save path
        public FilePath GenerateSavePath(bool overwrite = false)
        {
            FilePath path = new("Assets");

            if (saveFolder != null && saveFolder != "")
                path.Append(saveFolder);

            // Ensure that the folder hierarchy exists.
            for (int i = 1; i < path.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(path.GetPartialPath(i)))
                    AssetDatabase.CreateFolder(path.GetPartialPath(i - 1), path[i]);
            }

            // Ensure that the folder for this save has a proper name.
            string uniqueName = saveName;
            string uniquePath = path.ToString() + saveName;
            int uniqueIndex = 0;

            while (AssetDatabase.IsValidFolder(uniquePath) && !overwrite)
            {
                uniqueName = saveName + (++uniqueIndex);
                uniquePath = path.ToString() + uniqueName;
            }

            path.Append(uniqueName);
            AssetDatabase.CreateFolder(path.GetPartialPath(path.Length - 2), uniqueName);

            return path;
        }

        // Override this method if you want to save the clone differently.
        // -----------------------------------------------------------------
        // clone    ->  A temporary deep copy of the preview game object
        // path     ->  The folder that all the save info will be written to
        protected abstract void SaveAssets(GameObject clone, FilePath path);

        // Create a temporary deep copy of the preview to define the prefab.
        // Override this if you want to clone the preview differently.
        protected virtual GameObject ClonePreview()
        {
            GameObject clone = new(saveName);

            return clone;
        }

        // Gets a component on a game object, and adds it first if it isn't there.
        protected T ConfirmComponent<T>(GameObject previewObject) where T : Component
        {
            T component = previewObject.GetComponent<T>();

            if (!component)
                component = previewObject.AddComponent<T>();

            return component;
        }

        // Adds a deep copy of a mesh filter to another game object.
        protected void CloneMeshFilterTo(GameObject clone, MeshFilter originalFilter)
        {
            Mesh originalMesh = originalFilter.sharedMesh;
            Mesh clonedMesh = new()
            {
                vertices = originalMesh.vertices,
                triangles = originalMesh.triangles,
                normals = originalMesh.normals,
                uv = originalMesh.uv
            };

            clone.AddComponent<MeshFilter>().sharedMesh = clonedMesh;
        }

        // Adds a deep copy of a mesh renderer to another game object.
        protected void CloneMeshRendererTo(GameObject clone, MeshRenderer originalRenderer)
        {
            Material originalMaterial = originalRenderer.sharedMaterial;
            Material clonedMaterial = new(originalMaterial.shader);
            clonedMaterial.CopyPropertiesFromMaterial(originalMaterial);

            clone.AddComponent<MeshRenderer>().sharedMaterial = clonedMaterial;
        }
    }
}
