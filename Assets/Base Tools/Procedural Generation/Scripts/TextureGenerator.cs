using SaveTools;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace ProceduralGen
{
    public abstract class TextureGenerator : ProceduralGeneratorBase
    {
        #region Inspector Variables
        [Header("Texture")]
        [SerializeField]
        [Min(1)]
        protected int width = 10;

        [SerializeField]
        [Min(1)]
        protected int height = 10;
        #endregion

        public override void Generate()
        {
            base.Generate();

            MeshFilter meshFilter = ConfirmComponent<MeshFilter>(preview);
            meshFilter.sharedMesh = MeshWeaver.Quad(width * 0.1f, height * 0.1f);

            MeshRenderer renderer = ConfirmComponent<MeshRenderer>(preview);
            renderer.sharedMaterial = GenerateMaterial();
        }

        // Ignore the clone for this one. No need.
        protected override void SaveAssets(GameObject clone, FilePath path)
        {
            Texture2D tex = (Texture2D)preview.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_MainTex");
            FilePath texturePath = path.CreateExtendedPath(saveName + ".png");
            string rawTexturePath = AssetDatabase.GenerateUniqueAssetPath(texturePath.ToString());
            File.WriteAllBytes(rawTexturePath, tex.EncodeToPNG());

            Debug.Log("Writing texture to PNG in the target folder. This could take a few minutes.");
        }

        protected virtual Material GenerateMaterial()
        {
            Material m = new(Shader.Find("Standard"));
            m.SetTexture("_MainTex", GenerateTexture());

            return m;
        }

        protected abstract Texture2D GenerateTexture();
    }
}
