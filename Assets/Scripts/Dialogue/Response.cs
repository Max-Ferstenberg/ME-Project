using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Response
{
    public int id; // Unique identifier for each response
    public string text; // The response text
    public int nextDialogueID; // ID of the next dialogue after the response is chosen
    public string category; // Category of the response ("A" or "B")
    public bool isSelected; // Tracks if the response has been selected
}
