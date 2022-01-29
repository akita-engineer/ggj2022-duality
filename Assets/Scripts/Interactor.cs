using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    public float interactionRange = 2.0f;

    public LayerMask interactionMask;

    private Interactable mCachedInteractable;

    public void TryInteract()
    {
        if (mCachedInteractable)
        {
            mCachedInteractable.Interact();
        }
    }

    public bool HasInteractableSelected()
    {
        return mCachedInteractable != null;
    }
    
    private void FixedUpdate()
    {
        mCachedInteractable = null;
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange, interactionMask))
        {
            Interactable interactable;
            if (hit.collider.TryGetComponent<Interactable>(out interactable))
            {
                mCachedInteractable = interactable;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * interactionRange);
    }
}
