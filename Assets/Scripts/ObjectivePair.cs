using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectivePair : MonoBehaviour
{
    [Tooltip("GameController will automatically be notified.")]
    public UnityEvent onSuccess;

    [Tooltip("Success/fail events will automatically be bound, as well as the required perspective side.")]
    public ObjectivePoint leftPoint;
    
    [Tooltip("Success/fail events will automatically be bound, as well as the required perspective side.")]
    public ObjectivePoint rightPoint;

    private int currentObjectivesComplete = 0;

    private float successDurationThreshold
    {
        get
        {
            return SimpleGameController.Instance.successDurationThreshold;
        }
    }

    private float successDuration = 0.0f;

    // Ensures all relative objective objects are active
    public void ActivateObjective()
    {
        gameObject.SetActive(true);
        leftPoint.gameObject.SetActive(true);
        rightPoint.gameObject.SetActive(true);
    }

    public void OnPointSuccess(ObjectivePoint point)
    {
        currentObjectivesComplete++;
    }

    public void OnPointFail(ObjectivePoint point)
    {
        currentObjectivesComplete--;
    }

    private void Start()
    {
        if (leftPoint == null || rightPoint == null)
        {
            Debug.Log("ObjectivePair points were not setup");
            this.enabled = false;
        }

        leftPoint.onSuccess.AddListener(OnPointSuccess);
        rightPoint.onSuccess.AddListener(OnPointSuccess);
        leftPoint.onFail.AddListener(OnPointFail);
        rightPoint.onFail.AddListener(OnPointFail);
        if (leftPoint.target == null)
        {
            leftPoint.target = SimpleGameController.Instance.playerL.transform;
        }
        if (rightPoint.target == null)
        {
            rightPoint.target = SimpleGameController.Instance.playerR.transform;
        }
    }

    private void Update()
    {
        if (currentObjectivesComplete >= 2)
        {
            successDuration += Time.deltaTime;
            if (successDuration >= successDurationThreshold)
            {
                onSuccess.Invoke();
                SimpleGameController.Instance.OnObjectivePairComplete(this);
                this.enabled = false;
            }
        }
        else
        {
            successDuration = 0;
        }
    }
}
