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

    public class VegetationPlacementAssetsPointer
    {
        private string name;
        public string Name => name;

        private string path;
        public string Path => path;

        private string filter;
        public string Filter => filter;

        private bool placeColliders;
        public bool PlaceColliders => placeColliders;

        private Vector3 minScale;
        public Vector3 MinScale => minScale;

        private Vector3 maxScale;
        public Vector3 MaxScale => maxScale;

        public VegetationPlacementAssetsPointer(string name, string path, string filter, bool placeColliders, Vector3 minScale, Vector3 maxScale)
        {
            this.name = name;
            this.path = path;
            this.filter = filter;
            this.placeColliders = placeColliders;
            this.minScale = minScale;
            this.maxScale = maxScale;
        }
    }

    public static VegetationPlacementAssetsPointer[] placementObjects = new VegetationPlacementAssetsPointer[]
    {
        new VegetationPlacementAssetsPointer("Grass", "Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/", "grass", false, Vector3.one, Vector3.one),
        new VegetationPlacementAssetsPointer("Trees", "Assets/Waldemarst/JapaneseGardenPackage/Prefabs/", "Tree", true, Vector3.one / 1.5f, Vector3.one),
        new VegetationPlacementAssetsPointer("Crystals", "Assets/SineVFX/TranslucentCrystals/Prefabs/", "Crystal", true, Vector3.one / 2.0f, Vector3.one),
        new VegetationPlacementAssetsPointer("Rocks", "Assets/HQ_BigRock/", "Rock", true, Vector3.one / 2.0f, Vector3.one)
    };

    private static void PlacementUpdate(SceneView scene)
    {
        if (Event.current.isScrollWheel && Event.current.shift)
        {
            if (Event.current.delta.y < 0)
            {
                int currentValue = EditorPrefs.GetInt("VegetationPlacementSelectedObject");
                int nextValue = (currentValue + 1) % placementObjects.Length;
                EditorPrefs.SetInt("VegetationPlacementSelectedObject", nextValue);
                Debug.Log($"[PLACEMENT EDITOR] Switched to placing {placementObjects[nextValue].Name}");
            }

            if (Event.current.delta.y > 0)
            {
                Debug.Log("Scroll down");
                int currentValue = EditorPrefs.GetInt("VegetationPlacementSelectedObject");
                int nextValue = currentValue == 0 ? placementObjects.Length - 1 : currentValue - 1;
                EditorPrefs.SetInt("VegetationPlacementSelectedObject", nextValue);
                Debug.Log($"[PLACEMENT EDITOR] Switched to placing {placementObjects[nextValue].Name}");
            }
        }

        if (Event.current.isMouse && Event.current.button == 2 && Event.current.type == EventType.MouseDown)
        {
            VegetationPlacementAssetsPointer pointer = placementObjects[EditorPrefs.GetInt("VegetationPlacementSelectedObject")];

            Debug.Log($"[PLACEMENT EDITOR] Placing {pointer.Name}...");

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                string[] assets = Directory.GetFiles(pointer.Path);
                List<GameObject> resultObjectPool = new List<GameObject>();

                foreach (string assetPath in assets)
                {
                    if (assetPath.Contains(".meta"))
                    {
                        continue;
                    }

                    if (!assetPath.Contains(".prefab"))
                    {
                        continue;
                    }

                    if (!assetPath.Contains(pointer.Filter))
                    {
                        continue;
                    }

                    GameObject objectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    resultObjectPool.Add(objectPrefab);
                }

                GameObject randomObject = resultObjectPool[Random.Range(0, resultObjectPool.Count - 1)];
                GameObject placementRoot = null;

                // We'd want to keep objects grouped on islands
                FloatingIsland islandRoot = hitInfo.transform.gameObject.GetComponentInParent<FloatingIsland>();
                if (islandRoot == null)
                {
                    placementRoot = GameObject.Find(pointer.Name);
                    if (placementRoot == null)
                    {
                        placementRoot = new GameObject(pointer.Name);
                        placementRoot.isStatic = true;
                    }
                }
                else
                {
                    for (int i = 0; i < islandRoot.transform.childCount; i++)
                    {
                        GameObject child = islandRoot.transform.GetChild(i).gameObject;
                        if (child.name == pointer.Name)
                        {
                            placementRoot = child;
                            break;
                        }
                    }

                    if (placementRoot == null)
                    {
                        placementRoot = new GameObject(pointer.Name);
                        placementRoot.isStatic = true;
                        placementRoot.transform.parent = islandRoot.transform;
                        placementRoot.transform.localPosition = Vector3.zero;

                    }
                }

                GameObject result = GameObject.Instantiate(randomObject, hitInfo.point, Quaternion.identity, placementRoot.transform);

                // Static
                foreach (Transform t in result.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.isStatic = true;
                }

                // Scale
                float randomScale = Random.Range(0.0f, 1.0f);
                result.transform.localScale = Vector3.Lerp(pointer.MinScale, pointer.MaxScale, randomScale);

                // Colliders
                if (pointer.PlaceColliders)
                {
                    Collider[] colliders = result.GetComponentsInChildren<Collider>();
                    if (colliders.Length == 0)
                    {
                        // Need to add mesh colliders if NO colliders are present
                        Renderer[] allRenderers = result.GetComponentsInChildren<Renderer>();
                        foreach (Renderer renderObject in allRenderers)
                        {
                            renderObject.gameObject.AddComponent<MeshCollider>();
                        }
                    }
                }

                Undo.RegisterCreatedObjectUndo(result, "Vegetation Placement Spawn");
            }
        }
    }
}
