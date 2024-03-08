using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class DebugDialogueController : MonoBehaviour
{
    [Tooltip("Debug trigger node with 'P'")]
    public string debugNodeName;

    private DialogueRunner dialogueRunner;

    void Awake()
    {
        dialogueRunner = GetComponent<DialogueRunner>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            dialogueRunner.StartDialogue(debugNodeName);
        }
    }
}
