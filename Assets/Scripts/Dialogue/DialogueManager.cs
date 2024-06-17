using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    // Introductory Text Flag
    private bool introductoryTextShown = false;
    private bool isTyping = false;

    // UI Components
    public TMP_Text dialogueText;
    public Button nextButton;
    public TMP_Text introText;
    public Image backgroundImageComponent;
    public RectTransform dialogueBoxRectTransform;
    public Button responseButton1;
    public Button responseButton2;
    public RectTransform responseContainer;
    public Button skipButton;


    // Dialogue Database
    public DialogueDatabase dialogueDatabase;

    // Image Animators and Components
    public Animator leftImageAnimator;
    public Animator centerImageAnimator;
    public Animator rightImageAnimator;
    public Image leftImageComponent;
    public Image centerImageComponent;
    public Image rightImageComponent;

    // Game Manager
    public GameManager gameManager;

    // Introductory Text Properties
    public string introductoryText;
    public Color introductoryTextColor = Color.cyan; // Customize as needed

    // Fade Properties
    public float fadeDuration = 1.0f;
    private bool fadeOutBackground = false;
    private bool fadeInBackground = false;

    // Internal State
    private Dialogue currentDialogue;
    private Coroutine typeSentenceCoroutine;
    private bool buttonClicked = false;
    private bool isTransitioning = false;
    private bool isInitialized = false;
    private bool isSkippingText = false;

    // Response Tracking
    private Dictionary<string, int> responseCategoryCounts = new Dictionary<string, int>();

    // Initial Positions
    private Vector3 leftImageInitialPosition;
    private Vector3 centerImageInitialPosition;
    private Vector3 rightImageInitialPosition;

    // Store the ID of the actual starting dialogue
    private int actualStartingDialogueID;

    // Initialization
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject); // Ensure the root GameObject is marked to not be destroyed
        }
        Debug.Log("DialogueManager Instance Created");
    }


    private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

void Start()
{
    Debug.Log("DialogueManager Start Method Called");

    if (nextButton == null || skipButton == null || dialogueText == null || dialogueDatabase == null ||
        leftImageComponent == null || centerImageComponent == null || rightImageComponent == null ||
        responseButton1 == null || responseButton2 == null || backgroundImageComponent == null ||
        dialogueBoxRectTransform == null || responseContainer == null || introText == null)
    {
        Debug.LogError("One or more required references are missing in DialogueManager.");
        return;
    }

    leftImageInitialPosition = leftImageComponent.rectTransform.localPosition;
    centerImageInitialPosition = centerImageComponent.rectTransform.localPosition;
    rightImageInitialPosition = rightImageComponent.rectTransform.localPosition;

    leftImageComponent.gameObject.SetActive(true);
    centerImageComponent.gameObject.SetActive(true);
    rightImageComponent.gameObject.SetActive(true);

    nextButton.onClick.RemoveAllListeners();
    skipButton.onClick.RemoveAllListeners();
    Debug.Log("Adding listeners to Next and Skip buttons");
    nextButton.onClick.AddListener(OnNextButtonClicked);
    skipButton.onClick.AddListener(OnSkipButtonClicked);

    InitializeComponents();
    Debug.Log("Calling StartDialogue for testing");
    StartDialogue(7569); // Assuming 7569 is the starting dialogue ID, adjust as necessary
}






    private void InitializeComponents()
    {
        Debug.Log("Initializing Components...");
        if (leftImageComponent != null) Debug.Log("Left Image Component Initialized.");
        if (centerImageComponent != null) Debug.Log("Center Image Component Initialized.");
        if (rightImageComponent != null) Debug.Log("Right Image Component Initialized.");
        if (dialogueDatabase != null) Debug.Log("Dialogue Database Initialized.");
        if (nextButton != null) Debug.Log("Next Button Initialized.");
        if (dialogueText != null) Debug.Log("Dialogue Text Initialized.");
        if (responseButton1 != null) Debug.Log("Response Button 1 Initialized.");
        if (responseButton2 != null) Debug.Log("Response Button 2 Initialized.");
        if (backgroundImageComponent != null) Debug.Log("Background Image Component Initialized.");
        if (dialogueBoxRectTransform != null) Debug.Log("Dialogue Box RectTransform Initialized.");
        if (responseContainer != null) Debug.Log("Response Container Initialized.");
        if (introText != null) Debug.Log("Intro Text Initialized.");
        if (gameManager != null) Debug.Log("Game Manager Initialized.");

        if (leftImageComponent != null && centerImageComponent != null && rightImageComponent != null)
        {
            isInitialized = true;
            Debug.Log("All required components are initialized.");
        }
        else
        {
            Debug.LogError("Failed to initialize components. Check your component assignments.");
        }
    }

    public void StartDialogue(int dialogueId)
    {
        Debug.Log("StartDialogue called with ID: " + dialogueId);

        if (!isInitialized)
        {
            Debug.LogError("DialogueManager is not initialized. Aborting dialogue sequence.");
            return;
        }

        // Always start with dialogue ID 7569
        dialogueId = 7569;

        actualStartingDialogueID = dialogueId;

        if (!string.IsNullOrEmpty(introductoryText) && !introductoryTextShown)
        {
            Debug.Log("Introductory text detected, showing it first.");
            ShowIntroductoryText();
        }
        else
        {
            Debug.Log("No introductory text, starting dialogue directly.");
            StartDialogueById(dialogueId);
        }
    }

    private void ShowIntroductoryText()
    {
        Debug.Log("Showing introductory text");
        introText.color = introductoryTextColor;
        introText.text = introductoryText;
        introText.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);
        introductoryTextShown = true; // Set the flag here
        Debug.Log("Introductory text shown: " + introductoryText);
    }

