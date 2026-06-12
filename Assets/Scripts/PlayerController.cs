using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite frontSprite;
    public Sprite sideSprite;

    [Header("Smooth Turn Settings")]
    public float turnSpeed = 15f;
    
    private float originalScaleX;
    private float targetScaleX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null && frontSprite != null)
            spriteRenderer.sprite = frontSprite;

        originalScaleX = transform.localScale.x;
        targetScaleX = originalScaleX;
    }

    void Update()
    {
        moveInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1;
        }

        moveInput = moveInput.normalized;

        HandleSpriteTransitions();
        SmoothFlip();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed; 
    }

    void HandleSpriteTransitions()
    {
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            spriteRenderer.sprite = sideSprite;
            
            // Since the source sprite naturally faces RIGHT:
            if (moveInput.x < 0)
            {
                targetScaleX = -originalScaleX; // Flip to face Left
            }
            else
            {
                targetScaleX = originalScaleX;  // Keep normal to face Right
            }
        }
        else if (Mathf.Abs(moveInput.y) > 0.1f || moveInput == Vector2.zero)
        {
            spriteRenderer.sprite = frontSprite;
            targetScaleX = originalScaleX;
        }
    }

    void SmoothFlip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.MoveTowards(localScale.x, targetScaleX, turnSpeed * Time.deltaTime);
        transform.localScale = localScale;
    }
}