using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 1f; 
    private float invincibilityTimer;

    [Header("UI Reference")]
    public TMP_Text hpText; 

    [Header("Juice Visuals")]
    public SpriteRenderer spriteRenderer;  // Drag your player's SpriteRenderer here [3]
    public Color flashColor = Color.red;   // Color the player flashes when hit [3]

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHPUI();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
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
        
        if (currentHealth < 0) 
            currentHealth = 0;

        Debug.Log($"Player took {amount} damage! Remaining Health: {currentHealth}");

        UpdateHPUI();

        // 1. Trigger the red flash and transparent blink cycle [3]
        StartCoroutine(DamageFlashRoutine());

        // 2. Trigger the camera shake (Duration, Magnitude)
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.15f, 0.15f); 
        }

        invincibilityTimer = invincibilityDuration;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = Color.white; 

        // Phase 1: Solid Flash (The instant shock of the impact) [3]
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.1f); // Hold flash for a split second

        // Phase 2: Rapid Blinking (Represents invincibility state / i-frames) [3]
        float elapsed = 0.1f;
        while (elapsed < invincibilityDuration)
        {
            // Set to semi-transparent [3]
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.25f);
            yield return new WaitForSeconds(0.06f);
            elapsed += 0.06f;

            // Set back to opaque [3]
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.06f);
            elapsed += 0.06f;
        }

        // Ensure player is fully visible when invincibility ends
        spriteRenderer.color = originalColor;
    }

    private void UpdateHPUI()
    {
        if (hpText != null)
        {
            hpText.text = $"HP: {currentHealth} / {maxHealth}";
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
    }
}