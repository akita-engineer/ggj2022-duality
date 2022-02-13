using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Just a convenience tool, so you don't need to bind to the SimpleGameController in editor.
public class RaiseFlag : MonoBehaviour
{
    public FlagScriptableObject key;

    public void Raise()
    {
        SimpleGameController.Instance.SetFlag(key);
    }

}
