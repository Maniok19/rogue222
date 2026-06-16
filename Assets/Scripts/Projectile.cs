using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;
    private float knockbackForce;
    private float knockbackDuration;
    private Vector2 direction;
    private float speed;
    private Rigidbody2D rb;

    public void Setup(Vector2 dir, float dmg, float kbForce, float kbDuration, float projSpeed)
    {
        direction = dir.normalized;
        damage = dmg;
        knockbackForce = kbForce;
        knockbackDuration = kbDuration;
        speed = projSpeed;

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Attribution de la vitesse physique
            rb.linearVelocity = direction * speed;
        }

        // Oriente le projectile pour qu'il regarde vers sa direction de vol
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Détruit le projectile après 5 secondes s'il ne touche rien (évite les fuites de mémoire)
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // On ignore le joueur
        if (other.CompareTag("Player")) return;

        // Tente d'appliquer des dégâts
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);

            // Tente d'appliquer du recul si c'est un ennemi
            EnemyAI enemyAI = other.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.ApplyKnockback(direction, knockbackForce, knockbackDuration);
            }

            // Détruit le projectile après impact
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles") || other.CompareTag("Obstacle"))
        {
            // Détruit le projectile s'il touche un mur
            Destroy(gameObject);
        }
    }
}