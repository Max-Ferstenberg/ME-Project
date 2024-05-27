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

    public Animator leftImageAnimator; // Animator for left image
    public Animator centerImageAnimator; // Animator for center image
    public Animator rightImageAnimator; // Animator for right image
    public Image leftImageComponent; // Image component for the left image
    public Image centerImageComponent; // Image component for the center image
    public Image rightImageComponent; // Image component for the right image

    private Dialogue currentDialogue;
    private Coroutine typeSentenceCoroutine;
    private bool buttonClicked = false; // Flag to prevent multiple clicks
    private bool isTransitioning = false; // Flag to prevent redundant updates

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
        if (nextButton == null || dialogueText == null || responsePanel == null || dialogueDatabase == null || leftImageComponent == null || centerImageComponent == null || rightImageComponent == null)
        {
            Debug.LogError("One or more required references are missing in DialogueManager.");
            return;
        }

        nextButton.gameObject.SetActive(false); // Ensure the Next button is hidden initially
        responsePanel.SetActive(false); // Ensure the response panel is hidden initially

        // Set default images at the start of the game
        leftImageComponent.gameObject.SetActive(true);
        centerImageComponent.gameObject.SetActive(true);
        rightImageComponent.gameObject.SetActive(true);

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
            GenerateResponseButtons(currentDialogue.responseIDs); // Show response buttons
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
        if (!isTransitioning)
        {
            UpdateImageStates(dialogue);
            TriggerFadeInAnimations(dialogue);
        }

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
            DisplayDialogue(dialogueToStart);
        }
        else
        {
            Debug.LogError("No dialogue found with ID: " + dialogueId);
        }
    }

    void GenerateResponseButtons(int[] responseIDs)
    {
        if (responsePanel == null || responsePrefab == null)
        {
            Debug.LogError("Response panel or response prefab is not assigned.");
            return;
        }

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

        StartDialogueById(response.nextDialogueID);
    }

    private void UpdateImageStates(Dialogue dialogue)
    {
        Debug.Log("Updating image states for dialogue ID: " + dialogue.id);

        // Update left image
        if (dialogue.isLeftImageVisible && dialogue.leftImage != null)
        {
            leftImageComponent.sprite = dialogue.leftImage;
            leftImageComponent.color = Color.white;
            leftImageComponent.gameObject.SetActive(true);
            ResetAnimatorState(leftImageAnimator);
            leftImageAnimator.Play("Idle");
            leftImageAnimator.SetBool("isTalking", dialogue.isLeftImageTalking);
        }
        else
        {
            leftImageComponent.sprite = null;
            leftImageComponent.color = new Color(0, 0, 0, 0);
            leftImageComponent.gameObject.SetActive(false);
        }

        // Update center image
        if (dialogue.isCenterImageVisible && dialogue.centerImage != null)
        {
            centerImageComponent.sprite = dialogue.centerImage;
            centerImageComponent.color = Color.white;
            centerImageComponent.gameObject.SetActive(true);
            ResetAnimatorState(centerImageAnimator);
            centerImageAnimator.Play("Idle");
            centerImageAnimator.SetBool("isTalking", dialogue.isCenterImageTalking);
        }
        else
        {
            centerImageComponent.sprite = null;
            centerImageComponent.color = new Color(0, 0, 0, 0);
            centerImageComponent.gameObject.SetActive(false);
        }

        // Update right image
        if (dialogue.isRightImageVisible && dialogue.rightImage != null)
        {
            rightImageComponent.sprite = dialogue.rightImage;
            rightImageComponent.color = Color.white;
            rightImageComponent.gameObject.SetActive(true);
            ResetAnimatorState(rightImageAnimator);
            rightImageAnimator.Play("Idle");
            rightImageAnimator.SetBool("isTalking", dialogue.isRightImageTalking);
        }
        else
        {
            rightImageComponent.sprite = null;
            rightImageComponent.color = new Color(0, 0, 0, 0);
            rightImageComponent.gameObject.SetActive(false);
        }

        // Apply mirroring
        ApplyMirroring(dialogue);
    }


    private void ApplyMirroring(Dialogue dialogue)
    {
        // Apply mirroring for left image
        if (dialogue.isLeftImageVisible && dialogue.leftImage != null)
        {
            Debug.Log($"Applying mirroring to left image: {dialogue.isLeftImageMirrored}");
            leftImageComponent.rectTransform.localScale = new Vector3(dialogue.isLeftImageMirrored ? -1 : 1, 1, 1);
        }
        else
        {
            leftImageComponent.rectTransform.localScale = Vector3.one;
        }

        // Apply mirroring for center image
        if (dialogue.isCenterImageVisible && dialogue.centerImage != null)
        {
            Debug.Log($"Applying mirroring to center image: {dialogue.isCenterImageMirrored}");
            centerImageComponent.rectTransform.localScale = new Vector3(dialogue.isCenterImageMirrored ? -1 : 1, 1, 1);
        }
        else
        {
            centerImageComponent.rectTransform.localScale = Vector3.one;
        }

        // Apply mirroring for right image
        if (dialogue.isRightImageVisible && dialogue.rightImage != null)
        {
            Debug.Log($"Applying mirroring to right image: {dialogue.isRightImageMirrored}");
            rightImageComponent.rectTransform.localScale = new Vector3(dialogue.isRightImageMirrored ? -1 : 1, 1, 1);
        }
        else
        {
            rightImageComponent.rectTransform.localScale = Vector3.one;
        }
    }


    private void TriggerFadeInAnimations(Dialogue dialogue)
    {
        Debug.Log("Triggering fade-in animations");

        if (dialogue.shouldLeftImageFadeIn && dialogue.isLeftImageVisible && dialogue.leftImage != null)
        {
            Debug.Log("Left image fade-in");
            if (leftImageComponent.gameObject.activeSelf)
            {
                leftImageAnimator.Play("FadeIn");
                StartCoroutine(HandleFadeInCompletion(leftImageAnimator, dialogue.isLeftImageTalking, dialogue.isLeftImageMirrored, leftImageComponent));
            }
        }

        if (dialogue.shouldCenterImageFadeIn && dialogue.isCenterImageVisible && dialogue.centerImage != null)
        {
            Debug.Log("Center image fade-in");
            if (centerImageComponent.gameObject.activeSelf)
            {
                centerImageAnimator.Play("FadeIn");
                StartCoroutine(HandleFadeInCompletion(centerImageAnimator, dialogue.isCenterImageTalking, dialogue.isCenterImageMirrored, centerImageComponent));
            }
        }

        if (dialogue.shouldRightImageFadeIn && dialogue.isRightImageVisible && dialogue.rightImage != null)
        {
            Debug.Log("Right image fade-in");
            if (rightImageComponent.gameObject.activeSelf)
            {
                rightImageAnimator.Play("FadeIn");
                StartCoroutine(HandleFadeInCompletion(rightImageAnimator, dialogue.isRightImageTalking, dialogue.isRightImageMirrored, rightImageComponent));
            }
        }
    }

    private void TriggerFadeOutAnimations(Dialogue dialogue)
    {
        Debug.Log("Triggering fade-out animations");

        if (dialogue.shouldLeftImageFadeOut && leftImageComponent.gameObject.activeSelf)
        {
            Debug.Log("Left image fade-out");
            StartCoroutine(TransitionToIdleAndFadeOut(leftImageAnimator));
        }

        if (dialogue.shouldCenterImageFadeOut && centerImageComponent.gameObject.activeSelf)
        {
            Debug.Log("Center image fade-out");
            StartCoroutine(TransitionToIdleAndFadeOut(centerImageAnimator));
        }

        if (dialogue.shouldRightImageFadeOut && rightImageComponent.gameObject.activeSelf)
        {
            Debug.Log("Right image fade-out");
            StartCoroutine(TransitionToIdleAndFadeOut(rightImageAnimator));
        }
    }

    private IEnumerator HandleFadeInCompletion(Animator animator, bool isTalking, bool isMirrored, Image imageComponent)
    {
        yield return new WaitForSeconds(1.0f); // Adjust duration as necessary
        animator.Play("Idle"); // Explicitly set to Idle state
        yield return new WaitForSeconds(0.1f); // Ensure Idle state is set
        animator.SetBool("isTalking", isTalking); // Set talking state after Idle
        imageComponent.rectTransform.localScale = new Vector3(isMirrored ? -1 : 1, 1, 1); // Apply mirroring
    }

    private IEnumerator TransitionToIdleAndFadeOut(Animator animator)
    {
        animator.Play("Idle"); // Ensure it goes to Idle first
        yield return new WaitForSeconds(0.1f); // Small delay to ensure transition to Idle
        animator.Play("FadeOut");
        yield return new WaitForSeconds(1.0f); // Adjust duration as necessary
        animator.SetBool("isFadingOut", false); // Reset fading out state
    }

    private IEnumerator ResetAnimationParameter(Animator animator, string parameter, float delay)
    {
        Debug.Log($"Resetting animation parameter {parameter} after {delay} seconds");
        yield return new WaitForSeconds(delay);
        animator.SetBool(parameter, false);
    }

    private void ResetAnimatorState(Animator animator)
    {
        animator.SetBool("isTalking", false);
        animator.SetBool("isFadingIn", false);
        animator.SetBool("isFadingOut", false);
    }

    private void TransitionAllToIdle()
    {
        if (leftImageComponent.gameObject.activeSelf)
        {
            leftImageAnimator.SetBool("isTalking", false);
            leftImageAnimator.Play("Idle");
        }
        if (centerImageComponent.gameObject.activeSelf)
        {
            centerImageAnimator.SetBool("isTalking", false);
            centerImageAnimator.Play("Idle");
        }
        if (rightImageComponent.gameObject.activeSelf)
        {
            rightImageAnimator.SetBool("isTalking", false);
            rightImageAnimator.Play("Idle");
        }
    }

    public void OnNextButtonClicked()
    {
        Debug.Log("OnNextButtonClicked called");
        if (buttonClicked) return; // Prevent multiple clicks

        buttonClicked = true; // Set flag
        Debug.Log("Next button clicked");

        TransitionAllToIdle(); // Ensure all characters transition to Idle state

        StartCoroutine(DisableButtonTemporarily());

        if (currentDialogue.hasResponses)
        {
            GenerateResponseButtons(currentDialogue.responseIDs); // Show response buttons
        }
        else if (currentDialogue.nextDialogueID != -1)
        {
            Debug.Log("Transitioning to next dialogue with ID: " + currentDialogue.nextDialogueID);

            // Trigger fade out animations for characters not in the next dialogue
            StartCoroutine(HandleDialogueTransition(currentDialogue));
        }
        else
        {
            Debug.Log("No more dialogues");
        }

        if (nextButton != null) nextButton.gameObject.SetActive(false); // Hide the Next button after clicking
    }

    private IEnumerator HandleDialogueTransition(Dialogue dialogue)
    {
        if (isTransitioning)
        {
            Debug.Log("Skipping HandleDialogueTransition because a transition is in progress");
            yield break;
        }

        isTransitioning = true;

        TriggerFadeOutAnimations(dialogue);

        // Wait for fade-out animations to complete
        yield return new WaitForSeconds(1.0f); // Adjust duration as necessary

        // Proceed to the next dialogue after animations complete
        Dialogue nextDialogue = dialogueDatabase.GetDialogueById(dialogue.nextDialogueID);
        if (nextDialogue != null)
        {
            // Ensure UpdateImageStates is called only once
            Debug.Log("Updating image states before fade-in");
            UpdateImageStates(nextDialogue); // Update image states before fade-in
            yield return new WaitForSeconds(0.1f); // Ensure state updates are applied

            Debug.Log("Triggering fade-in animations for the next dialogue");
            TriggerFadeInAnimations(nextDialogue); // Trigger fade-in animations for the next dialogue
            yield return new WaitForSeconds(1.0f); // Ensure fade-in animations complete before starting dialogue

            Debug.Log("Displaying next dialogue");
            DisplayDialogue(nextDialogue); // Start the new dialogue
        }
        else
        {
            Debug.LogError("No dialogue found with ID: " + dialogue.nextDialogueID);
        }

        isTransitioning = false;
    }

    private IEnumerator WaitForAnimationsToComplete(System.Action onComplete)
    {
        Debug.Log("Waiting for animations to complete");
        yield return new WaitForSeconds(1.0f); // Adjust duration as necessary
        onComplete?.Invoke();
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
