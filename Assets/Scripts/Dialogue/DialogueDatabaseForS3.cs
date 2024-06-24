using System.Linq;
using UnityEngine;

public class DialogueDatabaseForS3 : MonoBehaviour
{
    public DialogueForS3[] dialogues; // Array of all dialogues
    public Response[] responses; // Array of all responses

    public DialogueForS3 GetDialogueById(int id)
    {
        return dialogues.FirstOrDefault(d => d.id == id);
    }

    public Response GetResponseById(int id)
    {
        return responses.FirstOrDefault(r => r.id == id);
    }
}
