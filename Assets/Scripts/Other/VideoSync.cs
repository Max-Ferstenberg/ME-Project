using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class VideoSync : MonoBehaviour
{
    public VideoPlayer videoPlayer1;
    public VideoPlayer videoPlayer2;
    public TMP_Text unlockedCharacterText; // Reference to the TMP_Text component
    public Button backButton; // Reference to the back button
    public string characterName; // The name of the unlocked character
    public string mainMenuSceneName = "MainMenu"; // Name of the main menu scene

    public float typewriterSpeed = 0.01f; // Speed of the typewriter effect

    void Start()
    {
        // Ensure both Video Players are prepared before playing
        videoPlayer1.prepareCompleted += PrepareCompleted;
        videoPlayer2.prepareCompleted += PrepareCompleted;

        // Prepare the Video Players
        videoPlayer1.Prepare();
        videoPlayer2.Prepare();

        // Start the typewriter effect for the text
        StartCoroutine(TypeWriterEffect(characterName));

        // Assign the button's onClick event to the GoToMainMenu function
        backButton.onClick.AddListener(GoToMainMenu);
    }

    void PrepareCompleted(VideoPlayer vp)
    {
        // When both players are prepared, start playing
        if (videoPlayer1.isPrepared && videoPlayer2.isPrepared)
        {
            videoPlayer1.Play();
            videoPlayer2.Play();
        }
    }

    public void SetUnlockedCharacter(string characterName)
    {
        // Set the unlocked character text with the new format
        unlockedCharacterText.text = $"You have unlocked: {characterName}! Press next to return to the main menu and view new scenarios.";
    }

    IEnumerator TypeWriterEffect(string text)
    {
        string fullText = $"You have unlocked: {text}! Press next to return to the main menu and view new scenarios.";
        unlockedCharacterText.text = "";
        foreach (char letter in fullText.ToCharArray())
        {
            unlockedCharacterText.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        // Store the unlocked character name globally
        PlayerPrefs.SetString("UnlockedCharacter", text);
        PlayerPrefs.Save();
        Debug.Log($"Character name '{text}' saved to PlayerPrefs.");
    }

    public void GoToMainMenu()
    {
        // Trigger the fade out effect before loading the main menu scene
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeToScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("FadeManager instance is not found.");
            SceneManager.LoadScene(mainMenuSceneName); // Fallback to direct load if FadeManager is not available
        }
    }
}
