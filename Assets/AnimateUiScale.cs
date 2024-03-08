using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// man what am I writing.... 
public class AnimateUiScale : MonoBehaviour
{
    // sphagetti: this is the joined scale 
    public Vector3 targetScale;
    
    // sphagetti: this is the split scale 
    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    public void AnimateToTargetState(float duration)
    {
        StartCoroutine(AnimationHelper(initialScale, targetScale, duration));
    }

    public void AnimationToInitialState(float duration)
    {
        StartCoroutine(AnimationHelper(targetScale, initialScale, duration));
    }

    public IEnumerator AnimationHelper(Vector3 from, Vector3 to, float duration)
    {
        float counter = 0.0f;
        do
        {
            counter = Mathf.Min(counter + Time.deltaTime, duration);
            float progress = (duration <= 0.0f) ? 1.0f : counter / duration;
            transform.localScale = Vector3.Lerp(from, to, progress);
            yield return null;
        } while (counter < duration);
    }
}
