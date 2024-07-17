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
    private string fullText;
    private Queue<string> tagsQueue = new Queue<string>();

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

    public Image leftOmbreImage;
    public Image centerLeftOmbreImage;
    public Image centerOmbreImage;
    public Image centerRightOmbreImage;
    public Image rightOmbreImage;

    // Game Manager
    public GameManagerForS3 gameManager;

    // Fade Properties
    public float fadeDuration = 1.0f; // Duration for fade effects

    // Internal State
    private DialogueForS3 currentDialogue; // Current dialogue being displayed
    private Coroutine typeSentenceCoroutine; // Coroutine for typing effect
    private bool isTransitioning = false; // Flag for transition state
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
    private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    private void Awake()
    {
        leftImageInitialPosition = leftImageComponent.rectTransform.localPosition;
        centerLeftImageInitialPosition = centerLeftImageComponent.rectTransform.localPosition;
        centerImageInitialPosition = centerImageComponent.rectTransform.localPosition;
        centerRightImageInitialPosition = centerRightImageComponent.rectTransform.localPosition;
        rightImageInitialPosition = rightImageComponent.rectTransform.localPosition;

        leftOmbreImage.gameObject.SetActive(false);
        centerLeftOmbreImage.gameObject.SetActive(false);
        centerOmbreImage.gameObject.SetActive(false);
        centerRightOmbreImage.gameObject.SetActive(false);
        rightOmbreImage.gameObject.SetActive(false);
    }

    // Initialization on start
    async void Start()
    {
        // Store initial positions of image components
        leftImageInitialPosition = leftImageComponent.rectTransform.localPosition;
        centerLeftImageInitialPosition = centerLeftImageComponent.rectTransform.localPosition;
        centerImageInitialPosition = centerImageComponent.rectTransform.localPosition;
        centerRightImageInitialPosition = centerRightImageComponent.rectTransform.localPosition;
        rightImageInitialPosition = rightImageComponent.rectTransform.localPosition;

        // Disable image components
        leftImageComponent.gameObject.SetActive(false);
        centerLeftImageComponent.gameObject.SetActive(false);
        centerImageComponent.gameObject.SetActive(false);
        centerRightImageComponent.gameObject.SetActive(false);
        rightImageComponent.gameObject.SetActive(false);
        responseContainer.gameObject.SetActive(false);
        tutorialPanel.SetActive(false);

        // Set up button listeners
        nextButton.onClick.RemoveAllListeners();
        skipButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(ProceedToNextDialogue);
        skipButton.onClick.AddListener(OnSkipButtonClicked);

        if (SceneManager.GetActiveScene().name == "Scenario1")
        {
            ShowTutorialPanel();
            StartCoroutine(CloseTutorialPanelAndStartDialogue());
        }
        else
        {
            StartDialogueById(7569);
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private IEnumerator CloseTutorialPanelAndStartDialogue()
    {
        yield return new WaitUntil(() => isTutorialClosed);
        leftImageComponent.gameObject.SetActive(true);
        centerLeftImageComponent.gameObject.SetActive(true);
        centerImageComponent.gameObject.SetActive(true);
        centerRightImageComponent.gameObject.SetActive(true);
        rightImageComponent.gameObject.SetActive(true);
        responseContainer.gameObject.SetActive(true);
        StartDialogueById(7569);
    }

    public void StartDialogueById(int dialogueId)
    {
        DialogueForS3 dialogueToStart = dialogueDatabase.GetDialogueById(dialogueId);
        if (dialogueToStart != null)
        {
            DisplayDialogue(dialogueToStart);
        }
        else
        {
            Debug.LogError("No dialogue found with ID: " + dialogueId);
        }
    }

    public void CloseTutorialPanel()
    {
        isTutorialClosed = true;
        tutorialPanel.SetActive(false);
    }

    public void ShowTutorialPanel()
    {
        isTutorialClosed = false;
        tutorialPanel.SetActive(true);
        SceneSettingsManager sceneSettingsManager = FindObjectOfType<SceneSettingsManager>();
        sceneSettingsManager.CloseSettingsMenu();
        closeTutorialButton.onClick.RemoveAllListeners();
        closeTutorialButton.onClick.AddListener(CloseTutorialPanel);
    }

    private void ProceedToNextDialogue()
    {
        if (isTyping) return;
        TransitionAllToIdle();
        StartCoroutine(DisableButtonTemporarily());

        if (currentDialogue.hasResponses)
        {
            GenerateResponseButtons(currentDialogue.responseIDs);
        }
        else if (currentDialogue.nextDialogueID != -1)
        {
            StartDialogueById(currentDialogue.nextDialogueID);
        }
        else if (currentDialogue.isEndDialogue)
        {
            EndScenario();
        }
        else
        {
            Debug.Log("No more dialogues");
        }
    }

    public void OnSkipButtonClicked()
    {
        if (isTyping)
        {
            if (typeSentenceCoroutine != null)
            {
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
            TriggerFadeOutAnimations(dialogue);
            if (dialogue.fadeInBackground && dialogue.backgroundImage != null)
            {
                StartCoroutine(FadeInBackground(dialogue.backgroundImage));
            }
        }

        // Set the speaking character image to the talking state
        SetImageToTalkingState(leftImageComponent, dialogue.isLeftImageTalking, leftOmbreImage);
        SetImageToTalkingState(centerLeftImageComponent, dialogue.isCenterLeftImageTalking, centerLeftOmbreImage);
        SetImageToTalkingState(centerImageComponent, dialogue.isCenterImageTalking, centerOmbreImage);
        SetImageToTalkingState(centerRightImageComponent, dialogue.isCenterRightImageTalking, centerRightOmbreImage);
        SetImageToTalkingState(rightImageComponent, dialogue.isRightImageTalking, rightOmbreImage);

        responseButton1.gameObject.SetActive(false);
        responseButton2.gameObject.SetActive(false);

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f; // Reset scroll position to the top
    }

    // Coroutine to type out the sentence letter by letter
    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;
        fullText = sentence;

        // Process the text to extract tags
        string processedText = "";
        int index = 0;
        while (index < fullText.Length)
        {
            if (fullText[index] == '<')
            {
                int endIndex = fullText.IndexOf('>', index);
                if (endIndex != -1)
                {
                    string tag = fullText.Substring(index, endIndex - index + 1);
                    tagsQueue.Enqueue(tag);
                    processedText += tag;
                    index = endIndex + 1;
                    continue;
                }
            }
            processedText += fullText[index];
            index++;
        }

        // Display the text character by character, handling tags
        index = 0;
        while (index < processedText.Length)
        {
            if (processedText[index] == '<')
            {
                int endIndex = processedText.IndexOf('>', index);
                if (endIndex != -1)
                {
                    string tag = processedText.Substring(index, endIndex - index + 1);
                    dialogueText.text += tag;
                    index = endIndex + 1;
                    continue;
                }
            }
            dialogueText.text += processedText[index];
            index++;
            yield return new WaitForSeconds(0.01f);
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        isTyping = false;
        EnableNextButton();
    }

    void GenerateResponseButtons(int[] responseIDs)
    {
        SetupResponseButton(responseButton1, responseIDs[0]);
        SetupResponseButton(responseButton2, responseIDs[1]);
    }

    // Method to set up response button
    void SetupResponseButton(Button button, int responseID)
    {
        responseContainer.gameObject.SetActive(true);
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
                bool isTalking = true;
                if (response.speakingCharacterImage == leftImageComponent)
                {
                    SetImageToTalkingState(response.speakingCharacterImage, isTalking, leftOmbreImage);
                }
                else if (response.speakingCharacterImage == centerLeftImageComponent)
                {
                    SetImageToTalkingState(response.speakingCharacterImage, isTalking, centerLeftOmbreImage);
                }
                else if (response.speakingCharacterImage == centerImageComponent)
                {
                    SetImageToTalkingState(response.speakingCharacterImage, isTalking, centerOmbreImage);
                }
                else if (response.speakingCharacterImage == centerRightImageComponent)
                {
                    SetImageToTalkingState(response.speakingCharacterImage, isTalking, centerRightOmbreImage);
                }
                else if (response.speakingCharacterImage == rightImageComponent)
                {
                    SetImageToTalkingState(response.speakingCharacterImage, isTalking, rightOmbreImage);
                }
            }
            else
            {
                Debug.LogError("Button is missing a DialogueResponseButton component.");
            }
        }
    }

    // Method called when a response button is clicked
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
        else
        {
            Debug.Log("No more dialogues");
        }
    }

    // Method to track response selection
    private void TrackResponseSelection(Response response)
    {
        response.isSelected = true;

        if (!responseCategoryCounts.ContainsKey(response.category))
        {
            responseCategoryCounts[response.category] = 0;
        }

        responseCategoryCounts[response.category]++;
    }

    // Method to get the most selected category
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

    // Method to end the scenario
    private void EndScenario()
    {
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
            animator.Play("Idle");
            if (isTalking)
            {
                animator.Play("Talking");
            }
        }
        else
        {
            imageComponent.sprite = null;
            imageComponent.color = new Color(0, 0, 0, 0);
            imageComponent.gameObject.SetActive(false);
        }
    }

    // Method to trigger fade in animations
    public void TriggerFadeInAnimations(DialogueForS3 dialogue)
    {
        TriggerFadeIn(leftImageAnimator, leftImageComponent, dialogue.shouldLeftImageFadeIn, dialogue.isLeftImageVisible, dialogue.isLeftImageTalking, dialogue.isLeftImageMirrored);
        TriggerFadeIn(centerLeftImageAnimator, centerLeftImageComponent, dialogue.shouldCenterLeftImageFadeIn, dialogue.isCenterLeftImageVisible, dialogue.isCenterLeftImageTalking, dialogue.isCenterLeftImageMirrored);
        TriggerFadeIn(centerImageAnimator, centerImageComponent, dialogue.shouldCenterImageFadeIn, dialogue.isCenterImageVisible, dialogue.isCenterImageTalking, dialogue.isCenterImageMirrored);
        TriggerFadeIn(centerRightImageAnimator, centerRightImageComponent, dialogue.shouldCenterRightImageFadeIn, dialogue.isCenterRightImageVisible, dialogue.isCenterRightImageTalking, dialogue.isCenterRightImageMirrored);
        TriggerFadeIn(rightImageAnimator, rightImageComponent, dialogue.shouldRightImageFadeIn, dialogue.isRightImageVisible, dialogue.isRightImageTalking, dialogue.isRightImageMirrored);
    }

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
        yield return new WaitForSeconds(0.15f);
        if (imageComponent.gameObject.activeSelf)
        {
            animator.Play("Idle");
            imageComponent.rectTransform.localScale = new Vector3(isMirrored ? -1 : 1, 1, 1);

            float elapsedTime = 0f;
            float fadeDuration = 0.5f;
            while (elapsedTime < fadeDuration)
            {
                imageComponent.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, elapsedTime / fadeDuration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            imageComponent.color = new Color(1, 1, 1, 1);

            if (isTalking)
            {
                animator.Play("Talking");
            }
        }
    }

    public void TriggerFadeOutAnimations(DialogueForS3 dialogue)
    {
        TriggerFadeOut(leftImageAnimator, leftImageComponent, dialogue.shouldLeftImageFadeOut, dialogue.isLeftImageVisible);
        TriggerFadeOut(centerLeftImageAnimator, centerLeftImageComponent, dialogue.shouldCenterLeftImageFadeOut, dialogue.isCenterLeftImageVisible);
        TriggerFadeOut(centerImageAnimator, centerImageComponent, dialogue.shouldCenterImageFadeOut, dialogue.isCenterImageVisible);
        TriggerFadeOut(centerRightImageAnimator, centerRightImageComponent, dialogue.shouldCenterRightImageFadeOut, dialogue.isCenterRightImageVisible);
        TriggerFadeOut(rightImageAnimator, rightImageComponent, dialogue.shouldRightImageFadeOut, dialogue.isRightImageVisible);
    }

    private void TriggerFadeOut(Animator animator, Image imageComponent, bool shouldFadeOut, bool isVisible)
    {
        if (shouldFadeOut && isVisible && imageComponent.gameObject.activeSelf)
        {
            animator.Play("FadeOut");
            StartCoroutine(HandleFadeOutCompletion(animator, imageComponent));
        }
    }

    private IEnumerator HandleFadeOutCompletion(Animator animator, Image imageComponent)
    {
        yield return new WaitForSeconds(0.15f);
        if (imageComponent.gameObject.activeSelf)
        {
            animator.Play("Idle");

            float elapsedTime = 0f;
            float fadeDuration = 0.5f;
            while (elapsedTime < fadeDuration)
            {
                imageComponent.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, elapsedTime / fadeDuration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            imageComponent.color = new Color(1, 1, 1, 0);
            imageComponent.gameObject.SetActive(false);
        }
    }

    // Method to set an image to talking state
    private void SetImageToTalkingState(Image image, bool isTalking, Image ombreImage)
    {
        if (image != null)
        {
            Animator animator = image.GetComponent<Animator>();
            if (animator != null)
            {
                if (isTalking)
                {
                    animator.SetBool("isTalking", true);
                    animator.CrossFade("Talking", 0.25f);
                    StartCoroutine(FadeInOmbre(ombreImage));
                }
                else
                {
                    animator.SetBool("isTalking", false);
                    animator.CrossFade("Idle", 0.25f);
                    ombreImage.gameObject.SetActive(false); // Hide ombre when not talking
                }
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

    private IEnumerator FadeInOmbre(Image ombreImage)
    {
        if (ombreImage != null)
        {
            ombreImage.gameObject.SetActive(true);
            Color color = ombreImage.color;
            color.a = 0;
            ombreImage.color = color;

            float duration = 0.25f; // Duration of the fade-in effect
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Clamp01(elapsed / duration);
                ombreImage.color = color;
                yield return null;
            }

            color.a = 1f;
            ombreImage.color = color;
        }
    }

    // Coroutine to fade in background
    private IEnumerator FadeInBackground(Sprite newBackground)
    {
        // Fade out current background to black
        Color initialColor = backgroundImageComponent.color;
        Color blackColor = Color.black;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            backgroundImageComponent.color = Color.Lerp(initialColor, blackColor, normalizedTime);
            yield return null;
        }
        backgroundImageComponent.color = blackColor;

        // Set the new background
        backgroundImageComponent.sprite = newBackground;

        // Fade in new background from black
        Color targetColor = Color.white;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            backgroundImageComponent.color = Color.Lerp(blackColor, targetColor, normalizedTime);
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
    public void TransitionAllToIdle()
    {
        TransitionToIdle(leftImageAnimator, leftImageComponent);
        TransitionToIdle(centerLeftImageAnimator, centerLeftImageComponent);
        TransitionToIdle(centerImageAnimator, centerImageComponent);
        TransitionToIdle(centerRightImageAnimator, centerRightImageComponent);
        TransitionToIdle(rightImageAnimator, rightImageComponent);
    }

    private void TransitionToIdle(Animator animator, Image imageComponent)
    {
        if (imageComponent.gameObject.activeSelf)
        {
            animator.Play("Idle");
        }
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
