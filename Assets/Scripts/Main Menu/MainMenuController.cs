using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    // Method to be called when the Play button is pressed
    public void PlayGame()
    {
        SceneManager.LoadScene("Menu"); // Replace "Menu" with the actual name of the scene you want to load
    }

    public void GoToCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    // Method to be called when the Exit button is pressed
    public void ExitGame()
    {
        Application.Quit();
    }
}
