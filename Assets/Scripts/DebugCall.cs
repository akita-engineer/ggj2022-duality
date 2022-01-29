using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DebugCall : MonoBehaviour
{
    public UnityEvent call;

    public bool callNow = false;

    private void Update()
    {
        if (callNow) {
            call.Invoke();
            callNow = false;
        }
    }
}
