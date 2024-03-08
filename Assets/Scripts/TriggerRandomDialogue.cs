using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class TriggerRandomDialogue : MonoBehaviour
{
    public enum RandomMode
    {
        Nodes,
        Prefix
    }

    public RandomMode mode;

    [Header("Nodes")]
    public List<string> nodeNames;

    [Header("Prefix")]
    public string prefix;
    public int count;

    public DialogueRunner dialogueRunner;

    public void Trigger()
    {
        switch (mode)
        {
            case RandomMode.Nodes:
                if (nodeNames.Count > 0)
                {
                    var entry = nodeNames[Random.Range(0, nodeNames.Count)];
                    if (!dialogueRunner.IsDialogueRunning)
                    {
                        dialogueRunner.StartDialogue(entry);
                    }
                }
                break;
            case RandomMode.Prefix:
                int rand = Mathf.FloorToInt(Random.Range(0, count) + 1);
                if (!dialogueRunner.IsDialogueRunning)
                {
                    dialogueRunner.StartDialogue(prefix + rand.ToString());
                }
                break;
            default:
                break;
        }
    }
}
