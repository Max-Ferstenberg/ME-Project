using System;
using UnityEngine;

[Serializable]
public class Dialogue
{
    public int id; // Unique identifier for each dialogue
    public string text; // The dialogue text
    public int[] responseIDs; // IDs of the response dialogues
    public int nextDialogueID; // ID of the next dialogue in the sequence
    public bool isResponse; // Flag to indicate if this dialogue is a response
    public Sprite characterImage; // Reference to the character's image
    public bool isPlayerCharacter; // Flag to indicate if this dialogue is from the player character
    public bool hasSprite; // Flag to indicate if this dialogue has a sprite

    public bool hasResponses => responseIDs != null && responseIDs.Length > 0; // Determine if the dialogue has responses
}
