using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public GameObject settingsMenu;
    public Slider volumeSlider;
    public AudioSource backgroundMusic;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.5f;

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
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
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
        FadeManager.Instance.FadeToScene("Menu"); // Assuming your main menu scene is named "MainMenu"
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
