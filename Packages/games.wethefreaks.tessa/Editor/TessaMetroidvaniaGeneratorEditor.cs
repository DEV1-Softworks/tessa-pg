using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TessaMetroidvaniaGenerator))]
public class TessaMetroidvaniaGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8f);

        if (GUILayout.Button("Generate Sample Level"))
        {
            var generator = (TessaMetroidvaniaGenerator)target;
            generator.GenerateLevel();
            EditorUtility.SetDirty(generator);
            SceneView.RepaintAll();
        }
    }
}
