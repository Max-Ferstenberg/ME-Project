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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        settingsMenu.SetActive(false);
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            volumeSlider.value = AudioListener.volume; // Initialize slider value
        }
    }

    private void Start()
    {
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettingsMenu);
        }

        if (settingsMenu.activeSelf)
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
        StartCoroutine(FadeIn());
    }

    public void CloseSettingsMenu()
    {
        StartCoroutine(FadeOut());
    }

    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }

    public void ResumeGame()
    {
        CloseSettingsMenu();
    }

    public void ExitToMenu()
    {
        if (fadeManager != null)
        {
            fadeManager.FadeToScene("MainMenu"); // Assuming your main menu scene is named "MainMenu"
        }
    }

    private IEnumerator FadeIn()
    {
        settingsMenu.SetActive(true);
        fadeCanvasGroup.blocksRaycasts = true;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration * 0.5f); // Slightly darken the screen
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsedTime / fadeDuration * 0.5f);
            yield return null;
        }

        fadeCanvasGroup.blocksRaycasts = false;
        settingsMenu.SetActive(false);
    }
}
