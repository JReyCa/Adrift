using UnityEngine;
using UnityEditor;

namespace ProceduralGen
{
    [CustomEditor(typeof(ProceduralGeneratorBase), true)]
    public class GeneratorEditor : Editor
    {
        ProceduralGeneratorBase generator;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawAllSettings();

            GUILayout.Space(15.0f);

            // Buttons!
            if (GUILayout.Button("Generate"))
                generator.Generate();

            if (GUILayout.Button("Save"))
                generator.Save(generator.GenerateSavePath());
        }

        // Put calls of DrawSettings in here so the settings appear above the buttons.
        protected virtual void DrawAllSettings() { }

        // Draw some settings and possibly call something if they're changed.
        public void DrawSettings(Object settings, ref Editor editor, ref bool foldout, System.Action callback = null)
        {
            if (settings == null)
                return;

            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();
                }

                if (check.changed && generator.autoUpdate)
                    callback?.Invoke();
            };
        }

        protected virtual void OnEnable()
        {
            generator = (ProceduralGeneratorBase)target;
        }
    }
}
