using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneSettingsManager : MonoBehaviour
{
    public static SceneSettingsManager Instance { get; private set; }

    public GameObject settingsMenu;
    public GameObject settingsManager;
    public Button settingsButton;
    public Slider volumeSlider;
    public AudioSource backgroundMusic; // Added from SettingsManager
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.5f;
    private FadeManager fadeManager;

    private void Awake()
    {
        Debug.Log("SceneSettingsManager Awake called.");

        if (Instance != null && Instance != this)
        {
            Debug.Log("Another instance of SceneSettingsManager exists, destroying this one.");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Setting this instance as the singleton instance.");
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (settingsMenu == null)
        {
            Debug.LogError("settingsMenu is not assigned in the Inspector.");
        }
        else
        {
            settingsMenu.SetActive(false);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            volumeSlider.value = AudioListener.volume; // Initialize slider value
        }
        else
        {
            Debug.LogError("volumeSlider is not assigned in the Inspector.");
        }
    }

    private void Start()
    {
        Debug.Log("SceneSettingsManager Start called.");

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettingsMenu);
        }
        else
        {
            Debug.LogError("settingsButton is not assigned in the Inspector.");
        }

        if (settingsMenu != null && settingsMenu.activeSelf)
        {
            Debug.LogWarning("Settings menu was active on scene load. Deactivating.");
            settingsMenu.SetActive(false);
        }

        fadeManager = FindObjectOfType<FadeManager>(); // Get the FadeManager in the current scene
        if (fadeManager == null)
        {
            Debug.LogError("FadeManager not found.");
        }
    }

    public void OpenSettingsMenu()
    {
        Debug.Log("OpenSettingsMenu called.");
        StartCoroutine(FadeIn());
    }

    public void CloseSettingsMenu()
    {
        Debug.Log("CloseSettingsMenu called.");
        StartCoroutine(FadeOut());
    }

    public void OnVolumeChanged(float value)
    {
        Debug.Log("OnVolumeChanged called with value: " + value);
        AudioListener.volume = value;
    }

    public void ResumeGame()
    {
        Debug.Log("ResumeGame called.");
        CloseSettingsMenu();
    }

    public void ExitToMenu()
    {
        Debug.Log("ExitToMenu called.");
        if (fadeManager != null)
        {
            fadeManager.FadeToScene("MainMenu"); // Assuming your main menu scene is named "MainMenu"
        }
        else
        {
            Debug.LogError("Cannot exit to menu. FadeManager is null.");
        }
    }

    private IEnumerator FadeIn()
    {
        Debug.Log("FadeIn started.");
        settingsMenu.SetActive(true);
        fadeCanvasGroup.blocksRaycasts = true;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration * 0.5f); // Slightly darken the screen
            Debug.Log("Fading in... alpha: " + fadeCanvasGroup.alpha);
            yield return null;
        }
        Debug.Log("FadeIn completed.");
    }

    private IEnumerator FadeOut()
    {
        Debug.Log("FadeOut started.");
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsedTime / fadeDuration * 0.5f);
            Debug.Log("Fading out... alpha: " + fadeCanvasGroup.alpha);
            yield return null;
        }

        fadeCanvasGroup.blocksRaycasts = false;
        settingsMenu.SetActive(false);
        Debug.Log("FadeOut completed. Settings menu deactivated.");
    }
}