public void OnNextButtonClicked()
{
    Debug.Log("OnNextButtonClicked called");

    if (isTyping)
    {
        Debug.Log("Button pressed while typing, ignoring next action.");
        return;
    }

    Debug.Log("Next button in Next state");
    buttonClicked = true;
    ProceedToNextDialogue();
}


public void OnSkipButtonClicked()
{
    Debug.Log("OnSkipButtonClicked called");

    if (isTyping)
    {
        Debug.Log("Skip button in Skip state");
        if (typeSentenceCoroutine != null)
        {
            StopCoroutine(typeSentenceCoroutine);
            dialogueText.text = currentDialogue.text; // Immediately display the full text
            typeSentenceCoroutine = null;
            isTyping = false;

            Debug.Log("Skipped typing, enabling next button");
            EnableNextButton();
        }
    }
}




    private IEnumerator AllowNextAction()
    {
        yield return new WaitForSeconds(0.1f); // Short delay to allow button release
        buttonClicked = false;
        SetNextButtonState(false); // Set button to Next state
    }


    private IEnumerator WaitAndAllowNextButton()
    {
        yield return new WaitForEndOfFrame();
        buttonClicked = false;
        Debug.Log("Button click state reset.");
        SetNextButtonState(false); // Set button to Next state
    }

    private void ProceedToNextDialogue()
    {
        Debug.Log("Proceeding to the next dialogue");

        TransitionAllToIdle();

        StartCoroutine(DisableButtonTemporarily());

        if (currentDialogue.hasResponses)
        {
            Debug.Log("Generating response buttons for current dialogue.");
            GenerateResponseButtons(currentDialogue.responseIDs);
        }
        else if (currentDialogue.nextDialogueID != -1)
        {
            Debug.Log("Transitioning to next dialogue with ID: " + currentDialogue.nextDialogueID);
            StartDialogueById(currentDialogue.nextDialogueID);
        }
        else
        {
            Debug.Log("No more dialogues");
        }
    }
    private IEnumerator DelayAfterSkip()
    {
        yield return new WaitForSeconds(0.2f); // Adjust the delay as needed
        isTyping = false;
        buttonClicked = false;
        SetNextButtonState(false); // Set button to Next state
    }



    private IEnumerator DelayNextButtonAction()
    {
        yield return new WaitForEndOfFrame();
        buttonClicked = false;
    }

    private void OnNextButtonPointerDown(BaseEventData eventData)
    {
        if (isTyping)
        {
            isSkippingText = true;
        }
    }

    private void OnNextButtonPointerUp(BaseEventData eventData)
    {
        if (isSkippingText)
        {
            isSkippingText = false;
        }
    }


