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

    [MenuItem("Tools/Vegetation Placement/Toggle")]
    public static void ToggleVegetationPlacement()
    {
        EditorPrefs.SetBool("VegetationPlacementEditor", !EditorPrefs.GetBool("VegetationPlacementEditor"));
        InitializePlacement();
    }

    public static void SelectPlacementCategory(int category)
    {
        VegetationPlacementAssetsPointer pointer = placementObjects[category];
        EditorPrefs.SetInt("VegetationPlacementSelectedObject", category);
        Debug.Log($"[PLACEMENT EDITOR] Switched to placing {pointer.Name}");
    }

    [MenuItem("Tools/Vegetation Placement/Place Grass")]
    public static void SelectGrassCategory()
    {
        SelectPlacementCategory(0);
    }

    [MenuItem("Tools/Vegetation Placement/Place Trees")]
    public static void SelectTreesCategory()
    {
        SelectPlacementCategory(1);
    }

    [MenuItem("Tools/Vegetation Placement/Place Crystals")]
    public static void SelectCrystalsCategory()
    {
        SelectPlacementCategory(2);
    }

    [MenuItem("Tools/Vegetation Placement/Place Rocks")]
    public static void SelectRocksCategory()
    {
        SelectPlacementCategory(3);
    }

    public class VegetationPlacementAssetsPointer
    {
        private string name;
        public string Name => name;

        private EnvironmentObjectType objectType;
        public EnvironmentObjectType ObjectType => objectType;

        private string path;
        public string Path => path;

        private string filter;
        public string Filter => filter;

        private Vector3 minScale;
        public Vector3 MinScale => minScale;

        private Vector3 maxScale;
        public Vector3 MaxScale => maxScale;

        private Vector3 offset;
        public Vector3 Offset => offset;

        private Dictionary<EnvironmentObjectType, float> distancesMap;
        public Dictionary<EnvironmentObjectType, float> DistancesMap => distancesMap;

        public VegetationPlacementAssetsPointer(string name, EnvironmentObjectType objectType, string path, string filter, Vector3 offset, Vector3 minScale, Vector3 maxScale, Dictionary<EnvironmentObjectType, float> distancesMap)
        {
            this.name = name;
            this.objectType = objectType;
            this.path = path;
            this.filter = filter;
            this.offset = offset;
            this.minScale = minScale;
            this.maxScale = maxScale;
            this.distancesMap = distancesMap;
        }
    }

    // TODO: could do a proximity matrix config
    public static VegetationPlacementAssetsPointer[] placementObjects = new VegetationPlacementAssetsPointer[]
    {
        new VegetationPlacementAssetsPointer("Grass", EnvironmentObjectType.Grass, "Assets/Prefabs/Grass/", "grass", Vector3.zero, Vector3.one, Vector3.one, new Dictionary<EnvironmentObjectType, float>(){
            {EnvironmentObjectType.Grass, 0.05f },
            {EnvironmentObjectType.Crystal, 0.1f },
            {EnvironmentObjectType.Rock, 0.1f },
            {EnvironmentObjectType.Tree, 0.1f }
        }),
        new VegetationPlacementAssetsPointer("Trees", EnvironmentObjectType.Tree, "Assets/Prefabs/Trees/", "Tree", new Vector3(0.0f, -0.5f, 0.0f), Vector3.one, Vector3.one * 2.0f, new Dictionary<EnvironmentObjectType, float>(){
            {EnvironmentObjectType.Grass, 0.1f },
            {EnvironmentObjectType.Crystal, 7.5f },
            {EnvironmentObjectType.Rock, 7.5f },
            {EnvironmentObjectType.Tree, 5f }
        }),
        new VegetationPlacementAssetsPointer("Crystals", EnvironmentObjectType.Crystal, "Assets/Prefabs/Crystals/", "Crystal", Vector3.zero, Vector3.one * 4.0f, Vector3.one * 8.0f, new Dictionary<EnvironmentObjectType, float>(){
            {EnvironmentObjectType.Grass, 0.1f },
            {EnvironmentObjectType.Crystal, 15f },
            {EnvironmentObjectType.Rock, 5f },
            {EnvironmentObjectType.Tree, 7.5f }
        }),
        new VegetationPlacementAssetsPointer("Rocks", EnvironmentObjectType.Rock, "Assets/Prefabs/Rocks/", "Rock", new Vector3(0.0f, -0.1f, 0.0f), Vector3.one * 4.0f, Vector3.one * 8.0f, new Dictionary<EnvironmentObjectType, float>(){
            {EnvironmentObjectType.Grass, 0.1f },
            {EnvironmentObjectType.Crystal, 5f },
            {EnvironmentObjectType.Rock, 5f },
            {EnvironmentObjectType.Tree, 7.5f }
        })
    };

    private static void PlaceBridge(Vector3 startPos, Vector3 endPos)
    {
        string stairsPath = "Assets/Prefabs/Stairs/Stairs.prefab";
        string straightPath = "Assets/Prefabs/Stairs/Path.prefab";
        GameObject stairsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(stairsPath);
        GameObject pathPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(straightPath);


        bool isRising = endPos.y > startPos.y;
        Vector3 pathDir = endPos - startPos;
        Vector3 rotationDir = new Vector3(pathDir.x, 0.0f, pathDir.z);

        // Flat rotation of pieces
        Quaternion pieceRotation = isRising ? Quaternion.LookRotation(-rotationDir) : Quaternion.LookRotation(rotationDir);

        // Have the root game object store the bridge
        GameObject allStairsRoot = GameObject.Find("Stairs");
        if (allStairsRoot == null)
        {
            allStairsRoot = new GameObject("Stairs");
            allStairsRoot.isStatic = true;
        }

        GameObject stairsInstanceRoot = new GameObject("StairsInstanceRoot");
        stairsInstanceRoot.isStatic = true;
        stairsInstanceRoot.transform.position = startPos;
        stairsInstanceRoot.transform.rotation = Quaternion.LookRotation(pathDir);
        stairsInstanceRoot.transform.parent = allStairsRoot.transform;


        // First piece is
        GameObject startPiece = PrefabUtility.InstantiatePrefab(pathPrefab) as GameObject;
        startPiece.transform.position = startPos;
        startPiece.transform.rotation = pieceRotation;
        startPiece.transform.parent = stairsInstanceRoot.transform;
        startPiece.AddComponent<FloatingPath>();

        GameObject lastPlacedPiece = startPiece;
        float distance = float.MaxValue;
        int maxPlaced = int.MaxValue;
        int placedCount = 0;

        while (true)
        {
            //Check what would be closest to the line
            Vector3 potentialHorizontalPos = Vector3.zero;
            if (lastPlacedPiece.GetComponent<FloatingPath>())
            {
                Vector3 offset = isRising ? new Vector3(0.0f, 0.0f, -2.538f) : new Vector3(0.0f, 0.0f, 2.538f);
                potentialHorizontalPos = lastPlacedPiece.transform.TransformPoint(offset);
            }
            else if (lastPlacedPiece.GetComponent<FloatingStairs>())
            {
                //Vector3 offset = isRising ? new Vector3(0, 0.449f, -2.648f) : new Vector3(0, -0.449f, 2.648f);
                Vector3 offset = isRising ? new Vector3(0.0f, 0.576f, -2.476f) : new Vector3(0.0f, -0.576f, 2.476f);
                potentialHorizontalPos = lastPlacedPiece.transform.TransformPoint(offset);
            }

            Vector3 potentialVerticalPos = Vector3.zero;
            if (lastPlacedPiece.GetComponent<FloatingPath>())
            {
                Vector3 offset = isRising ? new Vector3(0.0f, 0.576f, -2.476f) : new Vector3(0.0f, -0.576f, 2.476f);
                potentialVerticalPos = lastPlacedPiece.transform.TransformPoint(offset);
            }
            else if (lastPlacedPiece.GetComponent<FloatingStairs>())
            {
                Vector3 offset = isRising ? new Vector3(0, 1.328139f, -2.645683f) : new Vector3(0, -1.328139f, 2.645683f);
                potentialVerticalPos = lastPlacedPiece.transform.TransformPoint(offset);
            }

            float potentialHorizontalDistance = Vector3.Distance(potentialHorizontalPos, endPos);
            float potentialVerticalDistance = Vector3.Distance(potentialVerticalPos, endPos);

            bool dipsTooFar = false;
            if (isRising && potentialVerticalPos.y > endPos.y)
            {
                dipsTooFar = true;
            }

            if (!isRising && potentialVerticalPos.y < endPos.y)
            {
                dipsTooFar = true;
            }

            if (potentialHorizontalDistance < potentialVerticalDistance || dipsTooFar)
            {

                if (potentialHorizontalDistance > distance)
                {
                    break;
                }

                distance = potentialHorizontalDistance;

                lastPlacedPiece = PrefabUtility.InstantiatePrefab(pathPrefab) as GameObject;
                lastPlacedPiece.transform.position = potentialHorizontalPos;
                lastPlacedPiece.transform.rotation = pieceRotation;
                lastPlacedPiece.transform.parent = stairsInstanceRoot.transform;
                lastPlacedPiece.AddComponent<FloatingPath>();
            }
            else
            {
                if (potentialVerticalDistance > distance)
                {
                    break;
                }

                distance = potentialVerticalDistance;

                lastPlacedPiece = PrefabUtility.InstantiatePrefab(stairsPrefab) as GameObject;
                lastPlacedPiece.transform.position = potentialVerticalPos;
                lastPlacedPiece.transform.rotation = pieceRotation;
                lastPlacedPiece.transform.parent = stairsInstanceRoot.transform;
                lastPlacedPiece.AddComponent<FloatingStairs>();
            }

            lastPlacedPiece.isStatic = true;

            placedCount++;
            if (placedCount == maxPlaced)
            {
                break;
            }
        }

        Undo.RegisterCreatedObjectUndo(stairsInstanceRoot, "Place bridges");
    }

    private static void PlacementUpdate(SceneView scene)
    {
        if (Event.current.isScrollWheel && Event.current.shift)
        {
            if (Event.current.delta.y < 0)
            {
                int currentValue = EditorPrefs.GetInt("VegetationPlacementSelectedObject");
                int nextValue = (currentValue + 1) % placementObjects.Length;
                SelectPlacementCategory(nextValue);
            }

            if (Event.current.delta.y > 0)
            {
                int currentValue = EditorPrefs.GetInt("VegetationPlacementSelectedObject");
                int nextValue = currentValue == 0 ? placementObjects.Length - 1 : currentValue - 1;
                SelectPlacementCategory(nextValue);
            }
        }

        if (Event.current.isMouse && Event.current.button == 2 && Event.current.type == EventType.MouseDown && Event.current.shift)
        {
            int state = EditorPrefs.GetInt("BridgePlacementState");

            // Spawn first part of bridge
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000.0f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (state == 0)
                {
                    Debug.Log("[BRIDGE PLACEMENT] Setting point A...");
                    EditorPrefs.SetFloat("BridgePlacement_X", hitInfo.point.x);
                    EditorPrefs.SetFloat("BridgePlacement_Y", hitInfo.point.y);
                    EditorPrefs.SetFloat("BridgePlacement_Z", hitInfo.point.z);
                    EditorPrefs.SetInt("BridgePlacementState", ++state);
                    return;
                }

                if (state == 1)
                {
                    Debug.Log("[BRIDGE PLACEMENT] Setting point B and placing the bridge!");

                    // Construct the bridge
                    Vector3 startPos = new Vector3(EditorPrefs.GetFloat("BridgePlacement_X"), EditorPrefs.GetFloat("BridgePlacement_Y"), EditorPrefs.GetFloat("BridgePlacement_Z"));
                    Vector3 endPos = hitInfo.point;
                    PlaceBridge(startPos, endPos);
                    EditorPrefs.SetInt("BridgePlacementState", 0);
                }
            }
        }

        // Hold to draw
        // NOTE: isKey is important, because control and alt may be called for numerous other events too..
        if (Event.current.isKey && Event.current.control && Event.current.alt)
        {
            VegetationPlacementAssetsPointer pointer = placementObjects[EditorPrefs.GetInt("VegetationPlacementSelectedObject")];

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000.0f, ~0, QueryTriggerInteraction.Ignore))
            {
                // Check if there is any bad overlap
                bool overlaps = false;
                foreach (EnvironmentObjectType type in pointer.DistancesMap.Keys)
                {
                    float distance = pointer.DistancesMap[type];
                    Collider[] allOverlaps = Physics.OverlapSphere(hitInfo.point, distance, ~0, QueryTriggerInteraction.Ignore);
                    foreach (Collider c in allOverlaps)
                    {
                        EnvironmentObject envObject = c.GetComponentInParent<EnvironmentObject>();
                        if (envObject && envObject.ObjectType == type)
                        {
                            overlaps = true;
                        }
                    }
                }

                if (overlaps)
                {
                    return;
                }

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

                GameObject result = PrefabUtility.InstantiatePrefab(randomObject) as GameObject;
                result.transform.position = hitInfo.point;
                result.transform.rotation = Quaternion.identity;
                result.transform.parent = placementRoot.transform;

                // Offset
                result.transform.position += pointer.Offset;

                // Scale
                float randomScale = Random.Range(0.0f, 1.0f);
                result.transform.localScale = Vector3.Lerp(pointer.MinScale, pointer.MaxScale, randomScale);

                Undo.RegisterCreatedObjectUndo(result, "Vegetation Placement Spawn");
            }
        }

        if (Event.current.control && Event.current.isMouse && Event.current.type == EventType.MouseDown && Event.current.button == 2)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000.0f, ~0, QueryTriggerInteraction.Ignore))
            {
                GameObject player = GameObject.Find("Player L");
                player.transform.position = hitInfo.point + Vector3.up * 2;

                GameObject player2 = GameObject.Find("PlayerL_R");
                player2.transform.position = hitInfo.point + Vector3.up * 2;
            }

            if (Event.current.capsLock)
            {
                EditorApplication.isPlaying = true;
            }
        }
    }

    static string islandsPath = "Assets/Prefabs/Islands/";
    private static void GenerateIsland(string filterName)
    {
        string[] assets = Directory.GetFiles(islandsPath);
        List<GameObject> resultIslandPool = new List<GameObject>();

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

            if (!assetPath.Contains(filterName))
            {
                continue;
            }

            GameObject objectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            resultIslandPool.Add(objectPrefab);
        }

        GameObject randomIsland = resultIslandPool[Random.Range(0, resultIslandPool.Count - 1)];
        Camera sceneCamera = SceneView.lastActiveSceneView.camera;
        Vector3 resultPos = sceneCamera.transform.position + sceneCamera.transform.forward * 30f;

        GameObject islandsRoot = GameObject.Find("Islands");
        if (islandsRoot == null)
        {
            islandsRoot = new GameObject("Islands");
        }

        GameObject newObject = PrefabUtility.InstantiatePrefab(randomIsland) as GameObject;
        newObject.transform.position = resultPos;
        newObject.transform.rotation = Quaternion.identity;
        newObject.transform.parent = islandsRoot.transform;
        Undo.RegisterCreatedObjectUndo(newObject, "Place Island");
    }

    [MenuItem("Tools/Island Placement/Generate Regular Island")]
    private static void GenerateRegularIsland()
    {
        GenerateIsland("RegularIsland");
    }

    [MenuItem("Tools/Island Placement/Generate Interesting Island")]
    private static void GenerateInterestingIsland()
    {
        GenerateIsland("InterestingIsland");
    }

    [MenuItem("Tools/Island Placement/Generate Platforming Island")]
    private static void GeneratePlatformingIsland()
    {
        GenerateIsland("PlatformingIsland");
    }
}
