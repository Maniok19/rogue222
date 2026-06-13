using UnityEngine;

public class DestructibleBox : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 20f;
    private float currentHealth;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Sprite damagedSprite;   // Optional: Sprite with cracks
    public Sprite brokenSprite;    // Optional: Leftover debris sprite
    public GameObject breakEffect; // Optional: Particle system to spawn on break

    private Collider2D boxCollider;
    private bool isBroken = false;

    private void Start()
    {
        currentHealth = maxHealth;
        boxCollider = GetComponent<Collider2D>();
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // This is the method required by the IDamageable interface
    public void TakeDamage(float amount)
    {
        if (isBroken) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Health: {currentHealth}");

        // 1. Show damaged visual if health falls to 50% or below
        if (currentHealth <= maxHealth * 0.5f && damagedSprite != null && currentHealth > 0)
        {
            spriteRenderer.sprite = damagedSprite;
        }

        // 2. Check if destroyed
        if (currentHealth <= 0)
        {
            Break();
        }
    }

    private void Break()
    {
        isBroken = true;

        // Disable collision so the player can walk through the remains
        if (boxCollider != null)
            boxCollider.enabled = false;

        // Spawn debris particles if assigned
        if (breakEffect != null)
        {
            Instantiate(breakEffect, transform.position, Quaternion.identity);
        }

        if (brokenSprite != null)
        {
            // Swap to a flat "rubble/broken" sprite on the floor
            spriteRenderer.sprite = brokenSprite;
            // Push it behind the player's rendering order so they walk over it
            spriteRenderer.sortingOrder--; 
        }
        else
        {
            // If no debris sprite is assigned, remove the object entirely
            Destroy(gameObject);
        }
    }
}