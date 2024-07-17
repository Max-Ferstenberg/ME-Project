using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

public class ImageToPNG : MonoBehaviour
{
    public Image uiImage; // Assign the UI Image element in the Inspector

    void Start()
    {
        // Ensure the UI Image has a proper reference
        if (uiImage == null)
        {
            Debug.LogError("UI Image reference is missing.");
            return;
        }

        // Capture the UI Image and save it as a PNG
        StartCoroutine(CaptureAndSaveImage());
    }

    private IEnumerator CaptureAndSaveImage()
    {
        // Create a new RenderTexture
        RenderTexture renderTexture = new RenderTexture(1024, 1024, 24);
        // Create a new Texture2D
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        // Set up the camera to render the UI Image
        Camera uiCamera = new GameObject("UICamera").AddComponent<Camera>();
        uiCamera.orthographic = true;
        uiCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
        uiCamera.targetTexture = renderTexture;

        // Temporarily set the UI Image's Canvas to render to the camera
        Canvas canvas = uiImage.canvas;
        RenderMode originalRenderMode = canvas.renderMode;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = uiCamera;

        // Wait for end of frame to capture the rendered UI
        yield return new WaitForEndOfFrame();

        // Render the UI Image
        uiCamera.Render();

        // Read the RenderTexture to Texture2D
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        // Reset the Canvas render mode
        canvas.renderMode = originalRenderMode;

        // Save the Texture2D as a PNG file
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/UI_Image.png", bytes);
        Debug.Log("Saved UI Image as PNG at: " + Application.dataPath + "/UI_Image.png");

        // Clean up
        RenderTexture.active = null;
        uiCamera.targetTexture = null;
        Destroy(renderTexture);
        Destroy(texture);
        Destroy(uiCamera.gameObject);
    }
}
