using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 30f;
    private float currentHealth;

    [Header("Juice Visuals")]
    public Color flashColor = Color.red;   // Color the enemy flashes when hit
    public float invincibilityDuration = 0.4f; // How long the enemy flashes/blinks when hit
    private float invincibilityTimer;

    [Header("Death Settings")]
    public GameObject deathFXPrefab; // Optional: Drag your WoodShatterFX here for basic particles

    // Automatically caches all sprite parts (supports single or multi-part setups)
    private SpriteRenderer[] childRenderers;
    private Coroutine flashCoroutine;

    private void Start()
    {
        currentHealth = maxHealth;

        // Find all SpriteRenderers on the enemy
        childRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(float amount)
    {
        if (invincibilityTimer > 0) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage! Enemy Health: {currentHealth}");

        // Stop any ongoing flash routine before starting a new one to prevent visual glitches
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(DamageFlashRoutine());

        invincibilityTimer = invincibilityDuration;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (childRenderers == null || childRenderers.Length == 0) yield break;

        Color originalColor = Color.white; 

        // Phase 1: Solid Flash (Turn all parts solid red)
        SetRenderersColor(flashColor);
        yield return new WaitForSeconds(0.1f); // Hold flash for a split second

        // Phase 2: Rapid Blinking (Blink all parts between transparent and opaque)
        float elapsed = 0.1f;
        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.25f);

        while (elapsed < invincibilityDuration)
        {
            // Set all to semi-transparent
            SetRenderersColor(transparentColor);
            yield return new WaitForSeconds(0.06f);
            elapsed += 0.06f;

            // Set all back to opaque
            SetRenderersColor(originalColor);
            yield return new WaitForSeconds(0.06f);
            elapsed += 0.06f;
        }

        // Ensure all parts are fully visible when invincibility ends
        SetRenderersColor(originalColor);
    }

    // Helper method to update color across all cached renderers at once
    private void SetRenderersColor(Color color)
    {
        if (childRenderers == null) return;
        
        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i] != null)
            {
                childRenderers[i].color = color;
            }
        }
    }

    private void Die()
    {
        if (deathFXPrefab != null)
        {
            Instantiate(deathFXPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}