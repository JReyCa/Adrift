using ProceduralGen;
using UnityEngine;
using UnityEditor;

namespace PlanetCreation
{
    [CustomEditor(typeof(PlanetCreator))]
    public class PlanetCreatorEditor : GeneratorEditor
    {
        private PlanetCreator creator;
        private Editor shapeEditor;
        private Editor materialEditor;

        protected override void DrawAllSettings()
        {
            GUILayout.Space(10f);
            DrawSettings(creator.shapeSettings, ref shapeEditor, ref creator.shapeSettingsFoldout, creator.Generate);
            DrawSettings(creator.materialSettings, ref materialEditor, ref creator.materialSettingsFoldout, creator.UpdateMaterials);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            creator = (PlanetCreator)target;
        }
    }
}
