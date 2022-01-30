using UnityEngine;

public enum EnvironmentObjectType
{
    Grass, Tree, Crystal, Rock
}

public class EnvironmentObject : MonoBehaviour
{
    [SerializeField]
    private EnvironmentObjectType objectType = default;
    public EnvironmentObjectType ObjectType => objectType;
}
