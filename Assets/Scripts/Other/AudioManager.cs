using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource backgroundMusic;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (!backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        if (backgroundMusic.isPlaying)
        {
            backgroundMusic.Stop();
        }
    }
}
