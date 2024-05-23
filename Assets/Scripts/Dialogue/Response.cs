using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Response
{
    public int id; // Unique identifier for each response
    public string text; // The response text
    public int nextDialogueID; // ID of the next dialogue after the response is chosen
}

