using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneSettingsManager : MonoBehaviour
{
    public GameObject settingsMenu;
    public Slider volumeSlider;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.5f;
    private FadeManager fadeManager;

    private void Start()
    {
        settingsMenu.SetActive(false);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        volumeSlider.value = GlobalVolumeManager.Instance.GetVolume(); // Initialize slider value
        fadeManager = FindObjectOfType<FadeManager>(); // Get the FadeManager in the current scene
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
        GlobalVolumeManager.Instance.SetVolume(value);
    }

    public void ResumeGame()
    {
        CloseSettingsMenu();
    }

    public void ExitToMenu()
    {
        fadeManager.FadeToScene("MainMenu"); // Assuming your main menu scene is named "MainMenu"
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
