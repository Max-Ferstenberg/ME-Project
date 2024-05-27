using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISortingManager : MonoBehaviour
{
    public Canvas mainCanvas;
    public Image leftImageComponent;
    public Image centerImageComponent;
    public Image rightImageComponent;
    public TMP_Text dialogueText;
    public GameObject responsePanel;
    public Button nextButton;

    void Start()
    {
        if (mainCanvas != null)
        {
            mainCanvas.overrideSorting = true;
            mainCanvas.sortingOrder = 0; // Base layer for the main canvas
        }

        // Set sorting orders for background images
        SetSortingOrder(leftImageComponent.gameObject, "Background", 0);
        SetSortingOrder(centerImageComponent.gameObject, "Background", 0);
        SetSortingOrder(rightImageComponent.gameObject, "Background", 0);

        // Set sorting orders for UI elements
        SetSortingOrder(dialogueText.gameObject, "Foreground", 1);
        SetSortingOrder(responsePanel, "Foreground", 1);
        SetSortingOrder(nextButton.gameObject, "Foreground", 1);
    }

    void SetSortingOrder(GameObject obj, string layerName, int order)
    {
        Canvas canvas = obj.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = obj.AddComponent<Canvas>();
        }
        canvas.overrideSorting = true;
        canvas.sortingLayerName = layerName;
        canvas.sortingOrder = order;

        // Ensure the graphic raycaster is enabled for UI elements
        GraphicRaycaster raycaster = obj.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = obj.AddComponent<GraphicRaycaster>();
        }
        raycaster.enabled = true;
    }
}

