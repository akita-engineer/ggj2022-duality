using UnityEditor;
using UnityEngine;

public class VegetationPlacementEditorWindow : EditorWindow
{
    [MenuItem("Window/Vegetation Placement Editor")]
    public static void Init()
    {
        VegetationPlacementEditorWindow window = (VegetationPlacementEditorWindow)EditorWindow.GetWindow(typeof(VegetationPlacementEditorWindow));
        window.Show();
    }

    public void OnGUI()
    {
        GUILayout.Label("Vegetation Placement Tool", EditorStyles.boldLabel);

        // NOTE: Possibly allow to populate pools here
    }
}
