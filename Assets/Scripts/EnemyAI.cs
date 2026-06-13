using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detectionRange = 6f; 

    [Header("Wandering Settings")]
    public float wanderSpeed = 1.5f;       // Slower, relaxed patrol speed [5]
    public float minWanderTime = 1f;       // Minimum time spent walking [5]
    public float maxWanderTime = 2.5f;     // Maximum time spent walking [5]
    public float minWaitTime = 1f;         // Minimum time spent standing still [5]
    public float maxWaitTime = 3f;         // Maximum time spent standing still [5]

    [Header("Combat")]
    public float damageAmount = 10f;
    public float attackCooldown = 1.5f; 

    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite frontSprite;
    public Sprite backSprite;
    public Sprite sideSprite;

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

    private Transform playerTransform;
    private Rigidbody2D rb;
    private float attackTimer;
    private float knockbackTimer;
    private float bounceTimer;
    private Vector2 currentMoveIntent; 

    // Wandering state variables [5]
    private bool isWandering;
    private float wanderTimer;
    private Vector2 wanderDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && frontSprite != null)
            spriteRenderer.sprite = frontSprite;

        originalScaleX = transform.localScale.x;
        originalScaleY = transform.localScale.y;
        targetScaleX = originalScaleX;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        HandleSpriteTransitions(currentMoveIntent);
        SmoothFlipAndJuice(currentMoveIntent);
    }

    private void FixedUpdate()
    {
        // 1. If currently knocked back, decay velocity and skip AI chase/wander [1]
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 8f);
            currentMoveIntent = Vector2.zero; 
            return; 
        }

        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            // Reset wandering state so when they lose the player, they pause before patrolling again
            isWandering = false;
            wanderTimer = 0f;

            // Chase the player
            Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
            currentMoveIntent = direction;
        }
        else
        {
            // Wander randomly [5]
            HandleWander();
        }
    }

    private void HandleWander()
    {
        wanderTimer -= Time.fixedDeltaTime;

        if (wanderTimer <= 0f)
        {
            // Toggle between walking and resting [5]
            isWandering = !isWandering;

            if (isWandering)
            {
                // Pick a random direction in radians, then convert to a direction vector [5]
                float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                wanderDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized;
                
                // Assign a random walking duration [5]
                wanderTimer = Random.Range(minWanderTime, maxWanderTime);
            }
            else
            {
                // Stand still and assign a random resting duration [5]
                wanderDirection = Vector2.zero;
                wanderTimer = Random.Range(minWaitTime, maxWaitTime);
            }
        }

        // Apply wandering movement [5]
        if (isWandering)
        {
            rb.linearVelocity = wanderDirection * wanderSpeed;
            currentMoveIntent = wanderDirection;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            currentMoveIntent = Vector2.zero;
        }
    }

    private void HandleSpriteTransitions(Vector2 moveInput)
    {
        if (spriteRenderer == null) return;

        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            if (sideSprite != null)
                spriteRenderer.sprite = sideSprite;

            if (moveInput.x < 0)
            {
                targetScaleX = -originalScaleX; 
            }
            else
            {
                targetScaleX = originalScaleX;  
            }
        }
        else if (Mathf.Abs(moveInput.y) > 0.1f)
        {
            if (moveInput.y > 0.1f)
            {
                if (backSprite != null)
                    spriteRenderer.sprite = backSprite;
            }
            else
            {
                if (frontSprite != null)
                    spriteRenderer.sprite = frontSprite;
            }
            
            targetScaleX = originalScaleX;
        }
    }

    private void SmoothFlipAndJuice(Vector2 moveInput)
    {
        Vector3 localScale = transform.localScale;

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

        float squashY = originalScaleY + (Mathf.Sin(bounceTimer) * currentAmount * originalScaleY);
        localScale.y = squashY;

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetTilt);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, turnSpeed * Time.deltaTime);

        localScale.x = Mathf.MoveTowards(localScale.x, targetScaleX, turnSpeed * Time.deltaTime);
        transform.localScale = localScale;
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        knockbackTimer = duration;
        rb.linearVelocity = direction * force; 
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && attackTimer <= 0)
        {
            IDamageable playerHealth = collision.gameObject.GetComponent<IDamageable>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                attackTimer = attackCooldown; 
            }
        }
    }
}