using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueManagerForS3 : MonoBehaviour
{
    public static DialogueManagerForS3 Instance { get; private set; }

    // Flags for various states
    private bool isTyping = false;

    // UI Components
    public TMP_Text dialogueText; // Main dialogue text
    public Button nextButton; // Button to proceed to the next dialogue
    public Image backgroundImageComponent; // Background image component
    public RectTransform dialogueBoxRectTransform; // RectTransform for dialogue box
    public Button responseButton1; // Response button 1
    public Button responseButton2; // Response button 2
    public RectTransform responseContainer; // Container for response buttons
    public Button skipButton; // Button to skip typing effect
    public ScrollRect scrollRect; // Reference to the ScrollRect component
    public GameObject tutorialPanel; // Tutorial Panel
    public Button closeTutorialButton; // Close Tutorial Button

    // Dialogue Database
    public DialogueDatabaseForS3 dialogueDatabase; // Database containing all dialogues

    // Image Animators and Components
    public Animator leftImageAnimator;
    public Animator centerLeftImageAnimator;
    public Animator centerImageAnimator;
    public Animator centerRightImageAnimator;
    public Animator rightImageAnimator;
    public Image leftImageComponent;
    public Image centerLeftImageComponent;
    public Image centerImageComponent;
    public Image centerRightImageComponent;
    public Image rightImageComponent;

    // Game Manager
    public GameManagerForS3 gameManager;

    // Fade Properties
    public float fadeDuration = 1.0f; // Duration for fade effects
    private bool fadeOutBackground = false; // Flag for fading out background
    private bool fadeInBackground = false; // Flag for fading in background

    // Internal State
    private DialogueForS3 currentDialogue; // Current dialogue being displayed
    private Coroutine typeSentenceCoroutine; // Coroutine for typing effect
    private bool isTransitioning = false; // Flag for transition state
    private bool isInitialized = false; // Flag for initialization state
    private bool isSkippingText = false; // Flag for skipping text
    private bool isTutorialClosed = false;

    // Response Tracking
    private Dictionary<string, int> responseCategoryCounts = new Dictionary<string, int>();

    // Initial Positions of Image Components
    private Vector3 leftImageInitialPosition;
    private Vector3 centerLeftImageInitialPosition;
    private Vector3 centerImageInitialPosition;
    private Vector3 centerRightImageInitialPosition;
    private Vector3 rightImageInitialPosition;

    // Method to add event triggers to buttons
    private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action) {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    // Initialization on start
    void Start() {
        // Store initial positions of image components
        leftImageInitialPosition = leftImageComponent.rectTransform.localPosition;
        centerLeftImageInitialPosition = centerLeftImageComponent.rectTransform.localPosition;
        centerImageInitialPosition = centerImageComponent.rectTransform.localPosition;
        centerRightImageInitialPosition = centerRightImageComponent.rectTransform.localPosition;
        rightImageInitialPosition = rightImageComponent.rectTransform.localPosition;

        // Enable image components
        leftImageComponent.gameObject.SetActive(true);
        centerLeftImageComponent.gameObject.SetActive(true);
        centerImageComponent.gameObject.SetActive(true);
        centerRightImageComponent.gameObject.SetActive(true);
        rightImageComponent.gameObject.SetActive(true);
        tutorialPanel.SetActive(false);


        // Set up button listeners
        nextButton.onClick.RemoveAllListeners();
        skipButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(ProceedToNextDialogue);
        skipButton.onClick.AddListener(OnSkipButtonClicked);

        StartDialogueById(7569); // Start the dialogue sequence

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    //Tutorial Panel Functions
    public void CloseTutorialPanel(){
        isTutorialClosed = true;
        tutorialPanel.SetActive(false);
    }

    public void ShowTutorialPanel() {
        isTutorialClosed = false;
        tutorialPanel.SetActive(true);
        SceneSettingsManager sceneSettingsManager = FindObjectOfType<SceneSettingsManager>();
        sceneSettingsManager.CloseSettingsMenu();
        closeTutorialButton.onClick.RemoveAllListeners();
        closeTutorialButton.onClick.AddListener(CloseTutorialPanel);
    }

    public void StartDialogueById(int dialogueId) {
        DialogueForS3 dialogueToStart = dialogueDatabase.GetDialogueById(dialogueId);
        if (dialogueToStart != null) {
            DisplayDialogue(dialogueToStart);
        } else {
            Debug.LogError("No dialogue found with ID: " + dialogueId);
        }
    }

    private void ProceedToNextDialogue() {
        if (isTyping) return;
        TransitionAllToIdle();
        StartCoroutine(DisableButtonTemporarily());

        if (currentDialogue.hasResponses) {
            GenerateResponseButtons(currentDialogue.responseIDs);
        } else if (currentDialogue.nextDialogueID != -1) {
            StartDialogueById(currentDialogue.nextDialogueID);
        } else if(currentDialogue.isEndDialogue){
            EndScenario(); 
        } else{
            Debug.Log("No more dialogues");
        }

    }

    public void OnSkipButtonClicked() {
        if (isTyping) {
            if (typeSentenceCoroutine != null) {
                StopCoroutine(typeSentenceCoroutine);
                dialogueText.text = currentDialogue.text;
                typeSentenceCoroutine = null;
                isTyping = false;

                EnableNextButton();
            }
        }
    }

    // Method to display a dialogue
    public void DisplayDialogue(DialogueForS3 dialogue)
    {
        currentDialogue = dialogue;

        // If there is an ongoing coroutine for typing the sentence, stop it
        if (typeSentenceCoroutine != null)
        {
            StopCoroutine(typeSentenceCoroutine);
        }

        typeSentenceCoroutine = StartCoroutine(TypeSentence(dialogue.text));

        // If the dialogue manager is not in a transition state, update the images and trigger fade-in animations
        if (!isTransitioning)
        {
            UpdateImageStates(dialogue);
            TriggerFadeInAnimations(dialogue);
            if (fadeInBackground && dialogue.backgroundImage != null)
            {
                StartCoroutine(FadeInBackground(dialogue.backgroundImage));
            }
        }
        responseButton1.gameObject.SetActive(false);
        responseButton2.gameObject.SetActive(false);
    }


    // Coroutine to type out the sentence letter by letter
    IEnumerator TypeSentence(string sentence) {
        dialogueText.text = "";
        isTyping = true;

        foreach (char letter in sentence.ToCharArray()) {
            dialogueText.text += letter;
            AdjustContentHeight();
            yield return new WaitForSeconds(0.01f);
        }

        isTyping = false;

        if (currentDialogue.nextDialogueID != -1) {
            EnableNextButton();
        }
    }

    // Adjust the height of the Content to fit the TextMeshProUGUI component
    void AdjustContentHeight()
    {
        Canvas.ForceUpdateCanvases();
        RectTransform textRectTransform = dialogueText.GetComponent<RectTransform>();
        RectTransform contentRectTransform = dialogueText.transform.parent.GetComponent<RectTransform>();

        // Update the size of the content based on the preferred height of the text
        float preferredHeight = dialogueText.preferredHeight;
        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, preferredHeight);

        // Recalculate the layout of the ScrollRect
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);

        // Adjust the vertical position to keep the text in view
        scrollRect.verticalNormalizedPosition = 1f;
    }

    void SetScrollbarToTop()
    {
        // Force update to ensure layout is correct before setting scroll position
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;

        // Manually set content position
        RectTransform contentRectTransform = scrollRect.content;
        contentRectTransform.anchoredPosition = new Vector2(contentRectTransform.anchoredPosition.x, 0);
    }

    void GenerateResponseButtons(int[] responseIDs) {
        SetupResponseButton(responseButton1, responseIDs[0]);
        SetupResponseButton(responseButton2, responseIDs[1]);
        PositionResponseButtons();
    }

    // Method to set up response button
    void SetupResponseButton(Button button, int responseID)
    {
        Response response = dialogueDatabase.GetResponseById(responseID);
        if (response != null) {
            DialogueResponseButton responseButton = button.GetComponent<DialogueResponseButton>();
            if (responseButton != null) {
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
    }

    // Method to position response buttons
    private void PositionResponseButtons() {
        float dialogueBoxHeight = dialogueBoxRectTransform.rect.height;
        float responseContainerHeight = responseContainer.rect.height;

        Vector3 dialogueBoxPosition = dialogueBoxRectTransform.position;
        float responseContainerYPosition = dialogueBoxPosition.y + (dialogueBoxHeight / 2) + responseContainerHeight + 50;

        responseContainer.position = new Vector3(dialogueBoxPosition.x, responseContainerYPosition, dialogueBoxPosition.z);
    }

    // Method called when a response button is clicked
    private void ResponseButtonClicked(Response response) {
        responseButton1.onClick.RemoveAllListeners();
        responseButton2.onClick.RemoveAllListeners();

        dialogueText.gameObject.SetActive(true);
        responseButton1.gameObject.SetActive(false);
        responseButton2.gameObject.SetActive(false);

        TrackResponseSelection(response);

        if (response.nextDialogueID != -1)
        {
            StartDialogueById(response.nextDialogueID);
        } else {
            Debug.LogError("Invalid response dialogue ID");
        }
    }

    // Method to track response selection
    private void TrackResponseSelection(Response response) {
        response.isSelected = true;

        if (!responseCategoryCounts.ContainsKey(response.category)) {
            responseCategoryCounts[response.category] = 0;
        }

        responseCategoryCounts[response.category]++;
    }

    // Method to get the most selected category
    public string GetMostSelectedCategory() {
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

    // Method to end the scenario
    private void EndScenario() {
        foreach (var kvp in responseCategoryCounts)
        {
            Debug.Log($"Category {kvp.Key}: {kvp.Value} selections");
        }

        dialogueText.text = "Scenario completed! Please click Next to proceed.";
        nextButton.gameObject.SetActive(true);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnEndScenarioConfirmed);
    }

    // Method called when end scenario is confirmed
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

    // Method to update image states
    private void UpdateImageStates(DialogueForS3 dialogue)
    {
        UpdateImageComponent(leftImageComponent, leftImageAnimator, dialogue.isLeftImageVisible, dialogue.leftImage, dialogue.isLeftImageTalking);
        UpdateImageComponent(centerLeftImageComponent, centerLeftImageAnimator, dialogue.isCenterLeftImageVisible, dialogue.centerLeftImage, dialogue.isCenterLeftImageTalking);
        UpdateImageComponent(centerImageComponent, centerImageAnimator, dialogue.isCenterImageVisible, dialogue.centerImage, dialogue.isCenterImageTalking);
        UpdateImageComponent(centerRightImageComponent, centerRightImageAnimator, dialogue.isCenterRightImageVisible, dialogue.centerRightImage, dialogue.isCenterRightImageTalking);
        UpdateImageComponent(rightImageComponent, rightImageAnimator, dialogue.isRightImageVisible, dialogue.rightImage, dialogue.isRightImageTalking);
    }

    // Method to update an individual image component
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

    // Method to trigger transition from talking to idle
    private void TriggerTalkingToIdleTransition(Animator animator, float transitionDuration)
    {
        animator.CrossFade("Idle", transitionDuration);
    }

    // Method to reset the animator state
    private void ResetAnimatorState(Animator animator)
    {
        animator.SetBool("isTalking", false);
        animator.SetBool("isFadingIn", false);
        animator.SetBool("isFadingOut", false);
    }

    // Method to trigger fade out animations
    private void TriggerFadeOutAnimations(DialogueForS3 dialogue)
    {
        TriggerFadeOut(leftImageAnimator, leftImageComponent, dialogue.shouldLeftImageFadeOut);
        TriggerFadeOut(centerLeftImageAnimator, centerLeftImageComponent, dialogue.shouldCenterLeftImageFadeOut);
        TriggerFadeOut(centerImageAnimator, centerImageComponent, dialogue.shouldCenterImageFadeOut);
        TriggerFadeOut(centerRightImageAnimator, centerRightImageComponent, dialogue.shouldCenterRightImageFadeOut);
        TriggerFadeOut(rightImageAnimator, rightImageComponent, dialogue.shouldRightImageFadeOut);
    }

    // Method to trigger fade out of an individual image component
    private void TriggerFadeOut(Animator animator, Image imageComponent, bool shouldFadeOut)
    {
        if (shouldFadeOut && imageComponent.gameObject.activeSelf)
        {
            StartCoroutine(TransitionToIdleAndFadeOut(animator, imageComponent));
        }
    }

    // Method to trigger fade in animations
    private void TriggerFadeInAnimations(DialogueForS3 dialogue)
    {
        if (dialogue.isLeftImageVisible && leftImageComponent.gameObject.activeSelf)
        {
            TriggerFadeIn(leftImageAnimator, leftImageComponent, dialogue.shouldLeftImageFadeIn, dialogue.isLeftImageVisible, dialogue.isLeftImageTalking, dialogue.isLeftImageMirrored);
        }
        if (dialogue.isCenterLeftImageVisible && centerLeftImageComponent.gameObject.activeSelf)
        {
            TriggerFadeIn(centerLeftImageAnimator, centerLeftImageComponent, dialogue.shouldCenterLeftImageFadeIn, dialogue.isCenterLeftImageVisible, dialogue.isCenterLeftImageTalking, dialogue.isCenterLeftImageMirrored);
        }
        if (dialogue.isCenterImageVisible && centerImageComponent.gameObject.activeSelf)
        {
            TriggerFadeIn(centerImageAnimator, centerImageComponent, dialogue.shouldCenterImageFadeIn, dialogue.isCenterImageVisible, dialogue.isCenterImageTalking, dialogue.isCenterImageMirrored);
        }
        if (dialogue.isCenterRightImageVisible && centerRightImageComponent.gameObject.activeSelf)
        {
            TriggerFadeIn(centerRightImageAnimator, centerRightImageComponent, dialogue.shouldCenterRightImageFadeIn, dialogue.isCenterRightImageVisible, dialogue.isCenterRightImageTalking, dialogue.isCenterRightImageMirrored);
        }
        if (dialogue.isRightImageVisible && rightImageComponent.gameObject.activeSelf)
        {
            TriggerFadeIn(rightImageAnimator, rightImageComponent, dialogue.shouldRightImageFadeIn, dialogue.isRightImageVisible, dialogue.isRightImageTalking, dialogue.isRightImageMirrored);
        }
    }

    // Method to trigger fade in of an individual image component
    private void TriggerFadeIn(Animator animator, Image imageComponent, bool shouldFadeIn, bool isVisible, bool isTalking, bool isMirrored)
    {
        if (shouldFadeIn && isVisible && imageComponent.gameObject.activeSelf)
        {
            animator.Play("FadeIn");
            StartCoroutine(HandleFadeInCompletion(animator, isTalking, isMirrored, imageComponent));
        }
    }

    // Coroutine to handle fade in completion
    private IEnumerator HandleFadeInCompletion(Animator animator, bool isTalking, bool isMirrored, Image imageComponent)
    {
        yield return new WaitForSeconds(1.0f); // Adjust this duration to match your FadeIn animation duration
        if (imageComponent.gameObject.activeSelf)
        {
            animator.Play("Idle");
            yield return new WaitForSeconds(0.1f);
            animator.SetBool("isTalking", isTalking);
            imageComponent.rectTransform.localScale = new Vector3(isMirrored ? -1 : 1, 1, 1);

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
                TriggerTalkingToIdleTransition(animator, 0.5f);
            }
        }
    }

    // Coroutine to handle transition to idle and fade out
    private IEnumerator TransitionToIdleAndFadeOut(Animator animator, Image imageComponent)
    {
        animator.Play("Idle");
        yield return new WaitForSeconds(0.1f);
        animator.Play("FadeOut");
        yield return new WaitForSeconds(1.0f);
        animator.SetBool("isFadingOut", false);

        ResetImagePosition(imageComponent);
    }

    // Method to set an image to talking state
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

    // Coroutine to fade out background
    private IEnumerator FadeOutBackground()
    {
        Color originalColor = backgroundImageComponent.color;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            backgroundImageComponent.color = Color.Lerp(originalColor, Color.black, normalizedTime);
            yield return null;
        }
        backgroundImageComponent.color = Color.black;
    }

    // Coroutine to fade in background
    private IEnumerator FadeInBackground(Sprite newBackground)
    {
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
    }

    // Coroutine to temporarily disable the next button
    private IEnumerator DisableButtonTemporarily()
    {
        if (nextButton != null) nextButton.interactable = false;
        yield return new WaitForSeconds(0.5f);
        if (nextButton != null) nextButton.interactable = true;
    }

    // Method to enable the next button
    private void EnableNextButton()
    {
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
            nextButton.interactable = true;
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(ProceedToNextDialogue);
        }
    }

    // Method to configure hover effect for a button
    void ConfigureButtonHoverEffect(Button button) {
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

    // Method called when button hover enter event is triggered
    void OnButtonHoverEnter(Button button)
    {
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.color = Color.yellow; // Change to your desired hover color
        }
    }

    // Method called when button hover exit event is triggered
    void OnButtonHoverExit(Button button)
    {
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.color = Color.white; // Change to your desired normal color
        }
    }

    // Method to transition all images to idle state
    private void TransitionAllToIdle()
    {
        if (leftImageComponent.gameObject.activeSelf)
        {
            leftImageAnimator.SetBool("isTalking", false);
            leftImageAnimator.Play("Idle");
        }
        if (centerLeftImageComponent.gameObject.activeSelf)
        {
            centerLeftImageAnimator.SetBool("isTalking", false);
            centerLeftImageAnimator.Play("Idle");
        }
        if (centerImageComponent.gameObject.activeSelf)
        {
            centerImageAnimator.SetBool("isTalking", false);
            centerImageAnimator.Play("Idle");
        }
        if (centerRightImageComponent.gameObject.activeSelf)
        {
            centerRightImageAnimator.SetBool("isTalking", false);
            centerRightImageAnimator.Play("Idle");
        }
        if (rightImageComponent.gameObject.activeSelf)
        {
            rightImageAnimator.SetBool("isTalking", false);
            rightImageAnimator.Play("Idle");
        }
    }

    // Debug tools

    // Method to simulate responses for testing
    public void SimulateResponses(int countA, int countB)
    {
        responseCategoryCounts["A"] = 0;
        responseCategoryCounts["B"] = 0;

        responseCategoryCounts["A"] += countA;
        responseCategoryCounts["B"] += countB;

        Debug.Log($"Simulated {countA} responses for category A and {countB} responses for category B");
    }

    // Method to trigger end of scenario for debugging
    public void TriggerEndScenarioDebug()
    {
        EndScenario();
    }

    // Method to reset image positions to initial positions
    private void ResetImagePosition(Image imageComponent)
    {
        if (imageComponent == leftImageComponent)
        {
            leftImageComponent.rectTransform.localPosition = leftImageInitialPosition;
        }
        else if (imageComponent == centerLeftImageComponent)
        {
            centerLeftImageComponent.rectTransform.localPosition = centerLeftImageInitialPosition;
        }
        else if (imageComponent == centerImageComponent)
        {
            centerImageComponent.rectTransform.localPosition = centerImageInitialPosition;
        }
        else if (imageComponent == centerRightImageComponent)
        {
            centerRightImageComponent.rectTransform.localPosition = centerRightImageInitialPosition;
        }
        else if (imageComponent == rightImageComponent)
        {
            rightImageComponent.rectTransform.localPosition = rightImageInitialPosition;
        }
    }
}

