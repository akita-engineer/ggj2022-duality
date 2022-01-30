using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericInteractable : Interactable
{
    public bool destroyOnInteract = true;

    public FlagScriptableObject requiresFlag;

    public FlagScriptableObject setsFlag;

    public UnityEvent onInteractEvent;

    public UnityEvent onInteractFailedEvent;

    public override void Interact()
    {
        if (requiresFlag != null && !SimpleGameController.Instance.GetFlag(requiresFlag))
        {
            onInteractFailedEvent.Invoke();
            return;
        }

        if (setsFlag != null)
        {
            SimpleGameController.Instance.SetFlag(setsFlag);
        }

        onInteractEvent.Invoke();
        if (destroyOnInteract)
        {
            Destroy(gameObject);
        }
    }
}
