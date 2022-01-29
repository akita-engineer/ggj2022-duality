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


        string stairsPath = "Assets/laxer Assets/wood bridge/Prefabs with collision/wood bridge stairs 2.prefab";
        string straightPath = "Assets/laxer Assets/wood bridge/Prefabs with collision/wood bridge 2.prefab";
        if (Event.current.isMouse && Event.current.button == 2 && Event.current.type == EventType.MouseDown && Event.current.shift)
        {
            int state = EditorPrefs.GetInt("BridgePlacementState");

            // Spawn first part of bridge
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
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

                    // Place the bridge!
                    Vector3 startPos = new Vector3(EditorPrefs.GetFloat("BridgePlacement_X"), EditorPrefs.GetFloat("BridgePlacement_Y"), EditorPrefs.GetFloat("BridgePlacement_Z"));
                    Vector3 endPos = hitInfo.point;

                    // If the angle between them is higher or equal than 45 degrees, than use the 


                    // we basically have to "render" the line with the bridge prefabs.
                    // we have a straight piece and an angle piece.

                    Vector3 pathDir = endPos - startPos;

                    GameObject stairsRootDebug = new GameObject("StairsRootDebug");
                    stairsRootDebug.transform.position = startPos;
                    stairsRootDebug.transform.rotation = Quaternion.LookRotation(pathDir);


                    float distance = Vector3.Distance(startPos, endPos);
                    int numberOfSteps = (int)(distance / 2.5f);

                    GameObject stairsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(stairsPath);
                    GameObject pathPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(straightPath);





                    if (endPos.y > startPos.y)
                    {
                        Vector3 rotationDir = new Vector3(pathDir.x, 0.0f, pathDir.z);
                        Quaternion resultRotation = Quaternion.LookRotation(-rotationDir);

                        GameObject startStairs = GameObject.Instantiate(stairsPrefab, startPos, resultRotation, stairsRootDebug.transform);

                        float currentDistance = float.MaxValue;
                        for (int i = 0; ; i++)
                        {
                            Vector3 resultWorldPos = startStairs.transform.TransformPoint(new Vector3(0, 1.328139f, -2.645683f) * i);

                            float dist = Vector3.Distance(resultWorldPos, endPos);
                            if (dist > currentDistance)
                            {
                                break;
                            }

                            currentDistance = dist;

                            GameObject.Instantiate(stairsPrefab, resultWorldPos, resultRotation, stairsRootDebug.transform);
                        }
                    } else
                    {
                        // Going down...

                        Vector3 rotationDir = new Vector3(pathDir.x, 0.0f, pathDir.z);
                        Quaternion resultRotation = Quaternion.LookRotation(rotationDir);

                        GameObject startStairs = GameObject.Instantiate(stairsPrefab, startPos, resultRotation, stairsRootDebug.transform);
                        Vector3 prevPoint = startPos;

                        float currentDistance = float.MaxValue;
                        for (int i = 0; ; i++)
                        {
                            // Compare current point against the result...


                            Vector3 resultWorldPosVertical = startStairs.transform.TransformPoint(new Vector3(0, -1.328139f, 2.645683f) * i);
                            Vector3 resultWorldPosHorizontal = startStairs.transform.TransformPoint(new Vector3(0, -0.449f, 2.648f) * i);


                            //float verticalDistance = Mathf.Abs(endPos.y - prevPoint.y);
                            //float horizontalDistance = Vector3.Distance(new Vector3(prevPoint.x, 0.0f, prevPoint.z), new Vector3(endPos.x, 0.0f, endPos.z));

                            //// whatever's furthest, need to cover
                            //if (verticalDistance > horizontalDistance)
                            //{
                            //    float dist = Vector3.Distance(resultWorldPosVertical, endPos);
                            //    if (dist > currentDistance)
                            //    {
                            //        break;
                            //    }

                            //    GameObject.Instantiate(stairsPrefab, resultWorldPosVertical, resultRotation, stairsRootDebug.transform);
                            //    prevPoint = resultWorldPosVertical;

                            //    currentDistance = dist;
                            //}
                            //else
                            //{
                            //    float dist = Vector3.Distance(resultWorldPosHorizontal, endPos);
                            //    if (dist > currentDistance)
                            //    {
                            //        break;
                            //    }

                            //    GameObject.Instantiate(pathPrefab, resultWorldPosHorizontal, resultRotation, stairsRootDebug.transform);
                            //    prevPoint = resultWorldPosHorizontal;

                            //    currentDistance = dist;
                            //}

                            // need to account for the fact that horizontal + vertical paths may be connected..

                            float dist = Vector3.Distance(resultWorldPosVertical, endPos);
                            if (dist > currentDistance)
                            {
                                break;
                            }

                            GameObject.Instantiate(stairsPrefab, resultWorldPosVertical, resultRotation, stairsRootDebug.transform);
                            prevPoint = resultWorldPosVertical;

                            currentDistance = dist;
                        }
                    }

                    //...
                    Undo.RegisterCreatedObjectUndo(stairsRootDebug, "Auto-stairs");
                    //...
                    EditorPrefs.SetInt("BridgePlacementState", 0);
                }
            }
        }

        if (Event.current.isMouse && Event.current.button == 2 && Event.current.type == EventType.MouseDown && !Event.current.shift)
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