private void SetNextButtonState(bool isTyping)
{
    this.isTyping = isTyping;

    nextButton.onClick.RemoveAllListeners();
    skipButton.onClick.RemoveAllListeners();

    if (isTyping)
    {
        Debug.Log("Setting next button to Skip state");
        nextButton.gameObject.SetActive(false); // Hide next button while typing
        skipButton.gameObject.SetActive(true);  // Show skip button while typing
        skipButton.onClick.AddListener(OnSkipButtonClicked);
    }
    else
    {
        Debug.Log("Setting next button to Next state");
        nextButton.gameObject.SetActive(true); // Show next button after typing
        skipButton.gameObject.SetActive(false); // Hide skip button after typing
        nextButton.onClick.AddListener(OnNextButtonClicked);
    }
}






    private void SkipToTextEnd()
    {
        Debug.Log("Skipping to the end of the text");

        if (typeSentenceCoroutine != null)
        {
            StopCoroutine(typeSentenceCoroutine);
            dialogueText.text = currentDialogue.text; // Immediately display the full text
            typeSentenceCoroutine = null;
            SetNextButtonState(false); // Set button to Next state after skipping
        }
    }


    private IEnumerator HandleDialogueTransition(Dialogue dialogue)
    {
        if (isTransitioning)
        {
            Debug.Log("Skipping HandleDialogueTransition because a transition is in progress");
            yield break;
        }

        isTransitioning = true;

        // Smoothly transition from talking to idle for all active image components
        if (leftImageComponent.gameObject.activeSelf && leftImageAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talking"))
        {
            TriggerTalkingToIdleTransition(leftImageAnimator, 0.5f);
        }
        if (centerImageComponent.gameObject.activeSelf && centerImageAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talking"))
        {
            TriggerTalkingToIdleTransition(centerImageAnimator, 0.5f);
        }
        if (rightImageComponent.gameObject.activeSelf && rightImageAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talking"))
        {
            TriggerTalkingToIdleTransition(rightImageAnimator, 0.5f);
        }

        if (fadeOutBackground)
        {
            yield return StartCoroutine(FadeOutBackground());
        }

        TriggerFadeOutAnimations(dialogue);

        yield return new WaitForSeconds(1.0f);

        Dialogue nextDialogue = dialogueDatabase.GetDialogueById(dialogue.nextDialogueID);
        if (nextDialogue != null)
        {
            Debug.Log("Updating image states before fade-in");
            UpdateImageStates(nextDialogue);
            yield return new WaitForSeconds(0.1f);

            Debug.Log("Triggering fade-in animations for the next dialogue");
            TriggerFadeInAnimations(nextDialogue); // Ensure this method call is correct
            yield return new WaitForSeconds(1.0f);

            if (nextDialogue.fadeInBackground && nextDialogue.backgroundImage != null)
            {
                StartCoroutine(FadeInBackground(nextDialogue.backgroundImage));
            }

            Debug.Log("Displaying next dialogue");
            DisplayDialogue(nextDialogue);
        }
        else
        {
            Debug.LogError("No dialogue found with ID: " + dialogue.nextDialogueID);
        }

        isTransitioning = false;
    }

    // Section: Dialogue Management
    public void DisplayDialogue(Dialogue dialogue)
    {
        Debug.Log("Displaying dialogue with ID: " + dialogue.id);
        currentDialogue = dialogue;
        if (typeSentenceCoroutine != null)
        {
            StopCoroutine(typeSentenceCoroutine);
        }
        buttonClicked = false;

        fadeOutBackground = dialogue.fadeOutBackground;
        fadeInBackground = dialogue.fadeInBackground;

        if (!isTransitioning)
        {
            Debug.Log("Updating image states and triggering fade-in animations.");
            UpdateImageStates(dialogue);
            TriggerFadeInAnimations(dialogue);

            if (fadeInBackground && dialogue.backgroundImage != null)
            {
                Debug.Log("Fading in background image.");
                StartCoroutine(FadeInBackground(dialogue.backgroundImage));
            }
        }

        dialogueText.color = dialogue.textColor; // Set the text color
        dialogueText.enableAutoSizing = true; // Enable auto-sizing
        dialogueText.fontSizeMin = 10; // Set a reasonable minimum size
        dialogueText.fontSizeMax = 50; // Set a reasonable maximum size
        dialogueText.text = ""; // Clear text to trigger auto-size recalculation
        dialogueText.text = dialogue.text; // Temporarily set text to get proper size

        // Adjust the final font size to fit
        dialogueText.ForceMeshUpdate();
        var textInfo = dialogueText.textInfo;
        float newSize = dialogueText.fontSize;

        while (textInfo.lineCount > 1 && newSize > dialogueText.fontSizeMin)
        {
            newSize--;
            dialogueText.fontSize = newSize;
            dialogueText.ForceMeshUpdate();
            textInfo = dialogueText.textInfo;
        }

        dialogueText.text = ""; // Clear text again for typing effect
        typeSentenceCoroutine = StartCoroutine(TypeSentence(dialogue.text));
        responseButton1.gameObject.SetActive(false);
        responseButton2.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(true);
    }

IEnumerator TypeSentence(string sentence)
{
    Debug.Log("Typing sentence: " + sentence);
    dialogueText.text = "";
    isTyping = true;
    SetNextButtonState(true); // Set button to Skip state

    foreach (char letter in sentence.ToCharArray())
    {
        dialogueText.text += letter;
        yield return new WaitForSeconds(0.01f);
    }

    Debug.Log("Finished typing sentence.");
    isTyping = false;
    SetNextButtonState(false); // Set button to Next state

    if (currentDialogue.hasResponses)
    {
        Debug.Log("Current dialogue has responses, generating buttons.");
        GenerateResponseButtons(currentDialogue.responseIDs);
    }
    else if (currentDialogue.nextDialogueID != -1)
    {
        Debug.Log("Enabling next button for next dialogue.");
        EnableNextButton();
    }
    else if (currentDialogue.isEndDialogue)
    {
        Debug.Log("End of dialogue sequence reached.");
        EndScenario();
    }
}






    public void StartDialogueById(int dialogueId)
    {
        Debug.Log("StartDialogueById called with ID: " + dialogueId);

        if (dialogueId == 0 && !introductoryTextShown)
        {
            // Starting dialogue with introductory text
            Debug.Log("Initial dialogue ID detected, starting with introductory dialogue ID 7569.");
            dialogueId = 7569;
            introductoryTextShown = true; // Set the flag to true to avoid loop
        }

        Dialogue dialogueToStart = dialogueDatabase.GetDialogueById(dialogueId);
        if (dialogueToStart != null)
        {
            Debug.Log("Dialogue found with ID: " + dialogueId);
            DisplayDialogue(dialogueToStart);
        }
        else
        {
            Debug.LogError("No dialogue found with ID: " + dialogueId);
        }
    }

    // Section: Response Management
    void GenerateResponseButtons(int[] responseIDs)
    {
        if (responseButton1 == null || responseButton2 == null)
        {
            Debug.LogError("Response buttons are not assigned.");
            return;
        }

        responseButton1.gameObject.SetActive(false);
        responseButton2.gameObject.SetActive(false);

        if (responseIDs.Length > 0)
        {
            SetupResponseButton(responseButton1, responseIDs[0]);
        }
        if (responseIDs.Length > 1)
        {
            SetupResponseButton(responseButton2, responseIDs[1]);
        }

        PositionResponseButtons();
    }

    void SetupResponseButton(Button button, int responseID)
    {
        Response response = dialogueDatabase.GetResponseById(responseID);
        if (response != null)
        {
            DialogueResponseButton responseButton = button.GetComponent<DialogueResponseButton>();
            if (responseButton != null)
            {
                responseButton.SetResponseText(response.text);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ResponseButtonClicked(response));
                ConfigureButtonHoverEffect(button);
                button.gameObject.SetActive(true);

                // Set the speaking character image to the talking state
                SetImageToTalkingState(response.speakingCharacterImage);
            }
            else
            {
                Debug.LogError("Button is missing a DialogueResponseButton component.");
            }
        }
        else
        {
            Debug.LogError("No response found for ID: " + responseID);
        }
    }

    private void PositionResponseButtons()
    {
        // Get the height of the dialogue box
        float dialogueBoxHeight = dialogueBoxRectTransform.rect.height;

        // Calculate the position for the response container
        Vector3 dialogueBoxPosition = dialogueBoxRectTransform.position;
        float responseContainerYPosition = dialogueBoxPosition.y + dialogueBoxHeight / 2 - 10; // Adjust the initial offset as needed

        // Set the position of the response container
        responseContainer.position = new Vector3(dialogueBoxPosition.x, responseContainerYPosition, dialogueBoxPosition.z);
    }

    private void ResponseButtonClicked(Response response)
    {
        responseButton1.onClick.RemoveAllListeners();
        responseButton2.onClick.RemoveAllListeners();

        dialogueText.gameObject.SetActive(true);
        responseButton1.gameObject.SetActive(false);
        responseButton2.gameObject.SetActive(false);

        TrackResponseSelection(response);

        if (response.nextDialogueID != -1)
        {
            StartDialogueById(response.nextDialogueID);
        }
        else if (currentDialogue.isEndDialogue)
        {
            EndScenario();
        }
    }

    private void TrackResponseSelection(Response response)
    {
        response.isSelected = true;

        if (!responseCategoryCounts.ContainsKey(response.category))
        {
            responseCategoryCounts[response.category] = 0;
        }

        responseCategoryCounts[response.category]++;
    }

    public string GetMostSelectedCategory()
    {
        string mostSelectedCategory = null;
        int highestCount = 0;

        foreach (var kvp in responseCategoryCounts)
        {
            if (kvp.Value > highestCount)
            {
                highestCount = kvp.Value;
                mostSelectedCategory = kvp.Key;
            }
        }

        return mostSelectedCategory;
    }

    // Section: Scenario Management
    private void EndScenario()
    {
        Debug.Log("End of Scenario reached.");

        foreach (var kvp in responseCategoryCounts)
        {
            Debug.Log($"Category {kvp.Key}: {kvp.Value} selections");
        }

        dialogueText.text = "Scenario completed! Please click Next to proceed.";
        nextButton.gameObject.SetActive(true);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnEndScenarioConfirmed);
    }

    private void OnEndScenarioConfirmed()
    {
        if (gameManager != null)
        {
            gameManager.HandleEndOfScenario();
        }
        else
        {
            Debug.LogError("GameManager reference is missing.");
        }
    }

    // Section: Image Management
    private void UpdateImageStates(Dialogue dialogue)
    {
        Debug.Log("Updating image states for dialogue ID: " + dialogue.id);

        UpdateImageComponent(leftImageComponent, leftImageAnimator, dialogue.isLeftImageVisible, dialogue.leftImage, dialogue.isLeftImageTalking);
        UpdateImageComponent(centerImageComponent, centerImageAnimator, dialogue.isCenterImageVisible, dialogue.centerImage, dialogue.isCenterImageTalking);
        UpdateImageComponent(rightImageComponent, rightImageAnimator, dialogue.isRightImageVisible, dialogue.rightImage, dialogue.isRightImageTalking);
    }

    private void UpdateImageComponent(Image imageComponent, Animator animator, bool isVisible, Sprite image, bool isTalking)
    {
        if (isVisible && image != null)
        {
            imageComponent.sprite = image;
            imageComponent.gameObject.SetActive(true);
            ResetAnimatorState(animator);
            animator.Play("Idle");
            animator.SetBool("isTalking", isTalking);
        }
        else
        {
            imageComponent.sprite = null;
            imageComponent.color = new Color(0, 0, 0, 0);
            imageComponent.gameObject.SetActive(false);
        }

        if (!isTalking && imageComponent.gameObject.activeSelf)
        {
            TriggerTalkingToIdleTransition(animator, 0.5f); // Smooth transition with a duration of 0.5 seconds
        }
    }

    private void TriggerTalkingToIdleTransition(Animator animator, float transitionDuration)
    {
        animator.CrossFade("Idle", transitionDuration);
    }

    private void ResetAnimatorState(Animator animator)
    {
        animator.SetBool("isTalking", false);
        animator.SetBool("isFadingIn", false);
        animator.SetBool("isFadingOut", false);
    }

    // Section: Fade Animations
    private void TriggerFadeOutAnimations(Dialogue dialogue)
    {
        Debug.Log("Triggering fade-out animations");

        TriggerFadeOut(leftImageAnimator, leftImageComponent, dialogue.shouldLeftImageFadeOut);
        TriggerFadeOut(centerImageAnimator, centerImageComponent, dialogue.shouldCenterImageFadeOut);
        TriggerFadeOut(rightImageAnimator, rightImageComponent, dialogue.shouldRightImageFadeOut);
    }

    private void TriggerFadeOut(Animator animator, Image imageComponent, bool shouldFadeOut)
    {
        if (shouldFadeOut && imageComponent.gameObject.activeSelf)
        {
            Debug.Log($"{imageComponent.name} fade-out");
            StartCoroutine(TransitionToIdleAndFadeOut(animator, imageComponent));
        }
    }

    private void TriggerFadeInAnimations(Dialogue dialogue)
    {
        Debug.Log("Triggering fade-in animations");

        if (dialogue.isLeftImageVisible && leftImageComponent.gameObject.activeSelf)
        {
            TriggerFadeIn(leftImageAnimator, leftImageComponent, dialogue.shouldLeftImageFadeIn, dialogue.isLeftImageVisible, dialogue.isLeftImageTalking, dialogue.isLeftImageMirrored);
        }
        if (dialogue.isCenterImageVisible && centerImageComponent.gameObject.activeSelf)
        {
            TriggerFadeIn(centerImageAnimator, centerImageComponent, dialogue.shouldCenterImageFadeIn, dialogue.isCenterImageVisible, dialogue.isCenterImageTalking, dialogue.isCenterImageMirrored);
        }
        if (dialogue.isRightImageVisible && rightImageComponent.gameObject.activeSelf)
        {
            TriggerFadeIn(rightImageAnimator, rightImageComponent, dialogue.shouldRightImageFadeIn, dialogue.isRightImageVisible, dialogue.isRightImageTalking, dialogue.isRightImageMirrored);
        }
    }

    private void TriggerFadeIn(Animator animator, Image imageComponent, bool shouldFadeIn, bool isVisible, bool isTalking, bool isMirrored)
    {
        if (shouldFadeIn && isVisible && imageComponent.gameObject.activeSelf)
        {
            Debug.Log($"{imageComponent.name} fade-in");
            animator.Play("FadeIn");
            StartCoroutine(HandleFadeInCompletion(animator, isTalking, isMirrored, imageComponent));
        }
    }

    private IEnumerator HandleFadeInCompletion(Animator animator, bool isTalking, bool isMirrored, Image imageComponent)
    {
        yield return new WaitForSeconds(1.0f); // Adjust this duration to match your FadeIn animation duration
        if (imageComponent.gameObject.activeSelf)
        {
            animator.Play("Idle");
            yield return new WaitForSeconds(0.1f);
            animator.SetBool("isTalking", isTalking);
            imageComponent.rectTransform.localScale = new Vector3(isMirrored ? -1 : 1, 1, 1);

            // Ensure the image alpha remains 1 (fully visible)
            float elapsedTime = 0f;
            float fadeDuration = 0.5f; // Adjust this duration as needed
            while (elapsedTime < fadeDuration)
            {
                imageComponent.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, elapsedTime / fadeDuration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            imageComponent.color = new Color(1, 1, 1, 1); // Ensure alpha is fully opaque

            if (!isTalking)
            {
                TriggerTalkingToIdleTransition(animator, 0.5f); // Smooth transition with a duration of 0.5 seconds
            }
        }
    }

    private IEnumerator TransitionToIdleAndFadeOut(Animator animator, Image imageComponent)
    {
        animator.Play("Idle");
        yield return new WaitForSeconds(0.1f);
        animator.Play("FadeOut");
        yield return new WaitForSeconds(1.0f);
        animator.SetBool("isFadingOut", false);

        // Reset the position to the initial position after fade-out
        ResetImagePosition(imageComponent);
    }

    private void SetImageToTalkingState(Image image)
    {
        if (image != null)
        {
            Animator animator = image.GetComponent<Animator>();
            if (animator != null)
            {
                ResetAnimatorState(animator);
                animator.SetBool("isTalking", true);
            }
            else
            {
                Debug.LogError("No Animator component found on the speaking character image.");
            }
        }
        else
        {
            Debug.LogError("Speaking character image is not assigned.");
        }
    }

    private IEnumerator FadeOutBackground()
    {
        Debug.Log("Fading out background");
        Color originalColor = backgroundImageComponent.color;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            backgroundImageComponent.color = Color.Lerp(originalColor, Color.black, normalizedTime);
            yield return null;
        }
        backgroundImageComponent.color = Color.black;
    }

    private IEnumerator FadeInBackground(Sprite newBackground)
    {
        Debug.Log("Fading in new background");
        backgroundImageComponent.sprite = newBackground;
        backgroundImageComponent.color = new Color(0, 0, 0, 0);

        Color targetColor = Color.white;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            backgroundImageComponent.color = Color.Lerp(new Color(0, 0, 0, 0), targetColor, normalizedTime);
            yield return null;
        }
        backgroundImageComponent.color = targetColor;
        Debug.Log("Background fade-in complete");
    }

    // Method to disable button temporarily
    private IEnumerator DisableButtonTemporarily()
    {
        if (nextButton != null) nextButton.interactable = false;
        yield return new WaitForSeconds(0.5f);
        if (nextButton != null) nextButton.interactable = true;
        buttonClicked = false;
    }

