using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Rig Reference")]
    public Transform rigTransform; 
    public Vector2 LastMoveDirection { get; private set; } = Vector2.down; // Defaults to facing down/front
    [Header("Movement")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator; // Added Animator reference

    [Header("Smooth Turn Settings")]
    public float turnSpeed = 15f;
    private float originalScaleX;
    private float originalScaleY;
    private float targetScaleX;

    [Header("Bouncy Walk Juice")]
    public float bounceSpeed = 14f;      // Speed of the step cycle
    [Range(0.05f, 0.3f)] 
    public float bounceAmount = 0.12f;   // Scale stretch when walking
    [Range(1f, 15f)] 
    public float tiltAmount = 8f;        // Tilting angle when walking

    [Header("Idle Breathing Juice")]
    public float idleBounceSpeed = 3f;      // Slower speed for breathing
    [Range(0.01f, 0.1f)] 
    public float idleBounceAmount = 0.03f;  // Subtle scale stretch for breathing

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
        }

        // Send parameters to the Animator Controller
        if (animator != null)
        {
            animator.SetFloat("moveX", LastMoveDirection.x);
            animator.SetFloat("moveY", LastMoveDirection.y);
            animator.SetBool("isWalking", isWalking);
        }

        HandleFlipping();
        SmoothFlipAndJuice();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed; 
    }

    void HandleFlipping()
    {
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            if (moveInput.x < 0)
            {
                targetScaleX = -originalScaleX; // Face Left
            }
            else
            {
                targetScaleX = originalScaleX;  // Face Right
            }

            // Apply scale instantly to prevent near-zero matrix collapses
            if (rigTransform != null)
            {
                Vector3 rigScale = rigTransform.localScale;
                rigScale.x = targetScaleX;
                rigTransform.localScale = rigScale;
            }
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

        // Apply squash and stretch safely to the rig instead of root physics
        if (rigTransform != null)
        {
            Vector3 rigScale = rigTransform.localScale;
            rigScale.y = originalScaleY + (Mathf.Sin(bounceTimer) * currentAmount * originalScaleY);
            rigTransform.localScale = rigScale;
        }

        // Apply smooth tilt interpolation
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetTilt);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, turnSpeed * Time.deltaTime);
    }
}