using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneSettingsManager : MonoBehaviour
{
    public GameObject settingsMenu;
    public GameObject settingsManager;
    public Button settingsButton;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;
    public AudioSource backgroundMusic;
    public AudioSource buttonSFX;
    public AudioSource nextbuttonSFX;
    private FadeManager fadeManager;

    private void Awake()
    {
        Debug.Log("SceneSettingsManager Awake called.");

        if (settingsMenu == null)
        {
            Debug.LogError("settingsMenu is not assigned in the Inspector.");
        }
        else
        {
            settingsMenu.SetActive(false);
        }

        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f); // Initialize slider value from PlayerPrefs
        }
        else
        {
            Debug.LogError("bgmVolumeSlider is not assigned in the Inspector.");
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f); // Initialize slider value from PlayerPrefs
        }
        else
        {
            Debug.LogError("sfxVolumeSlider is not assigned in the Inspector.");
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

        // Set the volume to the saved value at the start
        backgroundMusic.volume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        buttonSFX.volume = sfxVolume;
        nextbuttonSFX.volume = sfxVolume;
    }

    public void OpenSettingsMenu()
    {
        settingsMenu.SetActive(true);
    }

    public void CloseSettingsMenu()
    {
        settingsMenu.SetActive(false);
    }

    public void OnBGMVolumeChanged(float value)
    {
        backgroundMusic.volume = value;
        PlayerPrefs.SetFloat("BGMVolume", value); // Save the BGM volume value to PlayerPrefs
    }

    public void OnSFXVolumeChanged(float value)
    {
        buttonSFX.volume = value;
        nextbuttonSFX.volume = value;
        PlayerPrefs.SetFloat("SFXVolume", value); // Save the SFX volume value to PlayerPrefs
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
        else
        {
            Debug.LogError("Cannot exit to menu. FadeManager is null.");
        }
    }
}
