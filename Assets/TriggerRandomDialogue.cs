using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class TriggerRandomDialogue : MonoBehaviour
{
    public List<string> nodeNames;

    public DialogueRunner dialogueRunner;

    public void Trigger()
    {
        if (nodeNames.Count > 0)
        {
            var entry = nodeNames[Random.Range(0, nodeNames.Count)];
            if (!dialogueRunner.IsDialogueRunning)
            {
                dialogueRunner.StartDialogue(entry);
            }
        }
    }
}
