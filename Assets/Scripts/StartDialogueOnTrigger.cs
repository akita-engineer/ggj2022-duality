using UnityEngine;

public class StartDialogueOnTrigger : MonoBehaviour
{
    [SerializeField]
    private string nodeDialogue = default;

    [SerializeField]
    private Yarn.Unity.DialogueRunner dialogueRunner = default;

    bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
        {
            return;
        }

        dialogueRunner.StartDialogue(nodeDialogue);
        triggered = true;
    }
}
