using UnityEngine;

public class UIAutoScaler : MonoBehaviour
{
    [System.Serializable]
    public class UIElement
    {
        public RectTransform rectTransform;
        public Vector2 originalSize;
        public Vector2 originalPosition;
    }

    public UIElement[] uiElements;

    private Vector2 referenceResolution = new Vector2(1920, 1080); // Base resolution

    void Start()
    {
        // Automatically find all RectTransforms in children and add to uiElements
        RectTransform[] allRects = GetComponentsInChildren<RectTransform>();
        uiElements = new UIElement[allRects.Length];

        for (int i = 0; i < allRects.Length; i++)
        {
            uiElements[i] = new UIElement
            {
                rectTransform = allRects[i],
                originalSize = allRects[i].sizeDelta,
                originalPosition = allRects[i].anchoredPosition
            };
        }

        AdjustUI();
    }

    void AdjustUI()
    {
        float widthRatio = Screen.width / referenceResolution.x;
        float heightRatio = Screen.height / referenceResolution.y;

        foreach (var element in uiElements)
        {
            // Adjust size based on both width and height ratios
            element.rectTransform.sizeDelta = new Vector2(element.originalSize.x * widthRatio, element.originalSize.y * heightRatio);

            // Adjust position based on both width and height ratios
            element.rectTransform.anchoredPosition = new Vector2(element.originalPosition.x * widthRatio, element.originalPosition.y * heightRatio);
        }
    }

    void Update()
    {
        AdjustUI();
    }
}
