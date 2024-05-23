using System.Linq;
using UnityEngine;

public class DialogueDatabase : MonoBehaviour
{
    public Dialogue[] dialogues; // Array of all dialogues
    public Response[] responses; // Array of all responses

    public Dialogue GetDialogueById(int id)
    {
        return dialogues.FirstOrDefault(d => d.id == id);
    }

    public Response GetResponseById(int id)
    {
        return responses.FirstOrDefault(r => r.id == id);
    }
}
