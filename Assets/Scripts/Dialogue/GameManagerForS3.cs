using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerForS3 : MonoBehaviour
{
    public DialogueManagerForS3 dialogueManager; // Reference to the DialogueManagerForS3
    public DialogueDatabaseForS3 dialogueDatabase; // Reference to the DialogueDatabaseForS3

    public string nextSceneA; // Scene name for the next scene if Category A is most selected
    public string nextSceneB; // Scene name for the next scene if Category B is most selected

    private void Start()
    {
        // Ensure all required references are assigned
        if (dialogueManager == null || dialogueDatabase == null)
        {
            Debug.LogError("DialogueManager or DialogueDatabase is not assigned in the GameManager.");
            return;
        }

        // Start the initial dialogue sequence
        StartDialogueSequence(1); // Start with the first dialogue (ID 1)
    }

    // Function to start a dialogue sequence by dialogue ID
    public void StartDialogueSequence(int dialogueId)
    {
        DialogueForS3 initialDialogue = dialogueDatabase.GetDialogueById(dialogueId);
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
        Debug.Log("OnDialogueEnded called with nextDialogueId: " + nextDialogueId);

        if (nextDialogueId != -1)
        {
            Debug.Log("Starting next dialogue sequence with ID: " + nextDialogueId);
            StartDialogueSequence(nextDialogueId);
        }
        else
        {
            Debug.Log("Dialogue sequence ended.");
            HandleEndOfScenario();
        }
    }

    // Function to handle the end of the scenario
    public void HandleEndOfScenario()
    {
        string mostSelectedCategory = dialogueManager.GetMostSelectedCategory();
        Debug.Log("Most selected response category: " + mostSelectedCategory);

        // Determine the scene to load based on the most selected category
        string sceneToLoad = mostSelectedCategory == "A" ? nextSceneA : nextSceneB;

        // Load the determined scene
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("No valid scene found for the given category.");
        }
    }
}
