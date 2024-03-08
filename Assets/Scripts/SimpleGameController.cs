using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

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

    [Header("Constants")]
    public float successDurationThreshold = 2.0f;

    [Header("Game Play / Objectives")]
    public List<ObjectivePair> objectivePairs = new List<ObjectivePair>();
    public ObjectivePair activeObjectivePair = null;
    public UnityEvent onObjectivePairComplete;
    public UnityEvent allObjectivePairsComplete;

    [Header("Game Play / Player")]
    public bool startWithPerspectivesJoined = true;

    [Header("Other")]
    public GameObject winScreen = default;
    public GameObject interactionPrompt = default;
    public List<Interactor> interactors = new List<Interactor>();

    [Header("Runtime References")]
    public GameObject playerL;
    public GameObject playerR;

    public UnityEvent onFall;

    private Dictionary<FlagScriptableObject, bool> variableStore = new Dictionary<FlagScriptableObject, bool>();

    private int mInteractionCount;

    public void OnObjectivePairComplete(ObjectivePair objective)
    {
        onObjectivePairComplete.Invoke();

        objectivePairs.Remove(objective);
        if (objectivePairs.Count == 0)
        {
            allObjectivePairsComplete.Invoke();
        }
        else
        {
            if (activeObjectivePair == objective)
            {
                activeObjectivePair = objectivePairs[0];
                activeObjectivePair.ActivateObjective();
            }
        }
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

    public void OnFall()
    {
        onFall.Invoke();
    }

    public void SetFlag(FlagScriptableObject flag)
    {
        Debug.Log("Raising flag: " + flag.name);
        variableStore[flag] = true;
    }

    public bool GetFlag(FlagScriptableObject flag)
    {
        bool value = false;
        variableStore.TryGetValue(flag, out value);
        Debug.Log("Getting flag: " + flag.name + " value: " + value);
        return value;
    }

    private void Start()
    {
        if (startWithPerspectivesJoined)
        {
            var lSplit = playerL.GetComponent<SplitablePerspective>();
            lSplit.transitionMode = SplitablePerspective.TransitionMode.InstantOneShot;
            lSplit.Activate();
            //var rSplit = playerR.GetComponent<SplitablePerspective>();
            //rSplit.transitionMode = SplitablePerspective.TransitionMode.InstantOneShot;
            //rSplit.Activate();
        }
    }

    private void Update()
    {
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
