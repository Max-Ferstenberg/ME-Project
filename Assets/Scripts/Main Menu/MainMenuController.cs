using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Menu");
    }

    public void GoToCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void GoToDisclaimer(){
        SceneManager.LoadScene("Disclaimer");
    }

    public void GoToUserGuide(){
        SceneManager.LoadScene("UserGuide");
    }

}
