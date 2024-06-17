using UnityEngine;

public class GlobalVolumeManager : MonoBehaviour
{
    public static GlobalVolumeManager Instance { get; private set; }
    private const string VolumeKey = "globalVolume";

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

        LoadVolume();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumeKey, volume);
    }

    public float GetVolume()
    {
        return PlayerPrefs.GetFloat(VolumeKey, 1f); // Default volume is 1
    }

    private void LoadVolume()
    {
        AudioListener.volume = GetVolume();
    }
}
