using TMPro;
using UnityEngine;

public class DialogueResponseButton : MonoBehaviour
{
    public TextMeshProUGUI responseText;

    public void SetResponseText(string text)
    {
        responseText.text = text;
        AdjustFontSize();
    }

    private void AdjustFontSize()
    {
        responseText.enableAutoSizing = true;
        responseText.fontSizeMin = 48;  // Minimum font size
        responseText.fontSizeMax = 100;  // Maximum font size
    }
}
