using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public TMP_Text dialogueText; // TextMeshPro text component
    public GameObject responsePanel; // Panel to hold buttons
    public Button responsePrefab; // Prefab for response buttons
    public Button nextButton; // Reference to the Next button
    public DialogueDatabase dialogueDatabase; // Reference to the Dialogue Database
    public Animator dialogueBoxAnimator; // Reference to the Animator

    public Animator playerCharacterAnimator; // Animator for player character
    public Animator npcCharacterAnimator; // Animator for NPCs
    public Image playerCharacterImage; // Image component for the player character
    public Image npcCharacterImage; // Image component for NPCs

    private Dialogue currentDialogue;
    private Coroutine typeSentenceCoroutine;
    private bool buttonClicked = false; // Flag to prevent multiple clicks

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        Debug.Log("DialogueManager Instance Created");
    }

    void Start()
    {
        // Ensure all necessary references are assigned
        if (nextButton == null || dialogueText == null || responsePanel == null || dialogueDatabase == null || playerCharacterImage == null || npcCharacterImage == null)
        {
            Debug.LogError("One or more required references are missing in DialogueManager.");
            return;
        }

        nextButton.gameObject.SetActive(false); // Ensure the Next button is hidden initially
        responsePanel.SetActive(false); // Ensure the response panel is hidden initially

        // Set default images at the start of the game
        playerCharacterImage.gameObject.SetActive(true);
        npcCharacterImage.gameObject.SetActive(true);
        
        // Remove all listeners to avoid multiple registrations
        nextButton.onClick.RemoveAllListeners();
        Debug.Log("Adding listener to Next button");
        nextButton.onClick.AddListener(OnNextButtonClicked);
    }


    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.01f); // Adjust timing for typewriter speed
        }

        if (currentDialogue.hasResponses)
        {
            Debug.Log("Triggering MoveUp animation");
            dialogueBoxAnimator.SetTrigger("MoveUp"); // Move the dialogue box up for responses
            StartCoroutine(WaitForAnimatorState("MoveUp", () =>
            {
                GenerateResponseButtons(currentDialogue.responseIDs); // Show response buttons
            }));
        }
        else if (currentDialogue.nextDialogueID != -1)
        {
            EnableNextButton(); // Enable the Next button if there is a next dialogue
        }
    }

    public void DisplayDialogue(Dialogue dialogue)
    {
        currentDialogue = dialogue;
        if (typeSentenceCoroutine != null)
        {
            StopCoroutine(typeSentenceCoroutine);
        }
        buttonClicked = false; // Reset flag

        // Ensure the character animations are updated before displaying the dialogue
        ShowCharacterImage(dialogue);
        typeSentenceCoroutine = StartCoroutine(TypeSentence(dialogue.text)); // Start typewriter effect
        responsePanel.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false); // Initially hide the Next button
    }

    public void StartDialogueById(int dialogueId)
    {
        Debug.Log("StartDialogueById called with ID: " + dialogueId);
        Dialogue dialogueToStart = dialogueDatabase.GetDialogueById(dialogueId);
        if (dialogueToStart != null)
        {
            // Explicitly set the initial animator states
            if (dialogueToStart.isPlayerCharacter)
            {
                playerCharacterAnimator.SetBool("isTalking", true);
                npcCharacterAnimator.SetBool("isTalking", false);
            }
            else
            {
                playerCharacterAnimator.SetBool("isTalking", false);
                npcCharacterAnimator.SetBool("isTalking", true);
            }

            DisplayDialogue(dialogueToStart);
        }
        else
        {
            Debug.LogError("No dialogue found with ID: " + dialogueId);
        }
    }

    void GenerateResponseButtons(int[] responseIDs)
    {
        foreach (Transform child in responsePanel.transform)
        {
            Destroy(child.gameObject); // Clear out existing buttons to prevent duplication
        }

        responsePanel.SetActive(true); // Show the response panel

        foreach (int id in responseIDs)
        {
            Response response = dialogueDatabase.GetResponseById(id);
            if (response != null)
            {
                Button button = Instantiate(responsePrefab, responsePanel.transform);
                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

                if (buttonText != null)
                {
                    buttonText.text = response.text;
                }
                else
                {
                    Debug.LogError("Button prefab is missing a TMP_Text component.");
                }

                button.onClick.AddListener(() => ResponseButtonClicked(response));
                button.gameObject.SetActive(true); // Ensure the button is active
                button.interactable = true; // Enable interaction immediately
            }
            else
            {
                Debug.LogError("No response found for ID: " + id);
            }
        }
    }

    private void ResponseButtonClicked(Response response)
    {
        Debug.Log("Response button clicked with ID: " + response.id);

        foreach (Button btn in responsePanel.GetComponentsInChildren<Button>())
        {
            btn.onClick.RemoveAllListeners(); // Remove all listeners from the button when clicked
        }

        dialogueText.gameObject.SetActive(true); // Show dialogue text again
        responsePanel.SetActive(false); // Hide the response panel

        StartCoroutine(WaitForAnimatorState("MoveUp", () =>
        {
            Debug.Log("Triggering MoveDown animation");
            dialogueBoxAnimator.SetTrigger("MoveDown"); // Move the dialogue box back down
            StartCoroutine(WaitForAnimatorState("MoveDown", () =>
            {
                StartDialogueById(response.nextDialogueID);
            }));
        }));
    }

    private IEnumerator WaitForAnimatorState(string stateName, System.Action onComplete)
    {
        Debug.Log($"Waiting for Animator to reach state: {stateName}");
        while (!IsAnimatorInState(stateName))
        {
            yield return null;
        }
        Debug.Log($"Animator reached state: {stateName}");
        onComplete?.Invoke();
    }

    private bool IsAnimatorInState(string stateName)
    {
        AnimatorStateInfo stateInfo = dialogueBoxAnimator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(stateName);
    }

    private void ShowCharacterImage(Dialogue dialogue)
    {
        if (dialogue.hasSprite && dialogue.characterImage != null)
        {
            if (dialogue.isPlayerCharacter)
            {
                npcCharacterAnimator.SetBool("isTalking", false); // Ensure NPC is in idle state
                playerCharacterImage.sprite = dialogue.characterImage;
                playerCharacterImage.gameObject.SetActive(true);
                playerCharacterAnimator.SetBool("isTalking", true);
            }
            else
            {
                playerCharacterAnimator.SetBool("isTalking", false); // Ensure player is in idle state
                npcCharacterImage.sprite = dialogue.characterImage;
                npcCharacterImage.gameObject.SetActive(true);
                npcCharacterAnimator.SetBool("isTalking", true);
            }
        }
        else
        {
            // Keep the current sprite visible and set to idle state
            if (dialogue.isPlayerCharacter)
            {
                playerCharacterAnimator.SetBool("isTalking", false);
                npcCharacterAnimator.SetBool("isTalking", false);
                playerCharacterImage.gameObject.SetActive(true);
            }
            else
            {
                playerCharacterAnimator.SetBool("isTalking", false);
                npcCharacterAnimator.SetBool("isTalking", false);
                npcCharacterImage.gameObject.SetActive(true);
            }
        }
    }


    private void StartAnimationForDialogue(Dialogue dialogue)
    {
        // Ensure the animations are triggered at the right time
        if (dialogue.isPlayerCharacter)
        {
            playerCharacterAnimator.SetTrigger("Talking");
            npcCharacterAnimator.SetTrigger("Idle");
        }
        else
        {
            playerCharacterAnimator.SetTrigger("Idle");
            npcCharacterAnimator.SetTrigger("Talking");
        }
    }

    public void OnNextButtonClicked()
    {
        Debug.Log("OnNextButtonClicked called");
        if (buttonClicked) return; // Prevent multiple clicks

        buttonClicked = true; // Set flag
        Debug.Log("Next button clicked");

        StartCoroutine(DisableButtonTemporarily());

        if (currentDialogue.hasResponses)
        {
            Debug.Log("Current dialogue has responses");
            dialogueBoxAnimator.SetTrigger("MoveUp"); // Move the dialogue box up for responses
            StartCoroutine(WaitForAnimatorState("MoveUp", () =>
            {
                GenerateResponseButtons(currentDialogue.responseIDs); // Show response buttons
            }));
        }
        else if (currentDialogue.nextDialogueID != -1)
        {
            Debug.Log("Transitioning to next dialogue with ID: " + currentDialogue.nextDialogueID);
            StartDialogueById(currentDialogue.nextDialogueID); // Continue to next dialogue
        }
        else
        {
            Debug.Log("No more dialogues");
        }

        if (nextButton != null) nextButton.gameObject.SetActive(false); // Hide the Next button after clicking
    }

    private IEnumerator DisableButtonTemporarily()
    {
        if (nextButton != null) nextButton.interactable = false; // Disable the button immediately
        yield return new WaitForSeconds(0.5f); // Adjust the delay as necessary
        if (nextButton != null) nextButton.interactable = true; // Re-enable the button
        buttonClicked = false; // Reset flag
    }

    private void EnableNextButton()
    {
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true); // Show the Next button
            nextButton.interactable = true; // Re-enable the button
            buttonClicked = false; // Reset flag
        }
    }
}
