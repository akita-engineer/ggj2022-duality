using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Note: I'm too lazy to reconfigure the FPS controller with Unity Events so this an in-between for additonal inputs
public class InputPropagator : MonoBehaviour
{
	public UnityEvent propagateOnInteract;

	public void OnInteract(InputValue value)
	{
		propagateOnInteract.Invoke();
	}
}
