using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwappableInputPropagatorMessages : MonoBehaviour
{
    public GameObject target;

    public SwappableInputPropagatorMessages swapTarget;

    public void OnMove(InputValue value)
    {
        target.SendMessage("OnMove", value);
    }

    public void OnLook(InputValue value)
    {
        target.SendMessage("OnLook", value);
    }

    public void OnJump(InputValue value)
    {
        target.SendMessage("OnJump", value);
    }

    public void OnSprint(InputValue value)
    {
        target.SendMessage("OnSprint", value);
    }

    public void OnInteract(InputValue value)
    {
        target.SendMessage("OnInteract", value);
    }

    public void OnSwapPerspectives(InputValue value)
    {
        var temp = target;
        target = swapTarget.target;
        swapTarget.target = temp;
    }
}
