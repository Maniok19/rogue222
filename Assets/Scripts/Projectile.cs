using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;
    private float knockbackForce;
    private float knockbackDuration;
    private Vector2 direction;
    private float speed;
    private Rigidbody2D rb;

    [Header("Projectile Settings")]
    [Tooltip("Durée de vie maximale de la flèche (en secondes) avant qu'elle ne disparaisse d'elle-même.")]
    [SerializeField] private float maxLifetime = 3f;

    [Header("Blob's Eye Upgrade Settings")]
    [SerializeField] private LayerMask enemyLayer;      
    [SerializeField] private float homingRadius = 6f;    
    [SerializeField] private float homingForce = 12f;     
    [Tooltip("Angle maximal de détection (en degrés) devant la flèche. Un angle de 45° crée un cône total de 90° devant.")]
    [SerializeField] private float homingAngle = 45f;    // <-- NOUVELLE VARIABLE MODIFIABLE
    [SerializeField] private GameObject trailPrefab;     

    private bool hasBlobsEye = false;
    private Transform currentTarget;

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
            rb.linearVelocity = direction * speed;
        }

        if (CharmInventory.Instance != null)
        {
            hasBlobsEye = CharmInventory.Instance.HasCharm(CharmType.BlobsEye);
        }

        UpdateRotation();
        Destroy(gameObject, maxLifetime);
    }

    private void FixedUpdate()
    {
        if (hasBlobsEye && rb != null)
        {
            FindTarget();
            
            // --- SÉCURITÉ DE VERROUILLAGE ---
            // Si on a déjà une cible, on s'assure qu'elle est TOUJOURS dans notre angle et notre portée.
            // Si la cible s'est déplacée derrière nous ou trop loin, la flèche perd son verrouillage.
            if (currentTarget != null)
            {
                Vector2 currentDir = rb.linearVelocity.normalized;
                Vector2 dirToTarget = (currentTarget.position - transform.position).normalized;
                float angle = Vector2.Angle(currentDir, dirToTarget);

                if (angle > homingAngle || Vector2.Distance(transform.position, currentTarget.position) > homingRadius)
                {
                    currentTarget = null; // Perte de cible !
                }
            }

            ApplyHomingSteering();
        }
    }

    private void FindTarget()
    {
        if (currentTarget != null && currentTarget.gameObject.activeInHierarchy) return;

        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(transform.position, homingRadius, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestTransform = null;

        // Détermine la direction actuelle de vol de la flèche
        Vector2 currentDir = direction;
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            currentDir = rb.linearVelocity.normalized;
        }

        foreach (var col in potentialTargets)
        {
            // 1. Filtre par Tag (uniquement les ennemis)
            if (!col.CompareTag("Enemy") && !col.CompareTag("Enemie")) continue;

            // 2. FILTRE PAR CÔNE DE VISION
            // On calcule l'angle entre la direction de vol de la flèche et la direction de l'ennemi
            Vector2 dirToTarget = (col.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(currentDir, dirToTarget);

            // Si l'ennemi n'est pas dans le cône devant la flèche, on l'ignore
            if (angle > homingAngle) continue; 

            float dist = Vector2.Distance(transform.position, col.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestTransform = col.transform;
            }
        }

        currentTarget = closestTransform;
    }

    private void ApplyHomingSteering()
    {
        // S'il n'y a aucune cible valide dans le cône, la flèche va tout droit
        if (currentTarget == null) return; 

        Vector2 targetDirection = (currentTarget.position - transform.position).normalized;
        
        Vector2 currentVelocity = rb.linearVelocity;
        Vector2 steer = (targetDirection * speed) - currentVelocity;

        rb.linearVelocity += steer * homingForce * Time.fixedDeltaTime;

        UpdateRotation();
    }

    private void UpdateRotation()
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Projectile")) return;
        if (other.isTrigger) return;

        if (hasBlobsEye && trailPrefab != null)
        {
            Instantiate(trailPrefab, transform.position, Quaternion.identity);
        }

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);

            EnemyAI enemyAI = other.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.ApplyKnockback(rb.linearVelocity.normalized, knockbackForce, knockbackDuration);
            }

            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- VISUALISATION DU CÔNE DE DÉTECTION ---
    // Cette fonction dessine le cône de visée en jaune directement dans votre éditeur de scène
    private void OnDrawGizmosSelected()
    {
        if (hasBlobsEye)
        {
            // Dessine la portée maximale en bleu/cyan
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, homingRadius);

            // Calcule la direction actuelle
            Vector2 currentDir = transform.right; 
            if (rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                currentDir = rb.linearVelocity.normalized;
            }
            else if (direction.sqrMagnitude > 0.1f)
            {
                currentDir = direction;
            }

            // Calcule les limites gauche et droite du cône
            Vector3 leftLimit = Quaternion.AngleAxis(-homingAngle, Vector3.forward) * currentDir * homingRadius;
            Vector3 rightLimit = Quaternion.AngleAxis(homingAngle, Vector3.forward) * currentDir * homingRadius;

            // Dessine les lignes du cône de détection en jaune
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + leftLimit);
            Gizmos.DrawLine(transform.position, transform.position + rightLimit);
        }
    }
}