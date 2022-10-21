using ProceduralGen;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace PlanetCreation
{
    [ExecuteInEditMode]
    public class PlanetCreator : MultiMeshGenerator
    {
        #region Inspector Variables
        [Tooltip("The resolution of the mesh.")]
        [SerializeField]
        [Range(1, 150)]
        private int resolution;

        [Tooltip("Splits each chunk into 4. (Not implemented yet)")]
        public bool chunkify = false;

        public ShapeSettings shapeSettings;
        public MaterialSettings materialSettings;

        [Tooltip("This is just for debugging purposes.")]
        public bool showGizmos = false;
        #endregion

        [HideInInspector]
        public ComputeShader computeShader;

        [HideInInspector]
        public bool shapeSettingsFoldout = false;

        [HideInInspector]
        public bool materialSettingsFoldout = false;

        protected override Mesh[] GenerateMeshes()
        {
            ShapeGenerator shapeGenerator = new(shapeSettings);
            Mesh[] meshes = IcosphereGeneratorGPU.Generate(computeShader, resolution * 2 - 1, shapeGenerator);

            return meshes;
        }

        protected override Material[] GenerateMaterials()
        {
            int numLevels = materialSettings.levels.Length;
            float[] thresholds = new float[numLevels];

            for (int i = 0; i < numLevels; i++)
            {
                var level = materialSettings.levels[i];
                thresholds[i] = level.threshold;
            }
            
            Material m = new(Shader.Find("Planet Creation/SpaceTerrain"));
            m.SetInteger("_NumTerrainLevels", numLevels);
            m.SetFloat("_MinRadius", shapeSettings.radius);
            m.SetFloat("_MaxRadius", shapeSettings.GetMaxRadius());
            m.SetTexture("_ThresholdData", materialSettings.GetThresholdData());
            m.SetTexture("_AlbedoTextures", materialSettings.GetAlbedoTextures());
            m.SetTexture("_NormalTextures", materialSettings.GetNormalTextures());
            
            return new Material[] { m };
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos)
                return;

            
        }
    }
}
