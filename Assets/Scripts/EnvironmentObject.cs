using UnityEngine;

public enum EnvironmentObjectType
{
    Grass, Tree, Crystal, Rock
}

public class EnvironmentObject : MonoBehaviour
{
    private EnvironmentObjectType objectType;
    public EnvironmentObjectType ObjectType => objectType;


    public void SetObjectType(EnvironmentObjectType type)
    {
        objectType = type;
    }
}
