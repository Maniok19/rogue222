using UnityEngine;

public class NPCDialogue : MonoBehaviour, IInteractable
{
    [Header("NPC Profile")]
    public string npcName = "Grandpa";

    [TextArea(3, 10)] // Gives you a nice box in the inspector to type paragraphs
    public string[] dialogueLines;

    public void Interact()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(npcName, dialogueLines);
        }
    }
}