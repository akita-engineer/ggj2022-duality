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

        private Vector3 offset;
        public Vector3 Offset => offset;

        public VegetationPlacementAssetsPointer(string name, string path, string filter, bool placeColliders, Vector3 offset, Vector3 minScale, Vector3 maxScale)
        {
            this.name = name;
            this.path = path;
            this.filter = filter;
            this.placeColliders = placeColliders;
            this.offset = offset;
            this.minScale = minScale;
            this.maxScale = maxScale;
        }
    }

    public static VegetationPlacementAssetsPointer[] placementObjects = new VegetationPlacementAssetsPointer[]
    {
        new VegetationPlacementAssetsPointer("Grass", "Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/", "grass", false, Vector3.zero, Vector3.one, Vector3.one),
        new VegetationPlacementAssetsPointer("Trees", "Assets/Waldemarst/JapaneseGardenPackage/Prefabs/", "Tree", true, new Vector3(0.0f, -0.5f, 0.0f), Vector3.one / 1.5f, Vector3.one),
        new VegetationPlacementAssetsPointer("Crystals", "Assets/Prefabs/Crystals/", "Crystal", true, Vector3.zero, Vector3.one * 4.0f, Vector3.one * 8.0f),
        new VegetationPlacementAssetsPointer("Rocks", "Assets/HQ_BigRock/", "Rock", true, new Vector3(0.0f, -0.1f, 0.0f), Vector3.one * 2.0f, Vector3.one * 4.0f)
    };

    private static void PlaceBridge(Vector3 startPos, Vector3 endPos)
    {
        string stairsPath = "Assets/laxer Assets/wood bridge/Prefabs with collision/wood bridge stairs 2.prefab";
        string straightPath = "Assets/laxer Assets/wood bridge/Prefabs with collision/wood bridge 2.prefab";
        GameObject stairsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(stairsPath);
        GameObject pathPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(straightPath);


        bool isRising = endPos.y > startPos.y;
        Vector3 pathDir = endPos - startPos;
        float totalDistance = Vector3.Distance(startPos, endPos);
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
        GameObject startPiece = GameObject.Instantiate(pathPrefab, startPos, pieceRotation, stairsInstanceRoot.transform);
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
            } else if (lastPlacedPiece.GetComponent<FloatingStairs>())
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

            if (potentialHorizontalDistance < potentialVerticalDistance || dipsTooFar) {

                if (potentialHorizontalDistance > distance)
                {
                    break;
                }

                distance = potentialHorizontalDistance;

                Debug.Log("[BRIDGE PLACEMENT] Placing horizontal path...");
                lastPlacedPiece = GameObject.Instantiate(pathPrefab, potentialHorizontalPos, pieceRotation, stairsInstanceRoot.transform);
                lastPlacedPiece.AddComponent<FloatingPath>();
            } else
            {
                if (potentialVerticalDistance > distance)
                {
                    break;
                }

                distance = potentialVerticalDistance;

                Debug.Log("[BRIDGE PLACEMENT] Placing vertical path...");
                lastPlacedPiece = GameObject.Instantiate(stairsPrefab, potentialVerticalPos, pieceRotation, stairsInstanceRoot.transform);
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
                Debug.Log("Scroll down");
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
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100.0f, ~0, QueryTriggerInteraction.Ignore))
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

        if (Event.current.isMouse && Event.current.button == 2 && Event.current.type == EventType.MouseDown && !Event.current.shift)
        {
            VegetationPlacementAssetsPointer pointer = placementObjects[EditorPrefs.GetInt("VegetationPlacementSelectedObject")];

            Debug.Log($"[PLACEMENT EDITOR] Placing {pointer.Name}...");

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100.0f, ~0, QueryTriggerInteraction.Ignore))
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

                // Offset
                result.transform.position += pointer.Offset;

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

        if (Event.current.control && Event.current.isMouse && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100.0f, ~0, QueryTriggerInteraction.Ignore))
            {
                GameObject player = GameObject.Find("PlayerCapsule");
                player.transform.position = hitInfo.point + Vector3.up * 2;
            }
        }
    }
}
