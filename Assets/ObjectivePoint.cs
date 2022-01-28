using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectivePoint : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float successThreshold = 0.9f;

    // how much to weight distance vs orientation
    [Range(0.0f, 1.0f)]
    public float distanceToOrientationEvaluationPercentage = 0.5f;

    // The L/R camera transform
    public Transform target;

    [Header("Distance")]
    public float distanceMaxDistance = 10.0f;
    public AnimationCurve distanceCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Orientation")]
    public float orientationMaxAngle = 15.0f;
    public float orientationStartDistance = 2.0f;
    public AnimationCurve orientationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    // distance to the objective before orientation score is taken into account
    public AnimationCurve orientationDistanceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float mScore = 0.0f;
    private float mPrevScore = 0.0f;

    // DEBUG
    [SerializeField]
    private float mXOffset = 540;
    private float mDistanceDelta;
    private float mDistanceScore;
    private float mOrientationDelta;
    private float mOrietationScore;

    private static float EvaluateInRange(float delta, float deltaMax, AnimationCurve curve)
    {
        return curve.Evaluate(Mathf.Clamp((deltaMax - delta) / deltaMax, 0, 1));
    }

    public void Update()
    {
        float distanceDelta = Vector3.Distance(target.position, transform.position);
        float distanceScore = EvaluateInRange(distanceDelta, distanceMaxDistance, distanceCurve);

        float rotationDelta = Quaternion.Angle(target.rotation, transform.rotation);
        float rotationRawScore = EvaluateInRange(rotationDelta, orientationMaxAngle, orientationCurve);
        float rotationDistanceScore = EvaluateInRange(distanceDelta, orientationStartDistance, orientationDistanceCurve);
        float rotationScore = rotationRawScore * rotationDistanceScore;

        mScore = distanceScore * distanceToOrientationEvaluationPercentage + rotationScore * (1 - distanceToOrientationEvaluationPercentage);

        // do stuff with final score

        if (mScore >= successThreshold && mPrevScore < successThreshold)
        {
            SimpleGameController.Instance.OnObjectiveComplete();
        }
        else if (mScore < successThreshold && mPrevScore >= successThreshold) {
            SimpleGameController.Instance.OnObjectiveUncomplete();
        }
        mPrevScore = mScore;

        // DEBUG
        mDistanceDelta = distanceDelta;
        mDistanceScore = distanceScore;
        mOrientationDelta = rotationDelta;
        mOrietationScore = rotationScore;
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(mXOffset, 10, 500, 500));
        if (mScore >= successThreshold)
        {
            GUI.color = Color.green;
        }
        GUILayout.Label("Score: " + mScore);
        GUI.color = Color.white;
        GUILayout.Label("Distance: " + mDistanceDelta);
        GUILayout.Label("Distance Score: " + mDistanceScore);
        GUILayout.Label("Orientation: " + mOrientationDelta);
        GUILayout.Label("Orientation Score: " + mOrietationScore);
        GUILayout.EndArea();
    }
}
