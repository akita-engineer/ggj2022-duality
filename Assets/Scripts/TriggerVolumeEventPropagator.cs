using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerVolumeEventPropagator : MonoBehaviour
{
    public UnityEvent<Collider> onEnter;
    public UnityEvent<Collider> onExit;
    [Tooltip("Triggers once when an intruder stays long enough. See stayDuration")]
    public UnityEvent onStay;

    public float stayDuration = 5.0f;

    private float counter = 0;
    [SerializeField]
    private int intruderCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        onEnter.Invoke(other);
        intruderCount++;
    }

    private void OnTriggerExit(Collider other)
    {
        onExit.Invoke(other);
        intruderCount--;
        if (intruderCount == 0)
        {
            counter = 0;
        }
    }

    private void FixedUpdate()
    {
        if (intruderCount > 0 && stayDuration > 0)
        {
            var prev = counter;
            counter += Time.fixedDeltaTime;
            if (prev < stayDuration && counter >= stayDuration) {
                onStay.Invoke();
            }
        }
    }
}
