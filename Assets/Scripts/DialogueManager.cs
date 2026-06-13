using System.Collections.Generic;
using UnityEngine;
using TMPro; // Ensure TextMeshPro is installed in your project

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel; // The visual UI box containing the text
    public TMP_Text nameText;        // Text component displaying the NPC's name
    public TMP_Text dialogueText;    // Text component displaying the lines

    private Queue<string> sentences;
    private bool isDialogueActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        sentences = new Queue<string>();

        // Start with the dialogue UI hidden
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public void StartDialogue(string npcName, string[] lines)
    {
        isDialogueActive = true;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (nameText != null)
            nameText.text = npcName;

        sentences.Clear();

        foreach (string line in lines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        
        // This is a direct swap. You can later add a typing effect here if desired.
        if (dialogueText != null)
            dialogueText.text = sentence;
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}