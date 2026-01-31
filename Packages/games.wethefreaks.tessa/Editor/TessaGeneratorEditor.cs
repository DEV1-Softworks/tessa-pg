using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TessaGenerator))]
public class TessaGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8f);

        if (GUILayout.Button("Generate Sample Level"))
        {
            var generator = (TessaGenerator)target;
            generator.GenerateLevel();
            EditorUtility.SetDirty(generator);
            SceneView.RepaintAll();
        }
    }
}
