using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public DialogueManager dialogueManager; // Reference to the DialogueManager
    public DialogueDatabase dialogueDatabase; // Reference to the DialogueDatabase

    private void Start()
    {
        // Start the initial dialogue sequence
        StartDialogueSequence(0); // Start with the first dialogue (ID 0)
    }

    // Function to start a dialogue sequence by dialogue ID
    public void StartDialogueSequence(int dialogueId)
    {
        Dialogue initialDialogue = dialogueDatabase.GetDialogueById(dialogueId);
        if (initialDialogue != null)
        {
            dialogueManager.StartDialogueById(dialogueId);
        }
        else
        {
            Debug.LogError("No initial dialogue found with ID: " + dialogueId);
        }
    }

    // Function to handle when a dialogue ends and next dialogue should start
    public void OnDialogueEnded(int nextDialogueId)
    {
        if (nextDialogueId != -1)
        {
            StartDialogueSequence(nextDialogueId);
        }
        else
        {
            Debug.Log("Dialogue sequence ended.");
        }
    }
}

