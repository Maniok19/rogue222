using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Input Action")]
    public InputActionProperty interactAction;

    [Header("Detection Settings")]
    public float interactionRange = 1.2f;
    public LayerMask interactableLayer; // Assign your "Interactable" layer here

    private PlayerController playerController;
    private Transform currentNPC; // Track the active NPC we are speaking with

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable() => interactAction.action?.Enable();
    private void OnDisable() => interactAction.action?.Disable();

    private void Update()
    {
        if (interactAction.action != null && interactAction.action.WasPressedThisFrame())
        {
            TryInteract();
        }

        // Auto-close dialogue if player walks too far away from the active NPC [1]
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive() && currentNPC != null)
        {
            float distance = Vector2.Distance(transform.position, currentNPC.position);
            if (distance > interactionRange)
            {
                DialogueManager.Instance.EndDialogue();
                currentNPC = null; // Stop tracking since dialogue ended [1]
            }
        }
        else if (DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive())
        {
            // Reset tracking if the dialogue was ended normally by finishing the text
            currentNPC = null;
        }
    }

    private void TryInteract()
    {
        // 1. If dialogue is currently active, advance the text instead of searching for a new NPC
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
        {
            DialogueManager.Instance.DisplayNextSentence();
            return;
        }

        // 2. Otherwise, check if there is an NPC nearby
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayer);
        
        foreach (Collider2D collider in hitColliders)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
                currentNPC = collider.transform; // Store the reference to this NPC [1]
                break; // Interact with only the closest one
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}