using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    public AudioSource buttonPressSFX;

    public void PlayButtonPressSound()
    {
        buttonPressSFX.Play();
    }
}
