using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuration information used by all player and objective objects in the game to ensure parity.
/// </summary>
[CreateAssetMenu(fileName = "Player Config", menuName = "ScriptableObjects/Player Config")]
public class PlayerConfigScriptableObject : ScriptableObject
{
    public float fov = 60.0f;
    public float splitFov = 30.0f;

    [Header("Transition: Return to Horizon")]
    public AnimationCurve returnToHorizonCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float returnToHorizonDuration = 1.0f;

    [Header("Transition: Split perspective")]
    public AnimationCurve splitCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve splitCurve2 = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float splitDuration = 1.0f;

    [Header("Transition: Rejoin perspective")]
    public AnimationCurve rejoinCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float rejoinDuration = 1.0f;
}
