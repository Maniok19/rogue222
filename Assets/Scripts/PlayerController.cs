using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Vector2 LastMoveDirection { get; private set; } = Vector2.down; // Defaults to facing down/front
    [Header("Movement")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite frontSprite;
    public Sprite sideSprite;
    public Sprite backSprite; // Added field for the back sprite

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
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null && frontSprite != null)
            spriteRenderer.sprite = frontSprite;

        originalScaleX = transform.localScale.x;
        originalScaleY = transform.localScale.y;
        targetScaleX = originalScaleX;
    }

    void Update()
    {
        moveInput = Vector2.zero;

        if (moveAction.action != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }

        if (moveInput.sqrMagnitude > 0.01f)
        {
            LastMoveDirection = moveInput.normalized;
        }

        HandleSpriteTransitions();
        SmoothFlipAndJuice();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed; 
    }

    void HandleSpriteTransitions()
    {
        // 1. Moving Horizontally takes priority (switches to side sprite)
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            if (sideSprite != null)
                spriteRenderer.sprite = sideSprite;

            if (moveInput.x < 0)
            {
                targetScaleX = originalScaleX; // Face Left
            }
            else
            {
                targetScaleX = -originalScaleX;  // Face Right
            }
        }
        // 2. Moving Vertically (without horizontal movement)
        else if (Mathf.Abs(moveInput.y) > 0.1f)
        {
            if (moveInput.y > 0.1f)
            {
                // Moving Upwards -> Set to Back Sprite
                if (backSprite != null)
                    spriteRenderer.sprite = backSprite;
            }
            else
            {
                // Moving Downwards -> Set to Front Sprite
                if (frontSprite != null)
                    spriteRenderer.sprite = frontSprite;
            }
            
            targetScaleX = originalScaleX;
        }
        // 3. If stationary (idle), we do nothing. The character stays on their last sprite.
    }

    void SmoothFlipAndJuice()
    {
        Vector3 localScale = transform.localScale;

        float currentSpeed;
        float currentAmount;
        float targetTilt;

        if (moveInput.magnitude > 0.1f)
        {
            // Walking State values
            currentSpeed = bounceSpeed;
            currentAmount = bounceAmount;
            
            // Calculate dynamic step tilt
            targetTilt = Mathf.Sin(bounceTimer) * tiltAmount;
        }
        else
        {
            // Idle Breathing State values
            currentSpeed = idleBounceSpeed;
            currentAmount = idleBounceAmount;
            
            // Reset tilt to upright position when standing still
            targetTilt = 0f;
        }

        // Keep the sine wave timer running
        bounceTimer += Time.deltaTime * currentSpeed;

        // Apply vertical squash & stretch (handling walk or breath seamlessly)
        float squashY = originalScaleY + (Mathf.Sin(bounceTimer) * currentAmount * originalScaleY);
        localScale.y = squashY;

        // Apply smooth tilt interpolation so it doesn't snap when stopping
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetTilt);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, turnSpeed * Time.deltaTime);

        // Apply horizontal turning scale
        localScale.x = Mathf.MoveTowards(localScale.x, targetScaleX, turnSpeed * Time.deltaTime);
        transform.localScale = localScale;
    }
}