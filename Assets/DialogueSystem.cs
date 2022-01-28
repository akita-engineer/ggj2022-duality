using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    private const float secondsPerWord = 0.24f;

    public DialogueSequenceScriptableObject dialogueSequence;
    public GameObject textbox;
    public TMP_Text dialogueText;

    private int mCurrentIndex = 0;
    private float mCurrentDuration = 0;
    private bool mRunning = false;

    public void StartSequence()
    {
        if (!mRunning && mCurrentIndex < dialogueSequence.sequence.Count)
        {
            textbox.SetActive(true);
            mRunning = true;
        }
    }

    public void NextDiaogue()
    {
        if (!mRunning)
        {
            return;
        }

        // NPC text
        var dialogue = dialogueSequence.sequence[mCurrentIndex];
        bool isNPCDialogue = dialogue.responses.Count == 0;

        if (isNPCDialogue)
        {
            float duration = GetDurationForCurrentDialogue();
            if (mCurrentDuration < duration)
            {
                mCurrentDuration = duration;
            }
            else
            {
                mCurrentIndex++;
                mCurrentDuration = 0;
            }
        }
    }

    public void SelectChoice()
    {
        // oh my why doesn't this have an argument? ;)
        if (!mRunning)
        {
            return;
        }

        var dialogue = dialogueSequence.sequence[mCurrentIndex];
        bool isNPCDialogue = dialogue.responses.Count == 0;
        if (!isNPCDialogue)
        {
            mCurrentIndex++;
            mCurrentDuration = 0;
        }
    }

    // TODO: Optimize (?)
    private float GetDurationForCurrentDialogue()
    {
        var dialogue = dialogueSequence.sequence[mCurrentIndex];
        if (dialogue.duration <= 0.0f)
        {
            int words = dialogue.text.Split(' ').Length;
            return secondsPerWord * words;
        } 
        else
        {
            return dialogue.duration;
        }
    }

    private void UpdateText()
    {
        if (!mRunning)
        {
            return;
        }

        var dialogue = dialogueSequence.sequence[mCurrentIndex];

        bool isNPCDialogue = dialogue.responses.Count == 0;

        if (isNPCDialogue)
        {
            var characters = dialogue.text.Length;
            var percent = Mathf.Clamp(mCurrentDuration / GetDurationForCurrentDialogue(), 0, 1);
            dialogueText.text = dialogue.text.Substring(0, (int)(characters * percent));
        }
        else
        {
            string text = "";
            for (int i = 0; i < dialogue.responses.Count; i++)
            {
                text += (i + 1) + ": " + dialogue.responses[i] + "\n";
            }
            dialogueText.text = text;
        }
    }

    public void Update()
    {
        // DEBUG
        if (Input.GetKeyDown(KeyCode.Q))
        {
            mCurrentIndex = 0;
            StartSequence();
        }

        // TODO: Update with input system
        if (Input.GetKeyDown(KeyCode.E))
        {
            NextDiaogue();
        }

        // HACK: LOL HORRIBLE STUFF!
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectChoice();
        }

        if (mRunning)
        {
            if (mCurrentIndex >= dialogueSequence.sequence.Count)
            {
                // next time player interacts ensure the dialogue box is hidden
                textbox.SetActive(false);
                mRunning = false;
            }
            else
            {
                mCurrentDuration += Time.deltaTime;
                UpdateText();
            }
        }
    }
}
