using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGameController : MonoBehaviour
{
    #region Singleton

    private static SimpleGameController _instance;

    public static SimpleGameController Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    #endregion

    public int successThreshold = 2;

    public int currentObjectivesComplete = 0;

    public float successDurationThreshold = 2.0f;

    public GameObject winScreen = default;

    private float mSuccessDuration;

    public void OnObjectiveComplete()
    {
        currentObjectivesComplete++;
    }

    public void OnObjectiveUncomplete()
    {
        currentObjectivesComplete--;
    }

    private void Update()
    {
        if (currentObjectivesComplete >= successThreshold)
        {
            mSuccessDuration += Time.deltaTime;
            if (mSuccessDuration >= successDurationThreshold)
            {
                winScreen.SetActive(true);
            }
        }
        else
        {
            mSuccessDuration = 0;
        }
    }
}
