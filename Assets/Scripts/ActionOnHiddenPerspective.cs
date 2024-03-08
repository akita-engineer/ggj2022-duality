using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ActionOnHiddenPerspective : MonoBehaviour
{
    [Tooltip("Invoked when movement action inputs are received while this perspective is disabled. \n(eg. swap inputs when controlled perspective is disabled via rejoin)")]
    public UnityEvent onInputActionOnHiddenPerspective;

    private SplitablePerspective splitablePerspective;

    private void Awake()
    {
        splitablePerspective = GetComponent<SplitablePerspective>();
    }

    private void InvokeEventIfDisabled()
    {
        if (splitablePerspective.IsHidden())
        {
            onInputActionOnHiddenPerspective.Invoke();
        }
    }

    public void OnMove(InputValue value)
    {
        InvokeEventIfDisabled();
    }

    public void OnSprint(InputValue value)
    {
        InvokeEventIfDisabled();
    }

    public void OnJump(InputValue value)
    {
        InvokeEventIfDisabled();
    }

    public void OnLook(InputValue value)
    {
        InvokeEventIfDisabled();
    }

    public void OnInteract(InputValue value)
    {
        InvokeEventIfDisabled();
    }
}
