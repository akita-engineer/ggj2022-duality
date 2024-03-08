using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTeleporter : MonoBehaviour
{
    public Transform teleportLocation;

    private CharacterController cc;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    public void Teleport()
    {
        if (teleportLocation != null)
        {
            Debug.Log("teleport valid");

            if (cc != null)
            {
                cc.enabled = false;
            }

            transform.position = teleportLocation.position;

            if (cc != null)
            {
                cc.enabled = true;
            }
        }
    }

    public void OnDebugTeleport()
    {
        Debug.Log("On Debug teleport");
        Teleport();
    }
}
