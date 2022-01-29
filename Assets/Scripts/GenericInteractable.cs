using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericInteractable : Interactable
{
    public bool destroyOnInteract = true;

    public UnityEvent onInteractEvent;

    public override void Interact()
    {
        onInteractEvent.Invoke();
        if (destroyOnInteract)
        {
            Destroy(gameObject);
        }
    }
}
