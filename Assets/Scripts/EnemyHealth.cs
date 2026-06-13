using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 30f;
    private float currentHealth;

    public GameObject deathFXPrefab; // Optional: Drag your WoodShatterFX here for basic particles

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage! Enemy Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
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