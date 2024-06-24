using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }
    public Image fadeImage;
    public float fadeDuration = 1f;
    private Canvas fadeCanvas;
    private CanvasGroup fadeCanvasGroup;

    private void Awake()
    {
        Instance = this;
        SetupFadeCanvas();

        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 1);
        }
        else
        {
            Debug.LogError("Fade image is not assigned in the FadeManager.");
        }
    }

    private void SetupFadeCanvas()
    {
        fadeCanvas = fadeImage.GetComponentInParent<Canvas>();
        fadeCanvasGroup = fadeImage.GetComponentInParent<CanvasGroup>();
        if (fadeCanvas != null)
        {
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 1000; // High sorting order to ensure it appears on top
        }
        else
        {
            Debug.LogError("Fade image's parent Canvas is not found.");
        }

        if (fadeCanvasGroup == null)
        {
            fadeCanvasGroup = fadeImage.gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        if (fadeImage != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1 - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            fadeCanvasGroup.alpha = alpha;
            yield return null;
        }
        fadeCanvasGroup.blocksRaycasts = false;
    }

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        fadeCanvasGroup.blocksRaycasts = true;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            fadeCanvasGroup.alpha = alpha;
            yield return null;
        }

        Debug.Log("Loading scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(0.1f); // Ensure the scene has time to load
        StartCoroutine(FadeIn());
    }
}
