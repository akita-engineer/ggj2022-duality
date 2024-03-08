using StarterAssets;
using System.Collections;
using UnityEngine;

public class StartDialogueOnTrigger : MonoBehaviour
{
    [SerializeField]
    private string nodeDialogue = default;

    [SerializeField]
    private Yarn.Unity.DialogueRunner dialogueRunner = default;

    public float lookAtAnimationDuration = 1.0f;

    bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
        {
            return;
        }

        dialogueRunner.StartDialogue(nodeDialogue);
        triggered = true;

        var cc = other.GetComponent<FirstPersonController>();
        if (cc)
        {
            StartCoroutine(AnimateCharacterLookAtThis(cc));
        }
    }

    private IEnumerator AnimateCharacterLookAtThis(FirstPersonController cc)
    {
        // just manually do it because fuck it
        var dir = (transform.position - cc.GetComponentInChildren<Camera>().transform.position).normalized;
        var rot = Quaternion.LookRotation(dir);

        var rotHorEuler = rot.eulerAngles;
        rotHorEuler.x = 0;
        var rotHor = Quaternion.Euler(rotHorEuler);
        Debug.Log(rot.eulerAngles);

        var initialCameraPitch = cc.GetCameraPitch();
        if (initialCameraPitch > 180.0f) initialCameraPitch -= 360.0f; // getting trolled here
        var desiredPitch = rot.eulerAngles.x;
        if (desiredPitch > 180.0f) desiredPitch -= 360.0f;
        Debug.Log(initialCameraPitch);
        Debug.Log(desiredPitch);

        var counter = 0.0f;
        while (counter < lookAtAnimationDuration)
        {
            counter = Mathf.Min(counter + Time.deltaTime, lookAtAnimationDuration);
            cc.transform.rotation = Quaternion.Slerp(cc.transform.rotation, rotHor, counter);
            cc.SetCameraPitch( Mathf.SmoothStep(initialCameraPitch, desiredPitch, counter));
            yield return null;
        }
    }
}
