using TMPro;
using UnityEngine;

public class DialogueResponseButton : MonoBehaviour
{
    public TextMeshProUGUI responseText;

    public void SetResponseText(string text)
    {
        responseText.text = text;
    }
}