private void EnableNextButton()
{
    if (nextButton != null)
    {
        nextButton.gameObject.SetActive(true);
        nextButton.interactable = true;
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNextButtonClicked);
        buttonClicked = false;
    }
}


    // Method to configure the hover effect for a button
    void ConfigureButtonHoverEffect(Button button)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }
        else
        {
            trigger.triggers.Clear();
        }

        AddEventTrigger(trigger, EventTriggerType.PointerEnter, (eventData) => OnButtonHoverEnter(button));
        AddEventTrigger(trigger, EventTriggerType.PointerExit, (eventData) => OnButtonHoverExit(button));
    }

    void OnButtonHoverEnter(Button button)
    {
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.color = Color.yellow; // Change to your desired hover color
        }
    }

    void OnButtonHoverExit(Button button)
    {
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.color = Color.white; // Change to your desired normal color
        }
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

    // Section: Debug Tools
    public void SimulateResponses(int countA, int countB)
    {
        responseCategoryCounts["A"] = 0;
        responseCategoryCounts["B"] = 0;

        if (!responseCategoryCounts.ContainsKey("A"))
        {
            responseCategoryCounts["A"] = 0;
        }
        responseCategoryCounts["A"] += countA;

        if (!responseCategoryCounts.ContainsKey("B"))
        {
            responseCategoryCounts["B"] = 0;
        }
        responseCategoryCounts["B"] += countB;

        Debug.Log($"Simulated {countA} responses for category A and {countB} responses for category B");
    }

    public void TriggerEndScenarioDebug()
    {
        EndScenario();
    }

    // Method to reset image positions
    private void ResetImagePosition(Image imageComponent)
    {
        if (imageComponent == leftImageComponent)
        {
            leftImageComponent.rectTransform.localPosition = leftImageInitialPosition;
        }
        else if (imageComponent == centerImageComponent)
        {
            centerImageComponent.rectTransform.localPosition = centerImageInitialPosition;
        }
        else if (imageComponent == rightImageComponent)
        {
            rightImageComponent.rectTransform.localPosition = rightImageInitialPosition;
        }
    }
}
