using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public GameObject interactionPrompt = default;
    public List<Interactor> interactors = new List<Interactor>();
    
    private float mSuccessDuration;
    private int mInteractionCount;

    public void OnObjectiveComplete()
    {
        currentObjectivesComplete++;
    }

    public void OnObjectiveUncomplete()
    {
        currentObjectivesComplete--;
    }

    public void ShowInteractionPrompt()
    {
        mInteractionCount++;
        if (mInteractionCount > 0)
        {
            interactionPrompt.SetActive(true);
        }
    }

    public void HideInteractionPrompt()
    {
        mInteractionCount--;
        if (mInteractionCount == 0)
        {
            interactionPrompt.SetActive(false);
        }
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

        bool shouldShowInteraction = interactors.Any(interactor => interactor.HasInteractableSelected());
        if (shouldShowInteraction && !interactionPrompt.activeSelf)
        {
            interactionPrompt.SetActive(true);
        }
        else if (!shouldShowInteraction && interactionPrompt.activeSelf)
        {
            interactionPrompt.SetActive(false);
        }
    }
}
