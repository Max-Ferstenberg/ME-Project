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

    // Fields for left, center, and right images
    public Sprite leftImage;
    public bool isLeftImageVisible;
    public bool isLeftImageTalking;
    public bool shouldLeftImageFadeIn;
    public bool shouldLeftImageFadeOut;
    public bool isLeftImageMirrored; // New field to indicate if the left image should be mirrored

    public Sprite centerImage;
    public bool isCenterImageVisible;
    public bool isCenterImageTalking;
    public bool shouldCenterImageFadeIn;
    public bool shouldCenterImageFadeOut;
    public bool isCenterImageMirrored; // New field to indicate if the center image should be mirrored

    public Sprite rightImage;
    public bool isRightImageVisible;
    public bool isRightImageTalking;
    public bool shouldRightImageFadeIn;
    public bool shouldRightImageFadeOut;
    public bool isRightImageMirrored; // New field to indicate if the right image should be mirrored

    public bool hasResponses => responseIDs != null && responseIDs.Length > 0; // Determine if the dialogue has responses
}
