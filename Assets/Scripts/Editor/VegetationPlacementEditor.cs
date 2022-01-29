using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class VegetationPlacementEditor
{
    static VegetationPlacementEditor()
    {
        InitializePlacement();
    }

    public static void InitializePlacement()
    {
        bool isActive = EditorPrefs.GetBool("VegetationPlacementEditor");
        Debug.Log("Vegetation placement is set to " + isActive);
        if (isActive)
        {
            SceneView.duringSceneGui += PlacementUpdate;
        }
        else
        {
            SceneView.duringSceneGui -= PlacementUpdate;
        }
    }

    [MenuItem("Tools/Toggle Vegetation Placement")]
    public static void ToggleVegetationPlacement()
    {
        EditorPrefs.SetBool("VegetationPlacementEditor", !EditorPrefs.GetBool("VegetationPlacementEditor"));
        InitializePlacement();
    }

    static string grassPrefab = "Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab";
    static string treesFolderPath = "Assets/Waldemarst/JapaneseGardenPackage/Prefabs/";

    private static void PlacementUpdate(SceneView scene)
    {
       if (Event.current.isMouse && Event.current.button == 0 && Event.current.type == EventType.MouseDown)
        {
            Debug.Log("Placing grass...");

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                GameObject grass = AssetDatabase.LoadAssetAtPath<GameObject>(grassPrefab);

                GameObject grassRoot = GameObject.Find("Grass");
                if (grassRoot == null)
                {
                    grassRoot = new GameObject("Grass");
                }
                GameObject.Instantiate(grass, hitInfo.point, Quaternion.identity, grassRoot.transform);
            }
        }

        if (Event.current.isMouse && Event.current.button == 2 && Event.current.type == EventType.MouseDown)
        {
            Debug.Log("Placing a tree...");

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                string[] assets = Directory.GetFiles(treesFolderPath);
                List<GameObject> resultTreePool = new List<GameObject>();

                foreach (string assetPath in assets)
                {
                    Debug.Log(assetPath);
                    if (assetPath.Contains(".meta"))
                    {
                        continue;
                    }

                    if (!assetPath.Contains(".prefab"))
                    {
                        continue;
                    }

                    if (!assetPath.Contains("Tree"))
                    {
                        continue;
                    }

                    GameObject treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    resultTreePool.Add(treePrefab);
                }

                GameObject randomTree = resultTreePool[Random.Range(0, resultTreePool.Count - 1)];

                GameObject treeRoot = GameObject.Find("Trees");
                if (treeRoot == null)
                {
                    treeRoot = new GameObject("Trees");
                }
                GameObject.Instantiate(randomTree, hitInfo.point, Quaternion.identity, treeRoot.transform);
            }
        }
    }
}
