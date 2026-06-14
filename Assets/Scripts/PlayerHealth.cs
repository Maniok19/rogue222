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
    public Color flashColor = Color.red;   // Color the player flashes when hit

    // Automatically caches all puppet parts (head, body, limbs)
    private SpriteRenderer[] childRenderers; 

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHPUI();

        // 1. Find all sprite renderers specifically under player_rig to avoid flashing weapons
        Transform rig = transform.Find("player_rig");
        if (rig != null)
        {
            childRenderers = rig.GetComponentsInChildren<SpriteRenderer>();
        }
        else
        {
            // Fallback if rig is missing
            childRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
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

        // Trigger the flash on all cached parts
        StartCoroutine(DamageFlashRoutine());

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
        if (childRenderers == null || childRenderers.Length == 0) yield break;

        Color originalColor = Color.white; 

        // Phase 1: Solid Flash (Turn all puppet parts solid red)
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

    // Helper method to update color across all puppet parts at once
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