using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Rig Reference")]
    public Transform rigTransform; 
    public Vector2 LastMoveDirection { get; private set; } = Vector2.down; // Raw direction
    public Vector2 SnappedMoveDirection { get; private set; } = Vector2.down; // Snapped direction for visuals

    [Header("Movement")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    [Header("Smooth Turn Settings")]
    public float turnSpeed = 15f;
    private float originalScaleX;
    private float originalScaleY;
    private float targetScaleX;

    [Header("Bouncy Walk Juice")]
    public float bounceSpeed = 14f;      
    [Range(0.05f, 0.3f)] 
    public float bounceAmount = 0.12f;   
    [Range(1f, 15f)] 
    public float tiltAmount = 8f;        

    [Header("Idle Breathing Juice")]
    public float idleBounceSpeed = 3f;      
    [Range(0.01f, 0.1f)] 
    public float idleBounceAmount = 0.03f;  

    private float bounceTimer;

    [Header("Input System Actions")]
    public InputActionProperty moveAction;

    void OnEnable()
    {
        moveAction.action?.Enable();
    }

    void OnDisable()
    {
        moveAction.action?.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rigTransform == null)
        {
            rigTransform = transform.Find("player_rig");
        }

        originalScaleX = rigTransform != null ? rigTransform.localScale.x : transform.localScale.x;
        originalScaleY = rigTransform != null ? rigTransform.localScale.y : transform.localScale.y;
        targetScaleX = originalScaleX;
    }

    void Update()
    {
        moveInput = Vector2.zero;

        if (moveAction.action != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }

        bool isWalking = moveInput.sqrMagnitude > 0.01f;

        if (isWalking)
        {
            LastMoveDirection = moveInput.normalized;
            SnappedMoveDirection = GetSnappedDirection(LastMoveDirection); // Snap the direction vector
        }

        // Send SNAPPED parameters to the Animator Controller to prevent double rendering
        if (animator != null)
        {
            animator.SetFloat("moveX", SnappedMoveDirection.x);
            animator.SetFloat("moveY", SnappedMoveDirection.y);
            animator.SetBool("isWalking", isWalking);
        }

        HandleFlipping();
        SmoothFlipAndJuice();
    }

    void FixedUpdate()
    {
        // Physics movement remains smooth and diagonal
        rb.linearVelocity = moveInput * moveSpeed; 
    }

    // Helper to snap diagonal vectors like (0.7, 0.7) to clean (0, 1) or (1, 0) cardinal vectors
    private Vector2 GetSnappedDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.01f) return Vector2.down;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return new Vector2(Mathf.Sign(dir.x), 0f); // Snap to Left/Right
        }
        else
        {
            return new Vector2(0f, Mathf.Sign(dir.y)); // Snap to Up/Down
        }
    }

    void HandleFlipping()
    {
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            if (moveInput.x < 0)
            {
                targetScaleX = -originalScaleX; // Face à Gauche
            }
            else
            {
                targetScaleX = originalScaleX;  // Face à Droite
            }
        }
        else if (Mathf.Abs(moveInput.y) > 0.1f)
        {
            // Réinitialise l'orientation par défaut quand on va vers le haut/bas
            targetScaleX = originalScaleX; 
        }

        // --- CORRECTION : On applique l'échelle ici, en dehors du "if/else", 
        // pour qu'elle s'actualise AUSSI lors des déplacements verticaux. ---
        if (rigTransform != null)
        {
            Vector3 rigScale = rigTransform.localScale;
            rigScale.x = targetScaleX;
            rigTransform.localScale = rigScale;
        }
    }

    void SmoothFlipAndJuice()
    {
        float currentSpeed;
        float currentAmount;
        float targetTilt;

        if (moveInput.magnitude > 0.1f)
        {
            currentSpeed = bounceSpeed;
            currentAmount = bounceAmount;
            targetTilt = Mathf.Sin(bounceTimer) * tiltAmount;
        }
        else
        {
            currentSpeed = idleBounceSpeed;
            currentAmount = idleBounceAmount;
            targetTilt = 0f;
        }

        bounceTimer += Time.deltaTime * currentSpeed;

        if (rigTransform != null)
        {
            Vector3 rigScale = rigTransform.localScale;
            rigScale.y = originalScaleY + (Mathf.Sin(bounceTimer) * currentAmount * originalScaleY);
            rigTransform.localScale = rigScale;
        }

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetTilt);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, turnSpeed * Time.deltaTime);
    }
}