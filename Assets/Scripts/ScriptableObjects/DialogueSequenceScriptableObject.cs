using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/DialogueSequence", order = 1)]
public class DialogueSequenceScriptableObject : ScriptableObject
{
    public string sequenceName;

    public List<Dialogue> sequence;
}

[System.Serializable]
public struct Dialogue
{
    public CharacterScriptableObject speaker;
    public string text;
    public float duration; // use 0 for auto-time
    // TODO: VO

    // HACK: This is a horrible hack because I forgot we needed responses from the player.
    // anything with responses will be interpreted to be used by the player (speaker + text should not be used)
    public List<string> responses;
}
